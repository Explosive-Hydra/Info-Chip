using HarmonyLib;
using System;
using System.Collections.Generic;
using MossLib.Tool;
using UnityEngine;

namespace InfoChip;

[HarmonyPatch(typeof(PlayerCamera))]
public static class PlayerCameraPatch
{
    private const string LocaleKeyPre = "hover.";
    public static Dictionary<string, List<Recipe>> ProductToRecipes = new();
    
    [HarmonyPatch("ItemHoverDescription")]
    [HarmonyPostfix]
    public static void Postfix(Item item, ref ValueTuple<string, string> __result)
    {
        if (item == null ||
            item.Stats?.rec is not { recognizable: true } ||
            !Input.GetKey(KeyBinds.GetBind("expanddesc")))
            return;

        string description = __result.Item2;
        string extraInfo = BuildTechnicalInfo(item);

        if (string.IsNullOrEmpty(extraInfo)) return;
        if (string.IsNullOrEmpty(description))
            __result.Item2 = extraInfo;
        else if (description.IndexOf(extraInfo, StringComparison.OrdinalIgnoreCase) < 0)
            __result.Item2 = $"{description.TrimEnd()}\n{extraInfo}";
    }

    private static string BuildTechnicalInfo(Item item)
    {
        ItemInfo info = item.Stats;
        if (info == null)
            return null;

        string result = "";

        if (ModLocale.HasLocaleKey(LocaleKeyPre + item.id))
        {
            result += "\n";
            result += Locale(item.id);
            result += "\n\n";
        }
        
        if (HasRecipe(item.id))
        {
            var recipes = GetRecipesByProduct(item.id);
            Plugin.Logger.LogInfo(recipes);
            Plugin.Logger.LogInfo(recipes.Count);
            // if (recipes == null || !recipes.Any())
            // {
            //     return null;
            // }
            //
            // string recipe = string.Join(", ", recipes);
            // result += recipe + "\n\n";
        }
        
        // 直接使用
        result += info.usable
            ? RichText.Green("✓ " + Locale("info.usable.true"))
            : RichText.Red("X  " + Locale("info.usable.false"));
        result += "\n";

        // 肢体使用
        result += info.usableOnLimb
            ? RichText.Green("✓ " + Locale("info.usable_on_limb.true"))
            : RichText.Red("X  " + Locale("info.usable_on_limb.false"));
        result += "\n";

        // 持续
        result += info.autoAttack
            ? Locale("info.auto_attack")
            : null;
        result += "\n";

        // 持续
        result += info.usableWithLMB
            ? Locale("info.usable_with_lrb")
            : null;
        result += "\n";

        // 无视抑郁
        result += info.ignoreDepression
            ? RichText.Color(Locale("info.ignore_depression"), "#FFFB91")
            : null;
        result += "\n";

        return string.IsNullOrEmpty(result.Trim())
            ? null
            : result.TrimEnd('\n');
    }

    private static bool HasRecipe(string productId)
    {
        return ProductToRecipes.ContainsKey(productId);
    }
    
    private static List<Recipe> GetRecipesByProduct(string productId)
    {
	    ProductToRecipes.TryGetValue(productId, out var list);
	    return list ?? [];
    }

    private static string Locale(string key, params object[] args)
    {
        return ModLocale.GetFormat($"{LocaleKeyPre}{key}", args);
    }
}