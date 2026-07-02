using System.Security.Cryptography;
using System.Text;

namespace PangyaAPI.UpdateList.Models
{
    /// <summary>
    /// Varre um diretório de arquivos de atualização e gera o arquivo updatelist
    /// finalizado (criptografado em XTEA).
    ///
    /// Baseado no XMLParser.cs original (Dashboard.RecursiveFileProcessor):
    /// - fdate/ftime usam LastWriteTime + 3 horas (padrão legado do Pangya)
    /// - fdir é apenas o nome imediato da pasta pai + "\"
    /// - pname = fname + ".zip"
    /// - psize = tamanho real do zip (preenchido após compressão)
    /// - CheckSum = MD5(nome + tamanho + data+3h) para detecção de mudanças
    /// - Extensões ignoradas: .bak .txt .lib .exp .pdb .xml .dmp .cln .json
    ///   e arquivos "uninstall.exe"
    /// </summary>
    public class UpdateMaker
    {
        private readonly Crc32 _crcCalculator = new Crc32();

        // Extensões/nomes de arquivo a ignorar na varredura (igual ao XMLParser original)
        private static readonly string[] IgnoredSuffixes =
        {
            ".bak", ".txt", ".lib", ".exp", ".pdb", ".xml",
            ".dmp", ".cln", ".json", "uninstall.exe"
        };

        /// <summary>
        /// Varre <paramref name="targetFolder"/> recursivamente, monta as entries e
        /// gera o arquivo updatelist final em <paramref name="outputPath"/>.
        /// </summary>
        public void GenerateFromDirectory(
            string targetFolder,
            string outputPath,
            uint[] regionKeys,
            string patchVersion,
            string updateVersion = "20090331",
            string clientPatchNum = "1",
            Action<int, int>? onProgress = null)
        {
            if (!Directory.Exists(targetFolder))
                throw new DirectoryNotFoundException($"Diretório alvo não existe: {targetFolder}");

            var allFiles = ListFiles(targetFolder);
            var entries  = new List<UpdateEntry>();
            int total    = allFiles.Count;
            int done     = 0;

            foreach (var filePath in allFiles)
            {
                var info = new FileInfo(filePath);

                // Ignora o próprio arquivo de saída se estiver dentro da pasta varrida
                if (info.FullName.Equals(Path.GetFullPath(outputPath), StringComparison.OrdinalIgnoreCase))
                    continue;

                // fdate/ftime: LastWriteTime + 3h (padrão legado do Pangya)
                DateTime writeTimePlus3 = info.LastWriteTime.AddHours(3);

                // fdir: apenas o nome imediato da pasta pai + "\"
                // Ex.: Pangya\data\round20\file.ext → fdir = "round20\"
                string immediateParent = info.Directory?.Name ?? "";
                string fdir = string.IsNullOrEmpty(immediateParent) ? "" : (immediateParent + "\\");

                var entry = new UpdateEntry
                {
                    fname       = info.Name,
                    fdir        = fdir,
                    fsize       = info.Length,
                    fcrc        = _crcCalculator.CalculateFileCRC(filePath),
                    fdate       = writeTimePlus3.ToString("yyyy-MM-dd"),
                    ftime       = writeTimePlus3.ToString("HH:mm:ss"),
                    pname       = info.Name + ".zip",
                    psize       = 0, // preenchido após compressão; 0 até lá

                    // Campos runtime-only
                    FullPath    = filePath,
                    CheckSum    = ComputeCheckSum(info.Name, info.Length, writeTimePlus3),
                    Index       = done + 1
                };

                entries.Add(entry);
                done++;
                onProgress?.Invoke(done, total);
            }

            var header = new UpdateHeader
            {
                ClientPatchVersion = patchVersion,
                ClientPatchNum     = clientPatchNum,
                UpdateVersion      = updateVersion
            };

            var writer = new UpdateWriter(regionKeys);
            writer.WriteUpdateList(outputPath, header, entries);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Varre recursivamente <paramref name="folder"/>, ignorando extensões/nomes
        /// indesejados (igual ao XMLParser original: .bak, .txt, .lib, .exp, .pdb,
        /// .xml, .dmp, .cln, .json, uninstall.exe).
        /// </summary>
        private static List<string> ListFiles(string folder)
        {
            var result = new List<string>();

            foreach (string subDir in Directory.GetDirectories(folder))
            {
                // Ignora subpastas cujo nome contenha extensões indesejadas
                string dirName = Path.GetFileName(subDir);
                if (IgnoredSuffixes.Any(s => dirName.Contains(s, StringComparison.OrdinalIgnoreCase)))
                    continue;

                result.AddRange(ListFiles(subDir));
            }

            foreach (string file in Directory.GetFiles(folder))
            {
                if (!IgnoredSuffixes.Any(s => file.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
                    result.Add(file);
            }

            return result;
        }

        /// <summary>
        /// MD5(nome + tamanho + data+3h formatada) — mesma lógica do XMLParser original.
        /// Útil para detecção de mudanças sem recalcular CRC de todos os arquivos.
        /// </summary>
        private static string ComputeCheckSum(string name, long size, DateTime writeTimePlus3)
        {
            string input = name + size.ToString() + writeTimePlus3.ToString("yyyy-MM-dd HH:mm:ss");
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
