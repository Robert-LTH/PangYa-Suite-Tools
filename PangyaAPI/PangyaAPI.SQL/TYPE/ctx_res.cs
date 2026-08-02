using System;
using System.Data;
using PangyaAPI.Utilities;
namespace PangyaAPI.SQL
{
    public class ctx_res
    {
        public object[] data { get; set; }
        public DataRow data_row;
        public uint cols { get; set; }
        public ctx_res next;

        public T ConvertToClass<T>() where T : new()
        {
            return data_row.ToObject<T>();
        }

        public bool IsNotNull(int column)
        {
            try
            {
                if (data == null || column < 0 || column >= data.Length)
                {
                    return false; // Array nulo ou índice inválido é considerado vazio
                }

                var value = data[column];

                // Verifica se o valor é nulo ou uma string vazia
                if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                {
                    return false;
                }
                return true; // O valor não é nulo, nem vazio
            }
            catch
            {
                return false;
            }
        }

        public bool GetBoolean(int colum)
        {
            return data[colum] != null && Convert.ToBoolean(data[colum]);
        }

        public float GetFloat(int colum)
        {
            return data[colum] != null ? Convert.ToSingle(data[colum]) : 0f;
        }

        public int GetInt32(int colum)
        {
            return data[colum] != null ? Convert.ToInt32(data[colum]) : 0;
        }

        public uint GetUInt32(int colum)
        {
            return data[colum] != null ? Convert.ToUInt32(data[colum]) : 0;
        }

        public long GetInt64(int colum)
        {
            return data[colum] != null ? Convert.ToInt64(data[colum]) : 0L;
        }

        public ulong GetUInt64(int colum)
        {
            return data[colum] != null ? Convert.ToUInt64(data[colum]) : 0UL;
        }

        public byte GetByte(int colum)
        {
            return data[colum] != null ? Convert.ToByte(data[colum]) : (byte)0;
        }

        public sbyte GetSByte(int colum)
        {
            return data[colum] != null ? Convert.ToSByte(data[colum]) : (sbyte)0;
        }

        public short GetInt16(int colum)
        {
            return data[colum] != null ? Convert.ToInt16(data[colum]) : (short)0;
        }

        public ushort GetUInt16(int colum)
        {
            return data[colum] != null ? Convert.ToUInt16(data[colum]) : (ushort)0;
        }

        public DateTime GetDateTime(int colum)
        {
            return data[colum] != null ? Convert.ToDateTime(data[colum]) : DateTime.MinValue;
        }

        public string GetString(int colum)
        {
            return data[colum]?.ToString() ?? string.Empty;
        }

    }
}

