using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using GeneralPurposeCommands.Patches;
using UnityEngine;

namespace GeneralPurposeCommands;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

    private static Plugin Instance;

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

        harmony.PatchAll(typeof(Plugin));
        harmony.PatchAll(typeof(Commands));
        harmony.PatchAll(typeof(MessageSystem));
        harmony.PatchAll(typeof(ChatControl));
    }
}
