using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using GeneralPurposeCommands.Patches;

namespace GeneralPurposeCommands;

[BepInPlugin(modGUID, modName, modVersion)]
public class GeneralPurposeCommands : BaseUnityPlugin
{
    private const string modGUID = "com.sirh.plugin.generalpurposecommands";
    private const string modName = "General Purpose Commands";
    private const string modVersion = "1.0.0";

    private readonly Harmony harmony = new(modGUID);

    private static GeneralPurposeCommands Instance;

    public static new ManualLogSource Logger;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"General Purpose Commands is loaded!");

        harmony.PatchAll(typeof(GeneralPurposeCommands));
        harmony.PatchAll(typeof(GeneralPurposeCommandsPatch));
    }
}
