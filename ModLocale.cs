using System.Reflection;
using BepInEx.Logging;
using MossLib.Base;

namespace InfoChip;

public class ModLocale : ModLocaleBase
{
    private static ModLocale _instance;

    public static void Initialize(ManualLogSource logger)
    {
        if (_instance != null)
            return;
        _instance = new ModLocale();
        _instance.Initialize(logger, Assembly.GetExecutingAssembly());
    }

    public static string GetFormat(string key, params object[] args)
    {
        return _instance.GetStringFormatted(key, args);
    }
}