using MossLib.Base;

namespace InfoChip.Lang;

public class EnLangGenerator : ModLangGenBase
{
    protected override string LanguageCode => "EN";

    protected override void BuildLocaleData()
    {
        Add("hover.info.usable.true", "Can be used directly");
        Add("hover.info.usable.false", "Cannot be used directly");
        Add("hover.info.usable_on_limb.true", "Can be used on limbs");
        Add("hover.info.usable_on_limb.false", "Cannot be used on limbs");
        Add("hover.info.auto_attack", "Continuous use when long press");
        Add("hover.info.usable_with_lrb", "Can only be used with left click");
        Add("hover.info.ignore_depression", "Ignore depression status");
    }
}