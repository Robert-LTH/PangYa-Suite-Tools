namespace PangyaAPI.UpdateList.Models;

/// <summary>
/// Defines all attributes of the update list's &lt;fileinfo&gt; element in one
/// place. UpdateReader and UpdateWriter iterate over this list, so adding a
/// field here automatically propagates it to both reading and writing without
/// risking divergence between them.
/// </summary>
public static class UpdateEntryFieldMap
{
    public static readonly IReadOnlyList<UpdateEntryField> Fields = new[]
    {
        new UpdateEntryField("fname", e => e.fname,        (e, v) => e.fname = v),
        new UpdateEntryField("fdir",  e => e.fdir,         (e, v) => e.fdir  = v),
        new UpdateEntryField("fsize", e => e.fsize.ToString(), (e, v) => e.fsize = ParseLong(v)),
        new UpdateEntryField("fcrc",  e => e.fcrc.ToString(),  (e, v) => e.fcrc  = ParseInt(v)),
        new UpdateEntryField("fdate", e => e.fdate,        (e, v) => e.fdate = v),
        new UpdateEntryField("ftime", e => e.ftime,        (e, v) => e.ftime = v),
        new UpdateEntryField("pname", e => e.pname,        (e, v) => e.pname = v),
        new UpdateEntryField("psize", e => e.psize.ToString(), (e, v) => e.psize = ParseInt(v)),
    };

    private static long ParseLong(string v) => long.TryParse(v, out var r) ? r : 0L;
    private static int  ParseInt(string v)  => int.TryParse(v,  out var r) ? r : 0;
}

/// <summary>Defines one &lt;fileinfo&gt; attribute: its XML name and typed getter/setter.</summary>
public sealed class UpdateEntryField
{
    public string XmlAttributeName { get; }
    public Func<UpdateEntry, string>   Get { get; }
    public Action<UpdateEntry, string> Set { get; }

    public UpdateEntryField(string xmlAttributeName,
                            Func<UpdateEntry, string> get,
                            Action<UpdateEntry, string> set)
    {
        XmlAttributeName = xmlAttributeName;
        Get = get;
        Set = set;
    }
}
