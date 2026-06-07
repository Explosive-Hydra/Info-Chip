using MossLib.Base;
using MossLib.Constant;

namespace InfoChip.Lang;

public class ZhCnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-CN";

    protected override void BuildLocaleData()
    {
        Add("hover.info.usable.true", "可直接使用");
        Add("hover.info.usable.false", "不可直接使用");
        Add("hover.info.usable_on_limb.true", "可对肢体使用");
        Add("hover.info.usable_on_limb.false", "不可对肢体使用");
        Add("hover.info.auto_attack", "长按时持续使用");
        Add("hover.info.usable_with_lrb", "只能左键使用");
        Add("hover.info.ignore_depression", "无视抑郁状态");
        Add("hover.info.recipe", "合成配方: ");
        Add($"item.{Items._9MmRound}", "⑨mm子弹");
        Add($"hover.{Items._9MmRound}", "⑨毫米子弹");
    }
}