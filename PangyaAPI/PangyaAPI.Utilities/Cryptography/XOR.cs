namespace PangyaAPI.Utilities.Cryptography;

public static class XOR
{
	public static async Task TransformFileAsync(string inputPath, string outputPath, byte key,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
		ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

		await using var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read,
			64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
		await using var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
			64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
		await TransformAsync(input, output, key, cancellationToken).ConfigureAwait(false);
	}

	public static async Task TransformAsync(Stream input, Stream output, byte key,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(input);
		ArgumentNullException.ThrowIfNull(output);

		byte[] buffer = new byte[64 * 1024];
		int read;
		while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
		{
			for (int i = 0; i < read; i++) buffer[i] ^= key;
			await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
		}
	}

	public static byte[] Cipher(byte[] data, uint key)
	{
		ArgumentNullException.ThrowIfNull(data);
		Cipher(data.AsSpan(), (byte)key);
		return data;
	}

	public static void Cipher(Span<byte> data, byte key)
	{
		for (int i = 0; i < data.Length; i++) data[i] ^= key;
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
