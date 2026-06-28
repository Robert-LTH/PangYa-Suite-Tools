//criado por LUISMK -> github.com/luismk
using PangyaAPI.PAK.Flags;
using PangyaAPI.Utilities.Cryptography; 
using System.Text;

namespace PangyaAPI.PAK.Models
{
    public class PakWriter
    {
        public byte PakVersion = 0x12;// 0x12 para versão 0.1, 0x13 para versão 0.2, 0x14 para versão 0.3
        public uint kXorKey = 0x71u;//version 0x71, 0x80 para versão 0 
        public string Author { get; set; } = "PangyaAPI.PAK";
        public PakFileEntryVersion EntryVersion { get; set; } = PakFileEntryVersion.V3;
        public PakFileEntryType EntryType { get; set; } = PakFileEntryType.LZ772;
        public byte CompressLevel { get; set; } = 5;
        public uint[] LocationKeys { get; set; } = PakKeys.JP;

        public void CreateFromDirectory(string sourceDir, string outputPath,
                                        Action<string>? log = null)
        {
            var entries = new List<(bool isDir, string path)>();

            string dir = sourceDir.TrimEnd('/', '\\');
            entries.Add((true, dir));

            foreach (string entry in Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.AllDirectories))
                entries.Add((Directory.Exists(entry), entry));

            string baseDir = Path.GetDirectoryName(dir) ?? dir;
            if (!baseDir.EndsWith('/') && !baseDir.EndsWith('\\'))
                baseDir += "/";

            CreateFromEntries(entries, baseDir, outputPath, log);
        }

        public void CreateFromFile(string filePath, string outputPath,
                                   Action<string>? log = null)
        {
            string baseDir = Path.GetDirectoryName(filePath) ?? "";
            if (!string.IsNullOrEmpty(baseDir) && !baseDir.EndsWith('/') && !baseDir.EndsWith('\\'))
                baseDir += "/";

            var entries = new List<(bool isDir, string path)> { (false, filePath) };
            CreateFromEntries(entries, baseDir, outputPath, log);
        }

        private void CreateFromEntries(List<(bool isDir, string path)> entries,
                                string baseDir,
                                string outputPath,
                                Action<string>? log)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true);//deveria euc-kr

            var fileEntries = new List<PakFileEntry>();

            // 1º PASSO: Gravar os arquivos e catalogar metadados PUROS (Sem criptografia ainda)
            foreach (var (isDir, path) in entries)
            {
                string relativeName = path.Length == baseDir.Length
                    ? Path.GetFileName(path)
                    : path.Substring(baseDir.Length);
                relativeName = relativeName.Replace('\\', '/');

                byte[] nameBytes = Encoding.ASCII.GetBytes(relativeName);
                int nameLen = nameBytes.Length;
                int nameAligned = (EntryVersion == PakFileEntryVersion.V3)
                    ? ((nameLen / 8 + (nameLen % 8 != 0 ? 1 : 0)) * 8)
                    : (nameLen + 1);

                byte[] nameField = new byte[nameAligned];
                Array.Copy(nameBytes, nameField, nameLen);

                // O offset real em disco é a posição ATUAL do BinaryWriter antes de gravar os dados do arquivo
                uint currentOffset = (uint)bw.BaseStream.Position;
                uint size = 0u;
                uint compSz = 0u;
                var type = isDir ? PakFileEntryType.Directory : EntryType;

                if (!isDir)
                {
                    byte[] fileData = File.ReadAllBytes(path);
                    size = (uint)fileData.Length;

                    string ext = Path.GetExtension(path).ToLowerInvariant();
                    if (ext == ".wav" || ext == ".mp3") type = PakFileEntryType.Raw;

                    byte[]? compressed = null;
                    if (type == PakFileEntryType.Raw)
                    {
                        compressed = fileData;
                    }
                    else if (type == PakFileEntryType.LZ77)
                    {
                        compressed = Lz77.Compress(fileData, CompressLevel, null);
                    }
                    else if (type == PakFileEntryType.LZ772)
                    {
                        compressed = Lz772.Compress(fileData, CompressLevel, null);
                    }

                    if (compressed == null)
                        throw new InvalidOperationException($"Falha ao comprimir: {path}");

                    compSz = (uint)compressed.Length;
                    bw.Write(compressed); // Escreve no arquivo binário
                }

                // Adiciona à lista temporária com os valores REAIS (puros)
                var entry = new PakFileEntry
                {
                    NameLength = (byte)(EntryVersion == PakFileEntryVersion.V3 ? nameAligned : nameLen),
                    Type = type,
                    Version = EntryVersion,
                    Offset = currentOffset, // Guardamos o offset correto
                    CompressSize = compSz,
                    Size = size,            // Guardamos o tamanho original correto
                };
                // IMPORTANTE: usar SetRawNameForWrite, e não a propriedade NameRaw.
                // O setter de NameRaw sanitiza (remove o padding de zeros) pensando
                // no cenário de LEITURA, o que corrompia o array aqui na escrita e
                // desalinhava todas as entries seguintes na tabela.
                entry.SetRawNameForWrite(nameField); // Nome limpo (com padding de zeros se V3)
                fileEntries.Add(entry);
            }

            // Escreve autor e o tamanho do campo do autor
            byte[] authorBytes = Encoding.ASCII.GetBytes(Author);
            bw.Write(authorBytes);
            ushort authorLenBE = (ushort)((authorBytes.Length >> 8) | (authorBytes.Length << 8));
            bw.Write(authorLenBE);

            // Marca o início exato da tabela de File Entries no arquivo final
            uint offsetFileEntry = (uint)bw.BaseStream.Position;

            // 2º PASSO: Criptografar e escrever a tabela de entradas (File Entries)
            foreach (var fe in fileEntries)
            {
                bw.Write(fe.NameLength);
                bw.Write((byte)(((byte)fe.Version << 4) | (byte)fe.Type));

                // Clona os dados para criptografar apenas no momento do fluxo de escrita
                uint encOffset = fe.Offset;
                uint encSize = fe.Size;
                byte[] encName = (byte[])fe.NameRaw.Clone();

                if (EntryVersion < PakFileEntryVersion.V3 || EntryVersion == PakFileEntryVersion.Raw)
                {
                    encSize ^= kXorKey;
                    for (int k = 0; k < encName.Length; k++) encName[k] ^= (byte)kXorKey;
                }
                else if (EntryVersion == PakFileEntryVersion.V3)
                {
                    // Junta Size e Offset em um bloco de 64-bit para o XTEA
                    ulong packed = ((ulong)encSize << 32) | encOffset;
                    packed = Xtea.Encrypt(LocationKeys, packed);
                    encSize = (uint)(packed >> 32);
                    encOffset = (uint)(packed & 0xFFFFFFFF);

                    // Criptografa o nome de 8 em 8 bytes alinhados
                    for (int k = 0; k < encName.Length; k += 8)
                    {
                        ulong block = BitConverter.ToUInt64(encName, k);
                        block = Xtea.Encrypt(LocationKeys, block);
                        var bytes = BitConverter.GetBytes(block);
                        Array.Copy(bytes, 0, encName, k, 8);
                    }
                }

                // Grava os dados devidamente protegidos na estrutura do arquivo
                bw.Write(encOffset);
                bw.Write(fe.CompressSize); // CompressSize NUNCA é encriptado no Pangya
                bw.Write(encSize);
                bw.Write(encName);
            }

            // Cabeçalho global do rodapé (Footer Header)
            bw.Write(offsetFileEntry);
            bw.Write((uint)fileEntries.Count);
            bw.Write(PakVersion);

            // Salva o arquivo em disco
            ms.Position = 0;
            using var fs = File.Create(outputPath);
            ms.CopyTo(fs);

            log?.Invoke($"Pak criado com sucesso: {outputPath} ({fs.Length} bytes)");
        }
    }
}
