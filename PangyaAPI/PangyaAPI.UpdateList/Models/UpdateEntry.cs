namespace PangyaAPI.UpdateList.Models;

/// <summary>
/// Represents a file in the update list (an XTEA-encrypted .xml file).
///
/// The eight fields persisted in XML are fname through psize and exactly mirror
/// the attributes of the &lt;fileinfo&gt; element. CheckSum, Index, and FullPath
/// are runtime-only fields used by UpdateMaker during processing and are never
/// serialized.
///
/// Note: fdate/ftime must be written as LastWriteTime plus three hours (the
/// legacy Pangya convention applied by UpdateMaker when building the entry).
/// </summary>
public class UpdateEntry
{
    // ── Fields persisted in XML (<fileinfo .../>) ───────────────────────────
    public string fname { get; set; } = "";

    /// <summary>
    /// The file's immediate directory, formatted as "DirectoryName\" (not a full path).
    /// Example: a file at Pangya\data\round20\file.ext has fdir = "round20\".
    /// </summary>
    public string fdir { get; set; } = "";
    public long fsize { get; set; }
    public int fcrc { get; set; }
    public string fdate { get; set; } = "";
    public string ftime { get; set; } = "";

    /// <summary>The name of the ZIP file for this file, formatted as "fname.zip".</summary>
    public string pname { get; set; } = "";

    /// <summary>The actual ZIP size after compression, populated by UpdateMaker.</summary>
    public int psize { get; set; }

    // ── Runtime-only fields (never written to or read from XML) ──────────────
    public string? FullPath { get; set; }

    /// <summary>
    /// MD5 of (name + size + date+3h), used to detect changes without
    /// recalculating every file's CRC during incremental checks.
    /// </summary>
    public string? CheckSum { get; set; }

    /// <summary>The file's position in the directory scan (processing order).</summary>
    public int Index { get; set; }
}
