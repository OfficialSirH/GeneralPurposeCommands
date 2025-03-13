using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GeneralPurposeCommands.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    [HarmonyPatch("Awake")]
    class MessageSystem : MonoBehaviour
    {
        public static MessageSystem Instance;

        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Instance == null)
            {
                Instance = new GameObject("MessageSystem").AddComponent<MessageSystem>();
                return;
            }
            Destroy(Instance.gameObject);
        }

        public new void SendMessage(string message)
        {
            StartCoroutine(SendMessageCoroutine(message));
        }

        public static IEnumerator SendMessageCoroutine(string message)
        {
            yield return new WaitForSeconds(2);
            PlayerAvatar.instance.ChatMessageSend(message, false);
            yield break;
        }
    }
}
