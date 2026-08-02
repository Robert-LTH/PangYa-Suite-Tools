#nullable disable
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PangyaAPI.Utilities.Resources;

namespace PangyaAPI.Utilities.BinaryModels
{
    public class PangyaBinaryWriter : BinaryWriter
    {
        private readonly Encoding _Encoding;

        public PangyaBinaryWriter(Stream output) : this(output, PangyaEncoding.Current)
        {
        }

        public PangyaBinaryWriter(Stream output, Encoding encoding) : this(output, encoding, false)
        {
        }

        public PangyaBinaryWriter(Stream output, Encoding encoding, bool leaveOpen)
            : base(output, encoding, leaveOpen)
        {
            _Encoding = encoding;
        }

        public PangyaBinaryWriter() : this(new MemoryStream(), PangyaEncoding.Current)
        {
        }

        public PangyaBinaryWriter(Encoding encoding) : this(new MemoryStream(), encoding)
        {
        }

        public PangyaBinaryWriter(byte[] id) : this(id, PangyaEncoding.Current)
        {
        }

        public PangyaBinaryWriter(byte[] id, Encoding encoding) : this(new MemoryStream(), encoding)
        {
            Write(id);
        }

        public PangyaBinaryWriter(ushort id) : this(id, PangyaEncoding.Current)
        {
        }

        public PangyaBinaryWriter(ushort id, Encoding encoding) : this(new MemoryStream(), encoding)
        {
            init_plain(id);
        }

        public PangyaBinaryWriter(short id) : this(id, PangyaEncoding.Current)
        {
        }

        public PangyaBinaryWriter(short id, Encoding encoding) : this(new MemoryStream(), encoding)
        {
            WriteInt16(id);
        }
        public uint GetSize => (uint)BaseStream.Length;
        public byte[] GetBytes => CreateBytes();


        public void Clear()
        {
            if (GetSize > 0)
            {
                this.Flush();
                this.Close();
                this.OutStream = new MemoryStream();
            }
        }

        public void init_plain(ushort value)
        {
            if (GetSize > 0)
                this.OutStream = new MemoryStream();

            WriteUInt16(value);
        }

        public void init_plain(Enum value)
        {
            if (GetSize > 0)
                this.OutStream = new MemoryStream();

            WriteUInt16(Convert.ToUInt16(value));
        }


        public void Write(ArrayList values)
        {
            try
            {
                foreach (var item in values)
                {
                    switch (Type.GetTypeCode(item.GetType()))
                    {
                        case TypeCode.Boolean: Write((bool)item); break;
                        case TypeCode.Char: Write((char)item); break;
                        case TypeCode.SByte: Write((sbyte)item); break;
                        case TypeCode.Byte: Write((byte)item); break;
                        case TypeCode.Int16: Write((short)item); break;
                        case TypeCode.UInt16: Write((ushort)item); break;
                        case TypeCode.Int32: Write((int)item); break;
                        case TypeCode.UInt32: Write((uint)item); break;
                        case TypeCode.Int64: Write((long)item); break;
                        case TypeCode.UInt64: Write((ulong)item); break;
                        case TypeCode.Single: Write((Single)item); break;
                        case TypeCode.Double: Write((double)item); break;
                        case TypeCode.Decimal: Write((decimal)item); break;
                        case TypeCode.DateTime: WriteTime((DateTime)item); break;
                        case TypeCode.String: WritePStr((string)item); break;

                        case TypeCode.Empty:
                        case TypeCode.Object:
                        case TypeCode.DBNull:
                        default: throw new Exception(UtilityMessages.Get("TypeNotImplemented"));
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public void WriteParams(params object[] values)
        {
            foreach (var value in values)
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean: Write((bool)value); break;
                    case TypeCode.Char: Write((char)value); break;
                    case TypeCode.SByte: Write((sbyte)value); break;
                    case TypeCode.Byte: Write((byte)value); break;
                    case TypeCode.Int16: Write((short)value); break;
                    case TypeCode.UInt16: Write((ushort)value); break;
                    case TypeCode.Int32: Write((int)value); break;
                    case TypeCode.UInt32: Write((uint)value); break;
                    case TypeCode.Int64: Write((long)value); break;
                    case TypeCode.UInt64: Write((ulong)value); break;
                    case TypeCode.Single: Write((Single)value); break;
                    case TypeCode.Double: Write((double)value); break;
                    case TypeCode.Decimal: Write((decimal)value); break;
                    case TypeCode.DateTime: WriteTime((DateTime)value); break;
                    case TypeCode.String: WritePStr((string)value); break;

                    case TypeCode.Empty:
                    case TypeCode.Object:
                    case TypeCode.DBNull:
                    default: throw new Exception(UtilityMessages.Get("TypeNotImplemented"));
                }
            }
        }


        public void WriteInt16(short value)
        {
            try
            {
                Write(value);
            }
            catch
            {

            }

        }
        public bool WriteStr(string message, int length)
        {

            try
            {
                if (message == null)
                {
                    message = string.Empty;
                }

                var ret = new byte[length];
                _Encoding.GetBytes(message).Take(length).ToArray().CopyTo(ret, 0);

                Write(ret);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteStr(string message)
        {
            try
            {
                WriteStr(message, message.Length);

            }
            catch
            {
                return false;
            }
            return true;

        }
        public bool WriteString(string data) => WritePStr(data);
        public bool WriteString(string data, int len) => WriteStr(data, len);
        public bool WritePStr(string data)
        {
            if (data == null) data = "";
            try
            {
                var bytes = _Encoding.GetBytes(data);

                if (bytes.Length > ushort.MaxValue)
                    throw new InvalidOperationException(UtilityMessages.Get("ShiftJisStringTooLong"));

                Write((ushort)bytes.Length); // Prefixa o tamanho
                Write(bytes); // Escreve a string

            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteBytes(byte[] message)
        {
            try
            {
                return WriteBytes(message, message.Length);
            }
            catch
            {
                return false;
            }
        }
        public bool WriteBytes(byte[] message, int length)
        {
            try
            {
                if (message == null)
                    message = new byte[length];

                var result = new byte[length];

                Buffer.BlockCopy(message, 0, result, 0, length);

                Write(result);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool Write(byte[] message, int length)
        {
            try
            {
                if (message == null)
                    message = new byte[length];

                var result = new byte[length];

                Buffer.BlockCopy(message, 0, result, 0, message.Length);

                Write(result);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteZero(int Lenght)
        {
            try
            {
                Write(new byte[Lenght]);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt16(ushort value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt16(Enum value)
        {
            try
            {
                WriteUInt16(Convert.ToUInt16(value));
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt16(int value)
        {
            try
            {
                Write((ushort)value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt16(uint value)
        {
            try
            {
                Write((ushort)value);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteSByte(sbyte value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteByte(Enum value)
        {

            try
            {
                Write(Convert.ToByte(value));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(UtilityMessages.Format("WriteByteFailed", ex.Message));
            }
            return false;
        }
        public bool WriteByte(int value)
        {
            if (value < byte.MinValue || value > byte.MaxValue)
            {
                Console.WriteLine(UtilityMessages.Format("ByteOutOfRange", value));
                Console.WriteLine(Environment.StackTrace); // Mostra de onde o valor veio
                return false;
            }

            try
            {
                Write((byte)value);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(UtilityMessages.Format("WriteByteFailed", ex.Message));
            }
            return false;
        }
        public bool WriteSingle(float value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt32(ushort value)
        {
            try
            {
                Write(Convert.ToUInt32(value));
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt32(short value)
        {
            try
            {
                Write(Convert.ToInt32(value));//fica melhor no int32
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt32(uint value)
        {
            try
            {
                Write((value));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt32(Enum value)
        {
            try
            {
                Write(Convert.ToUInt32(value));  // converte enum para uint
            }
            catch
            {
                return false;
            }
            return true;
        }


        public bool WriteInt16(short[] values)
        {
            try
            {
                for (uint i = 0; i < values.Count(); i++)
                    Write(values[i]);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt16(ushort[] values)
        {
            try
            {
                for (uint i = 0; i < values.Count(); i++)
                    Write(values[i]);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool WriteUInt32(uint[] values)
        {
            try
            {
                for (uint i = 0; i < values.Count(); i++)
                    Write(values[i]);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteInt32(int value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteUInt64(ulong value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteInt64(long value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteInt64(long[] value)
        {
            try
            {
                for (int i = 0; i < value.Length; i++)
                    Write(value[i]);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteDouble(double value)
        {
            try
            {
                Write(value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool WriteStruct<T>(T value) where T : struct
        {
            try
            {
                // Certifique-se de que o tipo T é realmente uma estrutura.
                if (!typeof(T).IsValueType)
                    throw new ArgumentException(UtilityMessages.Get("ValueMustBeStruct"));

                int size = Marshal.SizeOf(typeof(T));
                byte[] buffer = new byte[size];

                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(value, ptr, false);
                    Marshal.Copy(ptr, buffer, 0, size);

                    // Método Write recebe buffer e tamanho.
                    Write(buffer, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(UtilityMessages.Format("StructSerializationFailed", ex.Message));
                Console.WriteLine(UtilityMessages.Format("CallStack", ex.StackTrace));
                return false;
            }

            return true;
        }

        public void WriteFile(string file)
        {
            File.WriteAllBytes(file, GetBytes);
        }

        public bool WriteStruct(object value, object valueOri)
        {
            if (value == null || valueOri == null)
            {
                Console.WriteLine(UtilityMessages.Get("StructValuesNull"));
                return false;
            }

            try
            {
                int size = Marshal.SizeOf(valueOri);

                // Aloca memória e copia os dados da estrutura
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    byte[] arr = new byte[size];

                    Marshal.StructureToPtr(value, ptr, true);
                    Marshal.Copy(ptr, arr, 0, size);
                    Marshal.FreeHGlobal(ptr);
                    WriteBytes(arr);
                }
                catch (ArgumentException ex) // Corrigido para Exception com "E" maiúsculo
                {
                    // Log estruturado do erro
                    Console.WriteLine(UtilityMessages.Get("StructHandlingFailed"));
                    Console.WriteLine(UtilityMessages.Format("ExceptionMessage", ex.Message));
                    Console.WriteLine(UtilityMessages.Format("ExceptionSource", ex.Source));
                    Console.WriteLine(UtilityMessages.Format("ExceptionMethod", ex.TargetSite));
                    Console.WriteLine(UtilityMessages.Format("CallStack", ex.StackTrace));
                    return false;
                }
            }
            catch (Exception ex) // Corrigido para Exception com "E" maiúsculo
            {
                // Log estruturado do erro
                Console.WriteLine(UtilityMessages.Get("StructHandlingFailed"));
                Console.WriteLine(UtilityMessages.Format("ExceptionMessage", ex.Message));
                Console.WriteLine(UtilityMessages.Format("ExceptionSource", ex.Source));
                Console.WriteLine(UtilityMessages.Format("ExceptionMethod", ex.TargetSite));
                Console.WriteLine(UtilityMessages.Format("CallStack", ex.StackTrace));
                return false;
            }

            return true;
        }

        public bool WriteBuffer(object value, int size)
        {
            try
            {
                if (value == null)
                {
                    Debug.WriteLine(UtilityMessages.Format("WriteBufferNull", value.GetType().Name));
                    WriteBytes(new byte[size]);
                    return true;
                }

                byte[] arr = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);
                Write(arr);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(Environment.StackTrace);
                throw e;
            }
        }

        public bool WriteHexArray(string _value)
        {
            try
            {
                _value = _value.Replace(" ", "");
                int _size = _value.Length / 2;
                byte[] _result = new byte[_size];
                for (int ii = 0; ii < _size; ii++)
                    WriteByte(Convert.ToByte(_value.Substring(ii * 2, 2), 16));
            }
            catch
            {
                return false;
            }
            return true;
        }


        public void WriteZeroByte(int v)
        {
            WriteZero(v);
        }

        public void WriteFloat(float v)
        {
            this.Write(v);
        }
        public void WriteFloat(float[] value)
        {
            for (int i = 0; i < value.Length; i++)
                this.Write(value[i]);
        }

        public void WriteInt32(int[] value)
        {
            for (int i = 0; i < value.Length; i++)
                this.Write(value[i]);
        }
        public void WriteUInt64(ulong[] value)
        {
            for (int i = 0; i < value.Length; i++)
                this.Write(value[i]);
        }

        public void WriteUInt32(uint[] values, uint count)
        {
            var rest = count - values.Length;
            WriteUInt32(values);
            for (int i = 0; i < rest; i++)
            {
                WriteUInt32(0); // Preenche com zero se não houver valor
            }
        }
        /// <summary>
        /// Write Pangya Time
        /// </summary>
        /// <returns></returns>
        public bool WriteTime(DateTime? date)
        {
            try
            {
                if (date.HasValue == false || date?.Ticks == 0)
                {
                    Write(new byte[16]);
                    return true;
                }
                WriteUInt16((ushort)date?.Year);
                WriteUInt16((ushort)date?.Month);
                WriteUInt16(Convert.ToUInt16(date?.DayOfWeek));
                WriteUInt16((ushort)date?.Day);
                WriteUInt16((ushort)date?.Hour);
                WriteUInt16((ushort)date?.Minute);
                WriteUInt16((ushort)date?.Second);
                WriteUInt16((ushort)date?.Millisecond);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Write Pangya Time
        /// </summary>
        /// <returns></returns>
        public bool WriteTime()
        {
            DateTime date = DateTime.Now;
            try
            {
                WriteUInt16((ushort)date.Year);
                WriteUInt16((ushort)date.Month);
                WriteUInt16((ushort)date.DayOfWeek);
                WriteUInt16((ushort)date.Day);
                WriteUInt16((ushort)date.Hour);
                WriteUInt16((ushort)date.Minute);
                WriteUInt16((ushort)date.Second);
                WriteUInt16((ushort)date.Millisecond);
                return true;
            }
            catch
            {
                return false;
            }
        }
        byte[] CreateBytes()
        {
            if (OutStream is MemoryStream stream)
                return stream.ToArray();


            using (var memoryStream = new MemoryStream())
            {
                memoryStream.GetBuffer();
                OutStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

    }
}
