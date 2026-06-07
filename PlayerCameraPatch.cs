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
        if (item == null ||
            item.Stats?.rec is not { recognizable: true } ||
            !Input.GetKey(KeyBinds.GetBind("expanddesc")))
            return;

        string description = __result.Item2;
        string extraInfo = $"<color=#a2e8af><sprite index=2 tint=1><i>{ModLocale.GetFormat("key.shift_to_expand.down")}</i></color>\n" +
                           BuildTechnicalInfo(item);

        if (string.IsNullOrEmpty(extraInfo)) return;
        if (string.IsNullOrEmpty(description))
            __result.Item2 = extraInfo;
        else if (description.IndexOf(extraInfo, StringComparison.OrdinalIgnoreCase) < 0)
            __result.Item2 = $"{description.TrimEnd()}\n{extraInfo}";
    }

    private static string BuildTechnicalInfo(Item item)
    {
        ItemInfo info = item.Stats;
        bool ctrlDown = Input.GetKey(KeyCode.LeftControl) 
                    || Input.GetKey(KeyCode.RightControl);
        if (info == null)
            return null;

        string result = "";
        
        if (ModLocale.HasLocaleKey(LocaleKeyPre + item.id))
        {
            result += "\n";
            result += Locale(item.id);
            result += "\n\n";
        }
        
        if (!ctrlDown)
            result += $"<color=#a2e8af><sprite index=2 tint=1><i>{ModLocale.GetFormat("key.ctrl_to_expand.up")}</i></color>\n";
        else
        {
            result += $"<color=#a2e8af><sprite index=2 tint=1><i>{ModLocale.GetFormat("key.ctrl_to_expand.down")}</i></color>\n";
        }
        
        string recipeInfo = BuildRecipeString(item.id);
        if (ctrlDown
            || string.IsNullOrEmpty(recipeInfo))
        {
            result += recipeInfo + "\n\n";
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

        // 自动攻击
        result += info.autoAttack
            ? Locale("info.auto_attack") + "\n"
            : null;

        // 仅限左键
        result += info.usableWithLMB
            ? Locale("info.usable_with_lrb") + "\n"
            : null;

        // 无视抑郁
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

            // 渲染每种材料（去重后）
            foreach (var g in grouped)
            {
                var ri = g.Item;
                int count = g.Count;

                // 材料名称行
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

                nameLine = count > 1
                    ? $"  - {nameLine} x{count}"
                    : $"  - {nameLine}";
                blockLines.Add(nameLine);

                // 详细约束条件
                if (ri.isLiquid)
                {
                    switch (ri.specific)
                    {
                        case false when ri.quality != null:
                        {
                            string qLine = global::Locale.GetOther("craftliquidquality")
                                .Replace("<1>", ri.quality.amount.ToString("0.#"))
                                .Replace("<2>", ri.quality.LocaleName);
                            qLine = "    " + qLine;
                            blockLines.Add(qLine);
                            break;
                        }
                        case true when ri.minimumCondition > 0f:
                        {
                            string mlLine = global::Locale.GetOther("craftml")
                                .Replace("<>", ri.minimumCondition.ToString("0.#"));
                            mlLine = "    " + mlLine;
                            blockLines.Add(mlLine);
                            break;
                        }
                    }
                }
                else
                {
                    if (!ri.specific && ri.quality != null)
                    {
                        string qLine = global::Locale.GetOther("craftitemquality")
                            .Replace("<1>", ri.quality.amount.ToString("0.#"))
                            .Replace("<2>", ri.quality.LocaleName);
                        qLine = "    " + qLine;
                        blockLines.Add(qLine);

                        if (Recipes.QualityExamples != null)
                        {
                            var example = Recipes.QualityExamples
                                .FirstOrDefault(kvp =>
                                    kvp.Key.id == ri.quality.id &&
                                    Math.Abs(kvp.Key.amount - ri.quality.amount) < 0.001f);
                            if (example.Value != null)
                            {
                                string exLine = global::Locale.GetOther("craftexample")
                                    .Replace("<>", global::Locale.GetItem(example.Value));
                                exLine = "    " + exLine;
                                blockLines.Add(exLine);
                            }
                        }
                    }

                    if (!(ri.minimumCondition > 0f)) continue;
                    string condLine = global::Locale.GetOther("craftcondition")
                        .Replace("<>", (ri.minimumCondition * 100f).ToString("0.#"));
                    condLine = "    " + condLine;
                    blockLines.Add(condLine);
                }
            }

            if (blockLines.Count <= 0) continue;

            recipeBlocks.Add(string.Join("\n", blockLines));
        }

        return recipeBlocks.Count > 0
            ? RichText.White("\n" + 
                             Locale("info.recipe") + 
                             "\n" + 
                             string.Join("\n\n", recipeBlocks))
            : null;
    }

    private static bool HasRecipe(string productId)
    {
        return ProductToRecipes.ContainsKey(productId);
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