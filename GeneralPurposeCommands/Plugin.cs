using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using GeneralPurposeCommands.Patches;
using UnityEngine;

namespace GeneralPurposeCommands;

[BepInPlugin(modGUID, modName, modVersion)]
public class GeneralPurposeCommands : BaseUnityPlugin
{
    private const string modGUID = "com.sirh.plugin.generalpurposecommands";
    private const string modName = "General Purpose Commands";
    private const string modVersion = "1.1.0";

    private readonly Harmony harmony = new(modGUID);

    private static GeneralPurposeCommands Instance;

    public static new ManualLogSource Logger;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"General Purpose Commands is loaded!");

        harmony.PatchAll(typeof(GeneralPurposeCommands));
        harmony.PatchAll(typeof(Commands));
        harmony.PatchAll(typeof(MessageSystem));
        harmony.PatchAll(typeof(ChatControl));
    }
}
