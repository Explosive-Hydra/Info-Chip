using HarmonyLib;
using System;
using MossLib.Tool;
using UnityEngine;

namespace InfoChip;

[HarmonyPatch(typeof(PlayerCamera))]
public static class PlayerCameraPatch
{
    private const string LocaleKeyPre = "hover.";
    
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

        if (ModLocale.HasKey(LocaleKeyPre + item.id))
        {
            result += Locale(item.id);
            result += "\n";
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

    private static string Locale(string key, params object[] args)
    {
        return ModLocale.GetFormat($"{LocaleKeyPre}{key}", args);
    }
}