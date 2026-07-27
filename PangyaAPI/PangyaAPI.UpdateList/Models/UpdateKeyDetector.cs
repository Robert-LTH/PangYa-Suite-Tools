using System;
using System.IO;
using System.Text;
using PangyaAPI.UpdateList.Flags;
using PangyaAPI.Utilities.Cryptography;

public class UpdateKeyDetector
{ 

    public static UpdateResult DetectAndSetKey(string filePath, out uint[]? detectedKey, out byte[]? decryptedData, out string Document)
    {
        Document = "";
        detectedKey = null;
        decryptedData = null;

        if (IsFileCrypt(filePath) != OperacaoEnum.Decrypt)
        { 
            return UpdateResult.Sucess;
        }

        var data = File.ReadAllBytes(filePath);
        if (data.Length < 8 || data.Length % 8 != 0) return UpdateResult.Falied;

        if (Xtea.TryDecryptBlocks(data, UpdateKeys.All, "<?"u8, out string? matchedLabel,
            out uint[]? matchedKeys, out byte[]? plaintext))
        {
            Console.WriteLine($"[Sucesso] Chave detectada com sucesso: Região {matchedLabel}");
            detectedKey = matchedKeys;
            Document = Encoding.UTF8.GetString(plaintext!).Replace("\0", "").Trim();
            decryptedData = Encoding.UTF8.GetBytes(Document);

            string outputXmlPath = Path.Combine(Directory.GetCurrentDirectory(), "updatelist.xml");
            File.WriteAllBytes(outputXmlPath, decryptedData);
            return UpdateResult.Sucess;
        }

        Console.WriteLine("[Aviso] Nenhuma das chaves conhecidas conseguiu descriptografar o arquivo.");
        return UpdateResult.Test_New_Key;
    }

    public static OperacaoEnum IsFileCrypt(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return OperacaoEnum.Falied;
        }

        byte[] rawBytes = File.ReadAllBytes(filePath);
        if (rawBytes.Length < 2) return OperacaoEnum.Decrypt;

        // Verifica os cabeçalhos sem converter o arquivo inteiro para char[] (ganho de performance)
        if (rawBytes[0] == '<' && rawBytes[1] == '?')
        {
            // Se o arquivo tiver tamanho suficiente, lê a região do cabeçalho
            if (rawBytes.Length > 76)
            {
                char c75 = (char)rawBytes[75];
                char c76 = (char)rawBytes[76];
                Console.WriteLine($"[Info] Arquivo aberto em texto puro. Pronto para encriptar ({c75}{c76})");
            }
            return OperacaoEnum.Encrypt;
        }

        return OperacaoEnum.Decrypt;
    }
}
