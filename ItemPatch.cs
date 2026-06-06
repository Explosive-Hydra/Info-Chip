using HarmonyLib;
using System;
using System.Linq;

namespace InfoChip;

[HarmonyPatch(typeof(Item))]
public static class ItemPatch
{
    [HarmonyPatch("SetupItems")]
    [HarmonyPostfix]
    public static void SetupItemsPostfix()
    {
        if (Item.GlobalItems == null)
            return;

        var query = from kvp in Item.GlobalItems
            let itemId = kvp.Key
            let itemInfo = kvp.Value
            where itemInfo != null
            select new { itemId, itemInfo };

        foreach (var item in query)
        {
            string extra = BuildExtraInfo(item.itemInfo);
            if (!string.IsNullOrEmpty(extra))
                item.itemInfo.description = AppendIfMissing(
                    item.itemInfo.description ?? "", extra);
        }
    }

    private static string BuildExtraInfo(ItemInfo info)
    {
        string categoryLine = !string.IsNullOrEmpty(info.category)
            ? $"类型: {info.category}"
            : null;

        string usableLine = info.usable
            ? "✓ 可直接使用"
            : "X 不可直接使用";

        string usableOnLimbLine = info.usableOnLimb
            ? "✓ 可对肢体使用"
            : "X 不可对肢体使用";

        string wearableLine = null;
        if (info.wearable)
        {
            string armor = info.wearableArmor > 0f
                ? $" 护甲: {info.wearableArmor:F1}"
                : "";
            string isolation = info.wearableIsolation > 0f
                ? $" 隔热: {info.wearableIsolation:F1}"
                : "";
            wearableLine =
                $"✓ 可穿戴{armor}{isolation}";
        }

        string tagsLine = null;
        if (!string.IsNullOrEmpty(info.tags))
        {
            string[] tags = info.tags.Split(
                [','],
                StringSplitOptions.RemoveEmptyEntries);
            if (tags.Length > 0)
                tagsLine =
                    $"标签: {string.Join(", ", tags)}";
        }

        string[] lines =
        [
            categoryLine, usableLine, usableOnLimbLine,
            wearableLine, tagsLine
        ];
        string result = string.Join("\n",
            lines.Where(l => l != null));

        return string.IsNullOrEmpty(result)
            ? null
            : result;
    }

    private static string AppendIfMissing(string current, string addition)
    {
        if (string.IsNullOrWhiteSpace(addition)
            || current.IndexOf(addition,
                StringComparison.OrdinalIgnoreCase) >= 0)
            return current;

        return string.IsNullOrWhiteSpace(current)
            ? addition
            : $"{current.TrimEnd()}\n\n{addition}";
    }
}