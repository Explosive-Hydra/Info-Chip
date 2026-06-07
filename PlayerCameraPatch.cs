using HarmonyLib;
using System;
using MossLib.Tool;
using UnityEngine;

namespace InfoChip;

[HarmonyPatch(typeof(PlayerCamera))]
public static class PlayerCameraPatch
{
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

        if (!string.IsNullOrEmpty(extraInfo))
        {
            if (string.IsNullOrEmpty(description))
                __result.Item2 = extraInfo;
            else if (description.IndexOf(extraInfo, StringComparison.OrdinalIgnoreCase) < 0)
                __result.Item2 = $"{description.TrimEnd()}\n{extraInfo}";
        }
    }

    private static string BuildTechnicalInfo(Item item)
    {
        ItemInfo info = item.Stats;
        if (info == null)
            return null;

        string result = "";

        // 直接使用
        result += info.usable
            ? RichText.Green("✓ 可直接使用\n")
            : RichText.Red("X  不可直接使用\n");

        // 肢体使用
        result += info.usableOnLimb
            ? RichText.Green("✓ 可对肢体使用\n")
            : RichText.Red("X  不可对肢体使用\n");

        // 持续
        result += info.autoAttack
            ? "长按时持续使用\n"
            : "";

        // 持续
        result += info.usableWithLMB
            ? "只能左键使用\n"
            : "";

        // 无视抑郁
        result += info.ignoreDepression
            ? RichText.Color("无视抑郁状态\n", "#FFFB91")
            : null;

        return string.IsNullOrEmpty(result.Trim())
            ? null
            : result.TrimEnd('\n');
    }
}