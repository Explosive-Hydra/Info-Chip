using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

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
        _harmony.PatchAll();

        Logger.LogInfo("Info Chip loaded!");
    }
}