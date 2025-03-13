using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GeneralPurposeCommands.Patches
{
    [HarmonyPatch(typeof(ChatManager))]
    class ChatControl
    {
        public static InputAction Ctrl = new("Ctrl", InputActionType.Button, "<Keyboard>/ctrl");

        public static InputAction V = new("V", InputActionType.Button, "<Keyboard>/v");

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake()
        {
            Ctrl.Enable();
            V.Enable();
        }

        [HarmonyPatch("StateActive")]
        [HarmonyPostfix]
        public static void StateActive()
        {
            if (Ctrl.IsPressed() && V.WasPressedThisFrame())
            {
                string currentChatMessage = (string)typeof(ChatManager)
                    .GetField("chatMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(ChatManager.instance);
                typeof(ChatManager)
                    .GetField("chatMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(ChatManager.instance, currentChatMessage + GUIUtility.systemCopyBuffer);
                ChatManager.instance.chatText.text = currentChatMessage + GUIUtility.systemCopyBuffer;
                ChatUI.instance.SemiUITextFlashColor(Color.cyan, 0.2f);
                ChatUI.instance.SemiUISpringShakeY(2f, 5f, 0.2f);
                MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 1f, 0.2f, true);
            }
        }
    }
}
