using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MossLib.Tool;
using UnityEngine;

namespace InfoChip;

[HarmonyPatch(typeof(PlayerCamera))]
public static class PlayerCameraPatch
{
    private const string LocaleKeyPre = "hover.";
    private static readonly Dictionary<string, List<Recipe>> ProductToRecipes = new();

    [HarmonyPatch("ItemHoverDescription")]
    [HarmonyPostfix]
    public static void Postfix(Item item, ref ValueTuple<string, string> __result)
    {
        if (item == null || item.Stats?.rec is not { recognizable: true })
            return;

        // Shift 没按住时原版显示"按住Shift展开"，不干涉
        if (!Input.GetKey(KeyBinds.GetBind("expanddesc")))
            return;

        string description = __result.Item2;
        string extraInfo = BuildTechnicalInfo(item);
        if (string.IsNullOrEmpty(extraInfo)) return;

        // Shift 按住时原版"按住Shift展开"消失，加上"松开Shift"替代
        string hint = $"<color=#a2e8af><sprite index=2 tint=1><i>{ModLocale.GetFormat("key.shift_to_expand.down")}</i></color>\n";
        extraInfo = hint + extraInfo;

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

        bool needCtrl = Plugin.CtrlToExpand.Value;
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        string result = "";

        // 物品专属描述
        if (ModLocale.HasLocaleKey(LocaleKeyPre + item.id))
        {
            result += "\n";
            result += Locale(item.id);
            result += "\n\n";
        }

        // Ctrl 提示行 + 配方（二级展开）
        string recipeInfo = BuildRecipeString(item.id);
        bool showRecipe = !needCtrl || ctrlHeld;

        if (needCtrl)
        {
            string ctrlHint = ctrlHeld
                ? ModLocale.GetFormat("key.ctrl_to_expand.down")   // "松开Ctrl折叠更多信息"
                : ModLocale.GetFormat("key.ctrl_to_expand.up");    // "按住Ctrl展开更多信息"
            result += $"<color=#a2e8af><sprite index=2 tint=1><i>{ctrlHint}</i></color>\n";
        }

        if (showRecipe && !string.IsNullOrEmpty(recipeInfo))
            result += recipeInfo + "\n\n";

        // 技术标志（始终显示）
        result += info.usable
            ? RichText.Green("✓ " + Locale("info.usable.true"))
            : RichText.Red("X  " + Locale("info.usable.false"));
        result += "\n";

        result += info.usableOnLimb
            ? RichText.Green("✓ " + Locale("info.usable_on_limb.true"))
            : RichText.Red("X  " + Locale("info.usable_on_limb.false"));
        result += "\n";

        result += info.autoAttack
            ? Locale("info.auto_attack") + "\n"
            : null;

        result += info.usableWithLMB
            ? Locale("info.usable_with_lrb") + "\n"
            : null;

        result += info.ignoreDepression
            ? RichText.Color(Locale("info.ignore_depression"), "#FFFB91") + "\n"
            : null;

        return string.IsNullOrEmpty(result.Trim())
            ? null
            : result.TrimEnd('\n');
    }

    private static string BuildRecipeString(string itemId)
    {
        var recipes = GetRecipesByProduct(itemId);
        if (recipes == null || recipes.Count == 0)
            return null;

        var recipeBlocks = new List<string>();

        foreach (var recipe in recipes)
        {
            if (recipe?.items == null || recipe.items.Count == 0)
                continue;

            // 合并相同材料
            var grouped = recipe.items
                .Where(ri => ri != null)
                .GroupBy(ri => new
                {
                    ri.specific,
                    ri.specificId,
                    ri.isLiquid,
                    qualityId = ri.quality?.id,
                    qualityAmount = Math.Round(ri.quality?.amount ?? 0f, 4),
                    ri.minimumCondition,
                    ri.destroyItem
                })
                .Select(g => new { Item = g.First(), Count = g.Count() })
                .ToList();

            var blockLines = new List<string>();

            // 渲染每种材料（去重后），约束条件内联在名称后面
            foreach (var g in grouped)
            {
                var ri = g.Item;
                int count = g.Count;

                // 材料名称
                string nameLine;
                if (!ri.specific)
                {
                    if (ri.isLiquid)
                        nameLine = global::Locale.GetOther("craftanyliquid");
                    else if (ri.quality is { id: "hammering" or "cutting" })
                        nameLine = global::Locale.GetOther("craftanytool");
                    else
                        nameLine = global::Locale.GetOther("craftanyitem");
                }
                else
                {
                    nameLine = ri.isLiquid
                        ? global::Locale.GetOther(ri.specificId)
                        : global::Locale.GetItem(ri.specificId);
                }

                // 数量后缀
                if (count > 1)
                    nameLine += $" x{count}";

                // 内联约束条件
                var constraints = new List<string>();

                if (ri.isLiquid)
                {
                    switch (ri.specific)
                    {
                        case false when ri.quality != null:
                        {
                            string q = global::Locale.GetOther("craftliquidquality")
                                .Replace("<1>", ri.quality.amount.ToString("0.#"))
                                .Replace("<2>", ri.quality.LocaleName);
                            constraints.Add(q);
                            break;
                        }
                        case true when ri.minimumCondition > 0f:
                        {
                            string m = global::Locale.GetOther("craftml")
                                .Replace("<>", ri.minimumCondition.ToString("0.#"));
                            constraints.Add(m);
                            break;
                        }
                    }
                }
                else
                {
                    if (!ri.specific && ri.quality != null)
                    {
                        string q = global::Locale.GetOther("craftitemquality")
                            .Replace("<1>", ri.quality.amount.ToString("0.#"))
                            .Replace("<2>", ri.quality.LocaleName);
                        constraints.Add(q);

                        if (Recipes.QualityExamples != null)
                        {
                            var example = Recipes.QualityExamples
                                .FirstOrDefault(kvp =>
                                    kvp.Key.id == ri.quality.id &&
                                    Math.Abs(kvp.Key.amount - ri.quality.amount) < 0.001f);
                            if (example.Value != null)
                            {
                                string ex = global::Locale.GetOther("craftexample")
                                    .Replace("<>", global::Locale.GetItem(example.Value));
                                constraints.Add(ex);
                            }
                        }
                    }

                    if (ri.minimumCondition > 0f)
                    {
                        string c = global::Locale.GetOther("craftcondition")
                            .Replace("<>",
                                PlayerCamera.ConditionToColorCode(ri.minimumCondition) +
                                (ri.minimumCondition * 100f).ToString("0.#") +
                                "</color>"
                            );
                        constraints.Add(c);
                    }
                }

                if (constraints.Count > 0)
                    nameLine += " " + string.Join(" ", constraints);

                blockLines.Add($"  - {nameLine}");
            }

            if (blockLines.Count <= 0) continue;

            recipeBlocks.Add(string.Join("\n", blockLines));
        }

        return recipeBlocks.Count > 0
            ? RichText.White("\n" +
                             Locale("info.recipe") +
                             "\n" +
                             string.Join("\n", recipeBlocks))
            : null;
    }

    private static void EnsureRecipeLookup()
    {
        if (ProductToRecipes.Count > 0)
            return;
        if (Recipes.recipes == null || Recipes.recipes.Count == 0)
            return;
        foreach (var recipe in Recipes.recipes)
        {
            if (recipe?.result == null || string.IsNullOrEmpty(recipe.result.id))
                continue;
            string pid = recipe.result.id;
            if (!ProductToRecipes.ContainsKey(pid))
                ProductToRecipes[pid] = [];
            ProductToRecipes[pid].Add(recipe);
        }
    }

    private static List<Recipe> GetRecipesByProduct(string productId)
    {
        EnsureRecipeLookup();
        ProductToRecipes.TryGetValue(productId, out var list);
        return list ?? [];
    }

    private static string Locale(string key, params object[] args)
    {
        return ModLocale.GetFormat($"{LocaleKeyPre}{key}", args);
    }
}