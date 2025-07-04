using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using FTPcontentManager.Src.Attributes;
using FTPcontentManager.Src.Constants;
using FTPcontentManager.Src.Models;

namespace FTPcontentManager.Src.Extensions
{
	public static class StreamExtensions
	{
		private static readonly Type ModelInterface = typeof (IBinaryModel);

		public static byte[] ReadBytes(this Stream fs, int length)
		{
			var buffer = new byte[length];
			fs.ReadExactly(buffer, 0, length);
			return buffer;
		}

		public static ushort ReadShort(this Stream fs, bool isLowEndian = false)
		{
			var buffer = new byte[2];
			fs.ReadExactly(buffer, 0, 2);
			if (!isLowEndian) Array.Reverse(buffer);
			return BitConverter.ToUInt16(buffer, 0);
		}

		public static uint ReadInt24(this Stream fs, bool isLowEndian = false)
		{
			var buffer = new byte[3];
			fs.ReadExactly(buffer, 0, 3);
			if (!isLowEndian) Array.Reverse(buffer);
			return (uint)((buffer[0] << 16) + (buffer[1] << 8) + (buffer[2]));
		}

		public static uint ReadUInt(this Stream fs, bool isLowEndian = false)
		{
			var buffer = new byte[4];
			fs.ReadExactly(buffer, 0, 4);
			if (!isLowEndian) Array.Reverse(buffer);
			return BitConverter.ToUInt32(buffer, 0);
		}

		public static ulong ReadLong(this Stream fs, bool isLowEndian = false)
		{
			var buffer = new byte[8];
			fs.ReadExactly(buffer, 0, 8);
			if (!isLowEndian) Array.Reverse(buffer);
			return BitConverter.ToUInt64(buffer, 0);
		}

		public static double ReadDouble(this Stream fs)
		{
			var buffer = new byte[8];
			fs.ReadExactly(buffer, 0, 8);
			return BitConverter.ToDouble(buffer, 0);
		}

		public static float ReadFloat(this Stream fs)
		{
			var buffer = new byte[4];
			fs.ReadExactly(buffer, 0, 4);
			return BitConverter.ToSingle(buffer, 0);
		}

		public static string ReadString(this Stream fs, int length, Encoding encoding = null)
		{
			encoding = encoding ?? Encoding.ASCII;
			var buffer = new byte[length];
			fs.ReadExactly(buffer, 0, length);
			return encoding.GetString(buffer);
		}

		public static string ReadWString(this Stream fs, int? length = null, Encoding encoding = null)
		{
			encoding ??= Encoding.ASCII;
			var unlimited = false;
			if (!length.HasValue) {
				length = 64;
				unlimited = true;
			}
			var sb = new StringBuilder();
			int i;
			do {
				var buffer = new byte[length.Value];
				fs.ReadExactly(buffer, 0, length.Value);
				var encoded = encoding.GetChars(buffer);
				i = 0;
				while (i < encoded.Length && encoded[i] != 0) i++;
				sb.Append(encoding.GetString(encoding.GetBytes(encoded.Take(i).ToArray())));
			} while (i < length.Value && unlimited);
			return sb.ToString();
		}

		public static T ReadBlock<T>(this Stream stream, long? address = null)
		{
			return stream.ReadBlock<T, T>(address);
		}

		public static TData ReadBlock<TData,TDef>(this Stream stream, long? address = null)
		{
			return (TData) ReadBlock(stream, typeof (TData), typeof (TDef), null, address);
		}

		public static void ReadBlock(this Stream stream, object instance, long? address = null)
		{
			var type = instance.GetType();
			ReadBlock(stream, type, type, instance, address);
		}

		private static string _indent = String.Empty;

		private static object ReadBlock(Stream stream, Type dataType, Type defType, object instance, long? address)
		{
			if (address.HasValue) stream.Position = address.Value;
			instance = instance ?? CreateInstance(dataType);
			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var properties = defType.GetProperties(bindingFlags);
			_indent += "  ";
			foreach (var property in properties) {
				var attribute = property.GetAttribute<BinaryDataAttribute>();
				if (attribute == null) continue;

				var pos = stream.Position;
				var value = ReadPropertyValue(property, attribute, stream);
				var bytes = stream.Position - pos;
				var dataProperty = dataType.GetProperty(property.Name, bindingFlags);
				dataProperty.SetValue(instance, value, null);
			}
			_indent = _indent[..^2];
			return instance;
		}

		private static object CreateInstance(Type type)
		{
			var constructor = type.GetConstructor(Type.EmptyTypes);
			if (constructor == null)
				throw new NotSupportedException("Type must have got parameterless constructor: " + type.FullName);
			return constructor.Invoke(new object[] { });
		}

		private static object ReadPropertyValue(PropertyInfo property, BinaryDataAttribute attribute, Stream stream)
		{
			var propertyType = property.PropertyType;
			int length;

			if (ModelInterface.IsAssignableFrom(propertyType)) {
				return ReadBlock(stream, propertyType, propertyType, null, null);
			}
			
			if (propertyType.IsArray && ModelInterface.IsAssignableFrom(propertyType.GetElementType())) {
				if (!attribute.Length.HasValue)
					throw new NotSupportedException("Arrays must have length specified");

				var elementType = propertyType.GetElementType();
				length = attribute.Length.Value;
				var array = Array.CreateInstance(elementType, length);
				for (var i = 0; i < length; i++) {
					array.SetValue(ReadBlock(stream, elementType, elementType, null, null), i);
				}
				return array;
			}

			var valueType = propertyType.IsEnum ? Enum.GetUnderlyingType(propertyType) : propertyType;
			int? valueSize;

			if (attribute.Length.HasValue) {
				length = attribute.Length.Value;
				valueSize = valueType.IsValueType ? Marshal.SizeOf(valueType) : (int?)null;
			} else if (valueType == typeof(DateTime)) {
				valueSize = length = 0;
			} else if (valueType.IsValueType) {
				valueSize = length = Marshal.SizeOf(valueType);
			} else if (attribute.StringReadOptions == StringReadOptions.NullTerminated) {
				length = 64;
				valueSize = null;
			} else {
				throw new NotSupportedException("In case of reference types the length property is mandatory!");
			}

			var byteList = new List<byte>();
			var readMore = false;
			do {
				var buffer = stream.ReadBytes(length);
				if (attribute.StringReadOptions == StringReadOptions.NullTerminated) {
					var encoded = attribute.Encoding.GetChars(buffer);
					var i = 0;
					while (i < encoded.Length && encoded[i] != 0) i++;
					if (i < encoded.Length) {
						var text = attribute.Encoding.GetBytes(encoded.Take(i).ToArray());
						byteList.AddRange(text);
						stream.Position -= (length - text.Length - 2);
						readMore = false;
					} else {
						byteList.AddRange(buffer);
						readMore = true;
					}
				} else {
					byteList.AddRange(buffer);
				}
			} while (readMore);
			var bytes = byteList.ToArray();

			object value;
			if (propertyType.IsArray && propertyType.GetElementType() == typeof(byte)) {
				if (attribute.EndianType == EndianType.SwapBytesBy4 ||
					attribute.EndianType == EndianType.SwapBytesBy8)
					bytes.SwapBytes((int)attribute.EndianType);
				value = bytes;
			} else if (valueType == typeof(byte)) {
				value = bytes[0];
			} else if (valueType == typeof(string)) {
				switch (attribute.StringReadOptions) {
					case StringReadOptions.AutoTrim:
						var encoded = attribute.Encoding.GetChars(bytes);
						var i = 0;
						while (i < encoded.Length && encoded[i] != 0) i++;
						value = attribute.Encoding.GetString(attribute.Encoding.GetBytes(encoded.Take(i).ToArray()));
						break;
					case StringReadOptions.ID:
						value = bytes.ToHex();
						break;
					case StringReadOptions.NullTerminated:
					case StringReadOptions.Default:
						value = attribute.Encoding.GetString(bytes);
						break;
					default:
						throw new NotSupportedException("Invalid StringReadOptions value: " + attribute.StringReadOptions);
				}
			} else if (valueType == typeof(DateTime)) {
				var high = stream.ReadUInt();
				var low = stream.ReadUInt();
				long time = ((long)high << 32) + low;
				value = DateTime.FromFileTime(time);
			} else {
				if (attribute.EndianType == EndianType.BigEndian)
					Array.Reverse(bytes);
				if (valueSize.HasValue && length < valueSize)
					Array.Resize(ref bytes, valueSize.Value);

				if (valueType == typeof(short)) {
					value = BitConverter.ToInt16(bytes, 0);
				} else if (valueType == typeof(ushort)) {
					value = BitConverter.ToUInt16(bytes, 0);
				} else if (valueType == typeof(int)) {
					value = BitConverter.ToInt32(bytes, 0);
				} else if (valueType == typeof(uint)) {
					value = BitConverter.ToUInt32(bytes, 0);
				} else if (valueType == typeof(long)) {
					value = BitConverter.ToInt64(bytes, 0);
				} else if (valueType == typeof(ulong)) {
					value = BitConverter.ToUInt64(bytes, 0);
				} else {
					throw new NotSupportedException("Invalid value type: " + valueType);
				}
			}
			return value;
		}
	}
}