namespace PangyaAPI.Utilities.Cryptography;

public static class XOR
{
	public static byte[] Cipher(byte[] data, uint key)
	{
		for (int i = 0; i < data.Length; i++)
		{
			data[i] ^= (byte)key;
		}
		return data;
	}

	public static string XOR_data(char[] Data, int DataSize, int Compress_type)
	{
		if (Compress_type < 4)
		{
			for (int i = 0; i < DataSize; i++)
			{
				Data[i] ^= 'q';
			}
		}
		string text = new string(Data);
		return text.Substring(0, DataSize);
	}
}
