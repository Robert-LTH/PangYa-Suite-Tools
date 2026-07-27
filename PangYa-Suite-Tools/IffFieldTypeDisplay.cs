using PangYa_Suite_Tools.Localization;
using PangyaAPI.IFF;

namespace PangYa_Suite_Tools;

internal static class IffFieldTypeDisplay
{
    public static string GetName(IffFieldType type) => type == IffFieldType.Icon
        ? Strings.IFFManager_FieldTypeImageResource
        : type.ToString();

    public static void FormatComboBoxItem(object? sender, ListControlConvertEventArgs e)
    {
        if (e.ListItem is IffFieldType type) e.Value = GetName(type);
    }
}
