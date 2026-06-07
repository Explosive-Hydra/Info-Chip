using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using InfoChip.Lang;
using MossLib.Tool;

namespace InfoChip;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency("org.explosivehydra.mosslib")]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "org.explosivehydra.infochip";
    public const string Name = "Info Chip";
    public const string Version = "1.0.0";
    
    internal new static ManualLogSource Logger;
    private readonly Harmony _harmony = new(Guid);   
    internal static readonly Dictionary<string, ConfigEntryBase> ConfigRegistry = new();

    public static ConfigEntry<bool> CtrlToExpand;

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

        CtrlToExpand = RegisterConfig("ctrl_to_expand", true);
    }
    
    private ConfigEntry<T> RegisterConfig<T>(string key, T defaultValue)
    {
        var entry = Config.Bind("General", key, defaultValue, ConfigLocale($"{key}.description"));
        ConfigRegistry[key] = entry;
        return entry;
    }
    
    private static string ConfigLocale(string key)
    {
        return Locale($"config.{key}");
    }

    private static string Locale(string key)
    {
        return ModLocale.GetFormat(key);
    }
}