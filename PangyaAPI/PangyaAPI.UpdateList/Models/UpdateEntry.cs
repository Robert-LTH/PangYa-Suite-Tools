namespace PangyaAPI.UpdateList.Models;

/// <summary>
/// Representa um arquivo dentro da updatelist (.xml criptografado em XTEA).
///
/// Os 8 campos persistidos no XML são fname..psize — espelham exatamente os
/// atributos do elemento &lt;fileinfo&gt;. CheckSum, Index e FullPath são campos
/// auxiliares de RUNTIME, usados pelo UpdateMaker durante o processamento, mas
/// nunca serializados.
///
/// Nota: fdate/ftime devem ser gravados com LastWriteTime + 3 horas (padrão
/// legado do Pangya — aplicado pelo UpdateMaker ao montar o entry).
/// </summary>
public class UpdateEntry
{
    // ── Campos persistidos no XML (<fileinfo .../>) ─────────────────────────
    public string fname { get; set; } = "";

    /// <summary>
    /// Pasta imediata do arquivo, formato: "NomePasta\" (sem caminho completo).
    /// Ex.: arquivo em Pangya\data\round20\file.ext → fdir = "round20\".
    /// </summary>
    public string fdir { get; set; } = "";
    public long fsize { get; set; }
    public int fcrc { get; set; }
    public string fdate { get; set; } = "";
    public string ftime { get; set; } = "";

    /// <summary>Nome do zip correspondente a este arquivo. Formato: "fname.zip".</summary>
    public string pname { get; set; } = "";

    /// <summary>Tamanho real do zip após compressão. Preenchido pelo UpdateMaker após zipar.</summary>
    public int psize { get; set; }

    // ── Campos auxiliares de RUNTIME (nunca escritos/lidos no XML) ───────────
    public string? FullPath { get; set; }

    /// <summary>
    /// MD5 de (nome + tamanho + data+3h) — usado para detectar mudanças sem
    /// recalcular CRC de todos os arquivos em checagens incrementais.
    /// </summary>
    public string? CheckSum { get; set; }

    /// <summary>Posição do arquivo na varredura do diretório (ordem de processamento).</summary>
    public int Index { get; set; }
}
