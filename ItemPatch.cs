using HarmonyLib;
using System;
using System.Linq;

namespace InfoChip;

[HarmonyPatch(typeof(Item))]
public static class ItemPatch
{
    [HarmonyPatch("SetupItems")]
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (Item.GlobalItems == null)
            return;

        foreach (var itemInfo
                 in from kvp
                     in Item.GlobalItems
                 let itemId = kvp.Key
                 select kvp.Value
                 into itemInfo
                 where itemInfo != null
                 select itemInfo)
        {
            if (AdditionalDescriptions.TryGetValue(itemId, out string desc))
            {
                itemInfo.description = AppendIfMissing(itemInfo.description ?? "", desc);
            }

            // 2. 如果物品是液体容器，追加液体特定信息
            // if (itemInfo is not LiquidItemInfo { defaultContents: not null } liquidItem) continue;
            // string meaningfulLiquidId = GetOnlyMeaningfulLiquidId(liquidItem);
            // if (string.IsNullOrEmpty(meaningfulLiquidId) ||
            //     !FluidAdditionalInfo.TryGetValue(meaningfulLiquidId, out var fluidInfo)) continue;
            // if (!string.IsNullOrEmpty(fluidInfo.drink))
            //     itemInfo.description = AppendIfMissing(itemInfo.description ?? "", fluidInfo.drink);
            // if (!string.IsNullOrEmpty(fluidInfo.inject))
            //     itemInfo.description = AppendIfMissing(itemInfo.description ?? "", fluidInfo.inject);
        }
    }

    private static string AppendIfMissing(string current, string addition)
    {
        if (string.IsNullOrWhiteSpace(addition) ||
            current.IndexOf(addition, StringComparison.OrdinalIgnoreCase) >= 0) return current;
        return string.IsNullOrWhiteSpace(current)
            ? addition
            : $"{current.TrimEnd()}\n\n{addition}";
    }
}