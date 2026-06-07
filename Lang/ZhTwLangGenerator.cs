using MossLib.Base;

namespace InfoChip.Lang;

public class ZhTwLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "zh-TW";

    protected override void BuildLocaleData()
    {
        Add("hover.info.usable.true", "可直接使用");
        Add("hover.info.usable.false", "不可直接使用");
        Add("hover.info.usable_on_limb.true", "可對肢體使用");
        Add("hover.info.usable_on_limb.false", "不可對肢體使用");
        Add("hover.info.auto_attack", "長按時持續使用");
        Add("hover.info.usable_with_lrb", "只能左鍵使用");
        Add("hover.info.ignore_depression", "無視抑鬱狀態");
        Add("hover.info.recipe", "合成配方: ");
    }
}