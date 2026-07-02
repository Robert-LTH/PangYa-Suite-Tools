//criado por LUISMK -> github.com/luismk
using PangyaAPI.PAK.Flags;
using System.Linq;

namespace PangyaAPI.PAK.Models
{
    /// <summary>
    /// Configuração usada para reconstruir um PAK (mesmas opções do PakWriter).
    /// </summary>
    public readonly record struct PakRebuildOptions(
        PakFileEntryVersion EntryVersion,
        PakFileEntryType EntryType,
        byte CompressLevel,
        uint[] LocationKeys,
        string Author);
    
        /// <summary>
        /// Par de arquivo de origem + pasta relativa explícita dentro do PAK (pode ser null,
        /// caso em que o destino é resolvido automaticamente via FindExistingRelativeFolder).
        /// </summary>
        public readonly record struct PakInjectItem(string SourcePath, string? RelativeFolder);


    /// <summary>
    /// Operações de alto nível sobre um PAK existente: injetar/atualizar arquivos
    /// e remover arquivos, sempre preservando a estrutura de pastas original.
    /// Estratégia: extrai o conteúdo atual para uma pasta temporária, aplica a
    /// mutação desejada e reconstrói com o PakWriter — com backup automático
    /// do .pak original em caso de falha.
    /// </summary>
    public static class PakManager
    {
        /// <summary>
        /// Extrai todas as entradas (exceto as filtradas por <paramref name="skip"/>)
        /// preservando a estrutura de pastas original do PAK de forma paralela e otimizada.
        /// </summary>
        private static void ExtractAllPreservingStructure(PakReader reader, string tempDir,
                                                           Func<PakFileEntry, bool>? skip = null,
                                                           Action<int, int>? onProgress = null)
        {
            var files = reader.Entries.Where(e => e.Type != PakFileEntryType.Directory).ToList();

            // Filtra os arquivos válidos antes de iniciar o I/O
            var filesToProcess = files.Where(entry => skip == null || !skip(entry)).ToList();
            int total = filesToProcess.Count;
            if (total == 0) return;

            int done = 0;

            // Cria uma estrutura para bufferizar os dados descomprimidos na RAM
            var fileBuffer = new (string DestPath, byte[] Data)[total];

            // FASE 1 & 2: Leitura paralela do PAK e descompressão puramente em memória
            Parallel.For(0, total, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                var entry = filesToProcess[i];

                // Reaproveita o método rápido thread-safe do PakReader
                byte[]? data = reader.ExtractEntryToBytes(entry);

                if (data == null && entry.Size == 0)
                    data = Array.Empty<byte>();

                if (data != null)
                {
                    string relativePath = entry.Name.Replace('/', '\\');
                    string destPath = Path.Combine(tempDir, relativePath);
                    fileBuffer[i] = (destPath, data);
                }

                // Relata progresso em lotes seguros
                int currentDone = Interlocked.Increment(ref done);
                if (currentDone % 50 == 0 || currentDone == total)
                {
                    onProgress?.Invoke(currentDone, total);
                }
            });

            // FASE 3: Mapeamento de pastas únicas e escrita em disco contínua
            var directoriesToCreate = fileBuffer
                .Where(f => f.DestPath != null)
                .Select(f => Path.GetDirectoryName(f.DestPath))
                .Distinct();

            foreach (var dir in directoriesToCreate)
            {
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
            }

            // Escrita paralela e direta no SSD
            Parallel.ForEach(fileBuffer, file =>
            {
                if (file.DestPath == null || file.Data == null) return;
                File.WriteAllBytes(file.DestPath, file.Data);
            });
        }

        /// <summary>
        /// Procura, dentro das entries atuais do PAK, em qual pasta interna já existe
        /// um arquivo com o mesmo nome (case-insensitive). Usado para saber onde colocar
        /// um arquivo "atualizado" na pasta temporária antes de reconstruir o PAK.
        /// Retorna string.Empty se não encontrar (o arquivo é tratado como novo, na raiz).
        /// </summary>
        public static string FindExistingRelativeFolder(PakReader reader, string fileName)
        {
            var match = reader.Entries.FirstOrDefault(e =>
                e.Type != PakFileEntryType.Directory &&
                string.Equals(Path.GetFileName(e.Name.Replace('/', '\\')), fileName, StringComparison.OrdinalIgnoreCase));

            if (match == null) return "";

            string normalized = match.Name.Replace('/', '\\');
            return Path.GetDirectoryName(normalized) ?? "";
        }

        /// <summary>
        /// Sobrecarga que permite informar explicitamente em qual pasta interna cada arquivo
        /// deve cair (útil ao arrastar uma pasta inteira, preservando sua estrutura). Quando
        /// RelativeFolder é null, cai no comportamento antigo: procura pasta existente pelo
        /// nome do arquivo, ou usa defaultRelativeFolder.
        /// </summary>
        public static void InjectFiles(string pakPath, PakReader reader, IEnumerable<PakInjectItem> items,
                                        PakRebuildOptions options, string defaultRelativeFolder = "",
                                        Action<string>? log = null, Action<int, int>? onProgress = null, bool SaveBck = false)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "PakTemp_" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                log?.Invoke("Extraindo conteúdo atual do PAK...");
                ExtractAllPreservingStructure(reader, tempDir, onProgress: onProgress);

                foreach (var item in items)
                {
                    string fileName = Path.GetFileName(item.SourcePath);

                    string relFolder;
                    if (item.RelativeFolder != null)
                    {
                        // Pasta explícita (ex: vinda de uma pasta arrastada) — respeita sempre,
                        // mesmo que já exista um arquivo de mesmo nome em outro lugar do PAK.
                        relFolder = item.RelativeFolder;
                    }
                    else
                    {
                        relFolder = FindExistingRelativeFolder(reader, fileName);
                        if (string.IsNullOrEmpty(relFolder))
                            relFolder = defaultRelativeFolder;
                    }

                    string destDir = string.IsNullOrEmpty(relFolder) ? tempDir : Path.Combine(tempDir, relFolder);
                    Directory.CreateDirectory(destDir);

                    string destPath = Path.Combine(destDir, fileName);
                    File.Copy(item.SourcePath, destPath, true);

                    log?.Invoke(string.IsNullOrEmpty(relFolder)
                        ? $"Novo arquivo adicionado na raiz: {fileName}"
                        : $"Atualizado/adicionado em \"{relFolder}\": {fileName}");
                }

                reader.Dispose();
                RebuildFromTemp(pakPath, tempDir, options, log, SaveBck);
            }
            finally
            {
                TryDeleteDirectory(tempDir);
            }
        }

        public static void InjectFiles(string pakPath, PakReader reader, IEnumerable<string> sourceFiles,
                                PakRebuildOptions options, string defaultRelativeFolder = "",
                                Action<string>? log = null, Action<int, int>? onProgress = null)
        {
            var items = sourceFiles.Select(f => new PakInjectItem(f, null));
            InjectFiles(pakPath, reader, items, options, defaultRelativeFolder, log, onProgress);
        }

        /// <summary>
        /// Reconstrói o PAK usando uma chave/região diferente, mantendo todo o conteúdo
        /// (arquivos e estrutura de pastas) idêntico. Útil para "migrar" um PAK entre
        /// regiões/versões do cliente que usam chaves XTEA diferentes.
        /// </summary>
        public static void ChangeEncryptionKey(string pakPath, PakReader reader, PakRebuildOptions newOptions,
                                                Action<string>? log = null, Action<int, int>? onProgress = null, bool SaveBck = false)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "PakTemp_" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                log?.Invoke("Extraindo conteúdo atual do PAK (chave original)...");
                ExtractAllPreservingStructure(reader, tempDir, onProgress: onProgress);

                log?.Invoke("Reconstruindo PAK com a nova chave...");

                // Fecha o handle do .pak original antes do File.Move dentro de RebuildFromTemp.
                reader.Dispose();

                RebuildFromTemp(pakPath, tempDir, newOptions, log, SaveBck);
            }
            finally
            {
                TryDeleteDirectory(tempDir);
            }
        }

        /// <summary>
        /// Remove uma ou mais entradas (pelo nome interno completo, ex.:
        /// "data/round20_abbot/ase/ab_abbot01.pet") e reconstrói o PAK sem elas.
        /// </summary>
        public static void RemoveFiles(string pakPath, PakReader reader, IEnumerable<string> namesToRemove,
                                        PakRebuildOptions options, Action<string>? log = null,
                                        Action<int, int>? onProgress = null, bool SaveBck = false)
        {
            var removeSet = new HashSet<string>(
                namesToRemove.Select(n => n.Replace('/', '\\')),
                StringComparer.OrdinalIgnoreCase);

            string tempDir = Path.Combine(Path.GetTempPath(), "PakTemp_" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                log?.Invoke("Extraindo conteúdo atual do PAK (ignorando arquivo(s) removido(s))...");
                ExtractAllPreservingStructure(reader, tempDir,
                    skip: e => removeSet.Contains(e.Name.Replace('/', '\\')),
                    onProgress: onProgress);

                foreach (var name in removeSet)
                    log?.Invoke($"Removido: {name}");

                // Fecha o handle do .pak original antes do File.Move dentro de RebuildFromTemp.
                reader.Dispose();

                RebuildFromTemp(pakPath, tempDir, options, log, SaveBck);
            }
            finally
            {
                TryDeleteDirectory(tempDir);
            }
        }

        private static void RebuildFromTemp(string pakPath, string tempDir, PakRebuildOptions options, Action<string>? log, bool SaveBck)
        {
            if (SaveBck)
            {
                string backupPak = pakPath + ".bak";
                if (File.Exists(backupPak)) File.Delete(backupPak);
                File.Move(pakPath, backupPak);
            }
           

            try
            {
                var writer = new PakWriter
                {
                    EntryVersion = options.EntryVersion,
                    EntryType = options.EntryType,
                    CompressLevel = options.CompressLevel,
                    LocationKeys = options.LocationKeys,
                    Author = options.Author,
                };

                writer.CreateFromDirectoryContents(tempDir, pakPath, log); 
            }
            catch
            {
                if (SaveBck)
                {
                    string backupPak = pakPath + ".bak";
                    // Falhou ao reconstruir: restaura o backup para não perder o PAK original.
                    if (File.Exists(pakPath)) File.Delete(pakPath);
                    File.Move(backupPak, pakPath);
                }
                throw;
            }
        }/// <summary>
         /// Extrai uma lista específica de entradas (entries) de um arquivo .pak para um diretório de saída.
         /// </summary>
        public static void ExtractFiles(
            string currentPakPath,
            PakReader reader,
            List<PakFileEntry> entriesToExtract,
            string outputDir,
            Action<string> log,
            Action<int, int> onProgress)
        {
            if (string.IsNullOrEmpty(currentPakPath) || reader == null || entriesToExtract == null)
                throw new ArgumentNullException("Parâmetros de extração inválidos.");

            if (!File.Exists(currentPakPath))
                throw new FileNotFoundException($"Arquivo PAK não encontrado: {currentPakPath}");

            int totalFiles = entriesToExtract.Count;
            int currentFileIndex = 0;

            // Abrimos o stream do arquivo PAK principal para leitura dos dados brutos
            using (var pakStream = new FileStream(currentPakPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                foreach (var entry in entriesToExtract)
                {
                    // Ignora registros que sejam marcados como diretórios vazios na tabela do PAK
                    if (entry.Type == PakFileEntryType.Directory || entry.Size == 0)
                    {
                        currentFileIndex++;
                        onProgress?.Invoke(currentFileIndex, totalFiles);
                        continue;
                    }

                    try
                    {
                        // Normaliza o caminho interno do PAK para o sistema operacional local
                        string normalizedPath = entry.Name.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                        string fullOutputPath = Path.Combine(outputDir, normalizedPath);

                        // Garante que a estrutura de pastas onde o arquivo vai ser salvo exista no seu HD
                        string? directoryPath = Path.GetDirectoryName(fullOutputPath);
                        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Move o ponteiro de leitura do PAK para o início exato do arquivo desejado (Offset)
                        pakStream.Seek(entry.Offset, SeekOrigin.Begin);

                        // Lê os dados brutos e grava no arquivo de destino de forma otimizada
                        using (var outputStream = new FileStream(fullOutputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            byte[] buffer = new byte[8192]; // Buffer de 8KB para performance de leitura/escrita
                            long bytesRemaining = entry.Size;

                            while (bytesRemaining > 0)
                            {
                                int bytesToRead = (int)Math.Min(buffer.Length, bytesRemaining);
                                int bytesRead = pakStream.Read(buffer, 0, bytesToRead);

                                if (bytesRead == 0) break; // Fim inesperado do stream

                                outputStream.Write(buffer, 0, bytesRead);
                                bytesRemaining -= bytesRead;
                            }
                        }

                        log?.Invoke($"Extraído com sucesso: {entry.Name}");
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke($"Falha ao extrair o arquivo {entry.Name}: {ex.Message}");
                        // Dependendo do seu fluxo, você pode optar por lançar o erro 'throw;' ou ignorar e continuar
                    }

                    // Atualiza o contador de progresso após concluir a extração deste arquivo
                    currentFileIndex++;
                    onProgress?.Invoke(currentFileIndex, totalFiles);
                }
            }
        }

        private static void TryDeleteDirectory(string path)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, true); }
            catch { /* limpeza best-effort, não deve interromper o fluxo principal */ }
        }

    }
}
