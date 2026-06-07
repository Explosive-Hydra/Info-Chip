using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using InfoChip.Lang;
using MossLib.Tool;

namespace InfoChip;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency("org.explosivehydra.mosslib")]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    public const string Guid = "org.explosivehydra.infochip";
    public const string Name = "Info Chip";
    public const string Version = "1.0.0";
    private readonly Harmony _harmony = new(Guid);

    public void Awake()
    {
        Logger = base.Logger;

        LocaleGenerator.SetLogger(Logger);
        LocaleGenerator.Register(new EnLangGenerator(), Logger);
        LocaleGenerator.Register(new ZhCnLangGenerator(), Logger);
        LocaleGenerator.Register(new ZhTwLangGenerator(), Logger);
        LocaleGenerator.GenerateAll();
        
        _harmony.PatchAll();
        ModLocale.Initialize(Logger);
    }
}