using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeneralPurposeCommands.Utilities;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using static UnityEngine.InputSystem.InputRemoting;

namespace GeneralPurposeCommands.Patches
{
    [HarmonyPatch(typeof(SemiFunc))]
    [HarmonyPatch("Command")]
    public class Commands : MonoBehaviour
    {
        [HarmonyPostfix]
        public static void Additional_Command(string _command)
        {
            string[] input = _command.Trim().Split(" ");
            string command = input[0].ToLower();
            string[] args = input.Length >= 2 ? new Span<string>(input, 1, input.Length - 1).ToArray() : [];

            Result<string> result = command switch
            {
                "addmoney" => AddMoney(args),
                "listitems" => ListItems(),
                "spawnitem" => SpawnItem(args),
                _ => Result<string>.Err("NO_COMMAND_CALLED")
            };

            try
            {
                result.Match(
                   Ok: result => MessageSystem.Instance.SendMessage(result.Value),
                   Err: err =>
                   {
                       if (!err.ErrorMessage.Equals("NO_COMMAND_CALLED"))
                       {
                           MessageSystem.Instance.SendMessage(err.ErrorMessage);
                       }
                   }
                   );
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        //public static new IEnumerator SendMessage(string message)
        //{
        //    yield return new WaitForSeconds(2);
        //    PlayerAvatar.instance.ChatMessageSend(message, false);
        //    yield break;
        //}

        public static Result<string> AddMoney(string[] args)
        {
            if (args.Length == 0)
            {
                string errorMessage = "Amount is missing.";
                Plugin.Logger.LogError(errorMessage);
                return Result<string>.Err(errorMessage);
            }

            string unparsedAmount = args[0];
            bool isParsingSuccessful = int.TryParse(unparsedAmount, out int amount);

            if (!isParsingSuccessful)
            {
                string errorMessage = "Amount must be an integer type number.";
                Plugin.Logger.LogError(errorMessage);
                return Result<string>.Err(errorMessage);
            }

            SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() + amount);

            string message = $"New Total: {SemiFunc.StatGetRunCurrency()}";
            Plugin.Logger.LogInfo(message);
            return Result<string>.Ok(message);
        }

        private static Result<string> ListItems()
        {
            string message = "Items:\n";
            foreach (Item value in StatsManager.instance.itemDictionary.Values)
            {
                message += value.itemName + "\n";
            }
            message += "\nValuables:\n";
            string[] shapeCategories = ["01 Tiny", "02 Small", "03 Medium", "04 Big", "05 Wide", "06 Tall", "07 Very Tall"];
            foreach (string shapeCategory in shapeCategories)
            {
                foreach (GameObject valuable in Resources.LoadAll<GameObject>("Valuables/" + shapeCategory))
                {
                    message += valuable.name.Replace("Valuable ", string.Empty) + "\n";
                }
            }
            message += "\nRemoved Items:\n";
            foreach (GameObject removedItem in Resources.LoadAll<GameObject>("Items/Removed Items"))
            {
                message += removedItem.name + "\n";
            }
            Debug.Log(message);
            return Result<string>.Ok("Check Logs");
        }

        private static Result<string> SpawnItem(string[] args)
        {
            int spawnCount = 1;
            string searchName = "";
            if (args.Length > 0)
            {
                bool isSuccessful = int.TryParse(args[^1], out spawnCount);
                if (isSuccessful)
                {
                    searchName = args[..^1].Aggregate((x, y) => x + " " + y);
                }
                else
                {
                    searchName = args.Aggregate((x, y) => x + " " + y);
                    spawnCount = 1;
                }
            }

            if (string.IsNullOrEmpty(searchName))
            {
                Debug.LogWarning("Item name is empty!");
                return Result<string>.Err("nuh uh");
            }
            searchName = searchName.Trim();
            Item val = null;
            foreach (Item value in StatsManager.instance.itemDictionary.Values)
            {
                if (string.Equals(value.itemAssetName, searchName, StringComparison.OrdinalIgnoreCase) || string.Equals(value.itemName.Trim(), searchName, StringComparison.OrdinalIgnoreCase))
                {
                    val = value;
                    break;
                }
            }
            Transform transform = Camera.main.transform;
            Vector3 spawnLocation = transform.position + transform.forward * 2f;
            Quaternion identity = Quaternion.identity;
            if (val != null)
            {
                if (val.prefab == null)
                {
                    Debug.LogWarning(("Item prefab is null for: " + searchName));
                    return Result<string>.Err("nuh uh");
                }
                string text = "Valuables/" + val.prefab.name;
                GameObject val3 = Resources.Load<GameObject>(text);
                string itemCategory = val3 != null ? "Valuables/" : "Items/";
                Debug.Log(val3 != null ? ("Found valuable prefab in Resources/Valuables. Using '" + itemCategory + "'.") : ("Valuable prefab not found in Resources/Valuables; using '" + itemCategory + "' folder."));
                if (SemiFunc.IsMultiplayer())
                {
                    for (int i = 0; i < spawnCount; i++)
                        InstantiateRoomObject(itemCategory + val.prefab.name, spawnLocation + transform.up * i * 0.2f, identity);
                }
                else
                {
                    GameObject val4 = Resources.Load<GameObject>(itemCategory + val.prefab.name);
                    if (val4 != null)
                    {
                        for (int i = 0; i < spawnCount; i++)
                            Instantiate(val4, spawnLocation + transform.up * i * 0.2f, identity);
                    }
                    else
                    {
                        Debug.LogWarning("Prefab not found for singleplayer spawn: " + itemCategory + val.prefab.name);
                    }
                }
                string itemName = val.itemName;
                Debug.Log("Spawned item: " + itemName + " at " + spawnLocation.ToString());
                return Result<string>.Ok("yippee");
            }
            Debug.Log("Item not found in StatsManager; searching Resources subfolders for a valuable prefab.");
            if (TryLoadValuablePrefab(searchName, out string foundPath) != null)
            {
                if (SemiFunc.IsMultiplayer())
                {
                    for (int i = 0; i < spawnCount; i++)
                        InstantiateRoomObject(foundPath, spawnLocation + transform.up * i * 0.2f, identity);
                }
                else
                {
                    GameObject loadedValuable = Resources.Load<GameObject>(foundPath);
                    if (loadedValuable != null)
                    {
                        for (int i = 0; i < spawnCount; i++)
                            Instantiate(loadedValuable, spawnLocation + transform.up * i * 0.2f, identity);
                    }
                    else
                    {
                        Debug.LogWarning("Prefab not found for singleplayer spawn: " + foundPath);
                    }
                }
                Debug.Log("Spawned valuable from path: " + foundPath + " at " + spawnLocation.ToString());
                return Result<string>.Ok("yippee");
            }

            Debug.LogWarning("No item or valuable found with name: " + searchName);
            return Result<string>.Err("nuh uh");
        }

        private static GameObject TryLoadValuablePrefab(string searchName, out string foundPath)
        {
            foundPath = "";
            string[] shapeCategories = ["01 Tiny", "02 Small", "03 Medium", "04 Big", "05 Wide", "06 Tall", "07 Very Tall"];
            foreach (string shapeCategory in shapeCategories)
            {
                string valuablePath = "Valuables/" + shapeCategory + "/";
                foreach (GameObject valuable in Resources.LoadAll<GameObject>("Valuables/" + shapeCategory))
                {
                    string valuableName = valuable.name.Replace("Valuable ", string.Empty);
                    if (string.Equals(valuableName, searchName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundPath = valuablePath + valuable.name;
                        return valuable;
                    }
                }
            }
            string itemsPath = "Items/Removed Items/" + searchName;
            GameObject loadedItemPrefab = Resources.Load<GameObject>(itemsPath);
            if (loadedItemPrefab != null)
            {
                foundPath = itemsPath;
                return loadedItemPrefab;
            }
            return null;
        }

        // This method is a copy of the original Instantiate method from PhotonNetwork but avoids checking if the user is the host.
        public static GameObject InstantiateRoomObject(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null)
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                Debug.LogError("Can not Instantiate before the client joined/created a room.");
                return null;
            }

            try
            {
                return (GameObject)
                    GetMethod(typeof(PhotonNetwork), "NetworkInstantiate", typeof(InstantiateParameters))
                    .Invoke(null, [new InstantiateParameters(prefabName, position, rotation, group, data, 0, null, PhotonNetwork.LocalPlayer, PhotonNetwork.ServerTimestamp), true, false]);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        public static MethodInfo GetMethod(Type type, String name, Type uniqueParamType)
        {
            IEnumerable<MethodInfo> m = type.GetRuntimeMethods().Where(x => x.Name.Equals(name));
            return (from r in m let p = r.GetParameters() where p.Any(o => uniqueParamType.IsAssignableFrom(o.ParameterType)) select r).ToList()[0];
        }
    }
}
