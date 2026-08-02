using System.Text;
using System.Text.RegularExpressions;

namespace PangyaAPI.UI;

internal static class PangyaXmlCompatibility
{
    public static Encoding DetectEncoding(string path)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = new StreamReader(path, Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);
        char[] buffer = new char[512];
        int count = reader.Read(buffer, 0, buffer.Length);
        string header = new(buffer, 0, count);
        Match match = Regex.Match(header,
            """encoding\s*=\s*["'](?<value>[^"']+)["']""",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (match.Success)
        {
            try { return Encoding.GetEncoding(match.Groups["value"].Value); }
            catch (ArgumentException) { }
        }
        return reader.CurrentEncoding;
    }

    public static string EscapeBareAmpersands(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        var output = new StringBuilder(xml.Length);
        for (int index = 0; index < xml.Length;)
        {
            if (xml.AsSpan(index).StartsWith("<!--", StringComparison.Ordinal))
            {
                CopyMarkupBlock(xml, output, ref index, "-->");
                continue;
            }
            if (xml.AsSpan(index).StartsWith("<![CDATA[", StringComparison.Ordinal))
            {
                CopyMarkupBlock(xml, output, ref index, "]]>");
                continue;
            }
            if (xml.AsSpan(index).StartsWith("<?", StringComparison.Ordinal))
            {
                CopyMarkupBlock(xml, output, ref index, "?>");
                continue;
            }

            if (xml[index] == '&' && !IsValidEntityReference(xml.AsSpan(index)))
                output.Append("&amp;");
            else
                output.Append(xml[index]);
            index++;
        }
        return output.ToString();
    }

    private static bool IsValidEntityReference(ReadOnlySpan<char> value)
    {
        if (value.StartsWith("&amp;", StringComparison.Ordinal) ||
            value.StartsWith("&lt;", StringComparison.Ordinal) ||
            value.StartsWith("&gt;", StringComparison.Ordinal) ||
            value.StartsWith("&apos;", StringComparison.Ordinal) ||
            value.StartsWith("&quot;", StringComparison.Ordinal))
            return true;
        if (value.Length < 4 || value[1] != '#') return false;

        int index = 2;
        bool hexadecimal = index < value.Length && value[index] is 'x' or 'X';
        if (hexadecimal) index++;
        int digitStart = index;
        while (index < value.Length && (hexadecimal
                   ? Uri.IsHexDigit(value[index])
                   : char.IsAsciiDigit(value[index])))
            index++;
        return index > digitStart && index < value.Length && value[index] == ';';
    }

    private static void CopyMarkupBlock(string xml, StringBuilder output,
        ref int index, string terminator)
    {
        int end = xml.IndexOf(terminator, index, StringComparison.Ordinal);
        if (end < 0)
        {
            output.Append(xml, index, xml.Length - index);
            index = xml.Length;
            return;
        }
        int length = end - index + terminator.Length;
        output.Append(xml, index, length);
        index += length;
    }
}
