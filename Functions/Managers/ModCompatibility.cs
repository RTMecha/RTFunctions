using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;

using BeatmapObject = DataManager.GameData.BeatmapObject;
using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;
using Prefab = DataManager.GameData.Prefab;

namespace RTFunctions.Functions.Managers
{
    public class ModCompatibility : MonoBehaviour
    {
        //Move this to RTFunctions.Functions.Managers

        public static ModCompatibility inst;

        public static GameObject bepinex;

        public static Dictionary<string, object> sharedFunctions = new Dictionary<string, object>();

        private void Awake()
        {
            inst = this;

            bepinex = GameObject.Find("BepInEx_Manager");

            if (bepinex.GetComponentByName("ObjectModifiersPlugin"))
            {
                objectModifiersPlugin = bepinex.GetComponentByName("ObjectModifiersPlugin").GetType();
            }

            if (catalyst == null && catalystType == CatalystType.NotChecked)
            {
                if (!bepinex.GetComponentByName("CatalystBase"))
                {
                    catalystType = CatalystType.NotInstalled;
                }
                else
                {
                    catalystType = CatalystType.Regular;
                    catalyst = bepinex.GetComponentByName("CatalystBase").GetType();
                    catalystInstance = catalyst.GetField("Instance").GetValue(bepinex.GetComponentByName("CatalystBase"));

                    if ((string)catalyst.GetField("Name").GetValue(catalyst) == "Editor Catalyst")
                    {
                        catalystType = CatalystType.Editor;
                    }
                }
            }

            if (bepinex.GetComponentByName("ArcadePlugin"))
            {
                arcadePlugin = bepinex .GetComponentByName("ArcadePlugin").GetType();
            }

            if (bepinex.GetComponentByName("PlayerPlugin"))
            {
                player = AccessTools.TypeByName("RTPlayer");
                playerPlugin = bepinex.GetComponentByName("PlayerPlugin").GetType();
            }

            if (bepinex.GetComponentByName("EditorPlugin"))
            {
                rtEditor = AccessTools.TypeByName("RTEditor");
                editorPlugin = bepinex.GetComponentByName("EditorPlugin").GetType();
            }

            if (bepinex.GetComponentByName("FontPlugin"))
            {
                fontPlugin = bepinex.GetComponentByName("FontPlugin").GetType();
            }
        }

        #region CreativePlayers

        public static Type player;
        public static Type playerPlugin;

        public static object GetRTPlayer(int index)
        {
            return player.GetMethod("GetInstance").Invoke(player, new object[] { index });
        }

        public static void SetPlayerModel(int index, string id)
        {
            playerPlugin.GetMethod("SetPlayerModel").Invoke(playerPlugin, new object[] { index, id });
        }

        public static void ClearPlayerModels()
        {
            playerPlugin.GetMethod("ClearPlayerModels").Invoke(playerPlugin, new object[] { });
        }

        #endregion

        #region ObjectModifiers

        public static Type objectModifiersPlugin;

        public static void AddModifierObject(BeatmapObject _beatmapObject)
        {
            if (objectModifiersPlugin == null)
                return;

            if (!_beatmapObject.fromPrefab)
            {
                objectModifiersPlugin.GetMethod("AddModifierObject").Invoke(objectModifiersPlugin, new object[] { _beatmapObject });
            }
        }

        public static void RemoveModifierObject(BeatmapObject _beatmapObject)
        {
            if (objectModifiersPlugin == null)
                return;

            objectModifiersPlugin.GetMethod("RemoveModifierObject").Invoke(objectModifiersPlugin, new object[] { _beatmapObject });
        }

        public static void ClearModifierObjects()
        {
            if (objectModifiersPlugin == null)
                return;

            objectModifiersPlugin.GetMethod("ClearModifierObjects").Invoke(objectModifiersPlugin, new object[] { });
        }

        public static object GetModifierIndex(BeatmapObject _beatmapObject, int index)
        {
            if (objectModifiersPlugin == null)
                return null;

            return objectModifiersPlugin.GetMethod("GetModifierIndex").Invoke(objectModifiersPlugin, new object[] { _beatmapObject, index });
        }

        public static int GetModifierCount(BeatmapObject _beatmapObject)
        {
            if (objectModifiersPlugin == null)
                return 0;

            return (int)objectModifiersPlugin.GetMethod("GetModifierCount").Invoke(objectModifiersPlugin, new object[] { _beatmapObject });
        }

        public static object GetModifierObject(BeatmapObject _beatmapObject)
        {
            if (objectModifiersPlugin == null)
                return null;

            return objectModifiersPlugin.GetMethod("GetModifierObject").Invoke(objectModifiersPlugin, new object[] { _beatmapObject });
        }

        public static void RemoveModifierIndex(BeatmapObject _beatmapObject, int index)
        {
            if (objectModifiersPlugin == null)
                return;

            objectModifiersPlugin.GetMethod("RemoveModifierIndex").Invoke(objectModifiersPlugin, new object[] { _beatmapObject, index });
            //ObjectManager.inst.updateObjects(ObjEditor.inst.currentObjectSelection);
        }

        public static void AddModifierToObject(BeatmapObject _beatmapObject, int index)
        {
            if (objectModifiersPlugin == null)
                return;

            objectModifiersPlugin.GetMethod("AddModifierToObject").Invoke(objectModifiersPlugin, new object[] { _beatmapObject, index });
            //ObjectManager.inst.updateObjects(ObjEditor.inst.currentObjectSelection);
        }

        public static int GetModifierVariable(BeatmapObject _beatmapObject)
        {
            if (objectModifiersPlugin == null)
                return 0;

            return (int)objectModifiersPlugin.GetMethod("GetModifierVariable").Invoke(objectModifiersPlugin, new object[] { _beatmapObject });
        }

        #endregion

        #region Catalyst

        public static Type catalyst;
        public static CatalystType catalystType;
        public static object catalystInstance;
        public enum CatalystType
        {
            NotChecked,
            NotInstalled,
            Regular,
            Editor
        }

        public static void updateCatalystObject(BeatmapObject _beatmapObject)
        {

        }

        #endregion

        #region ArcadiaCustoms

        public static Type arcadePlugin;

        public static void AddLevel(string path)
        {
            if (arcadePlugin == null)
                return;

            arcadePlugin.GetMethod("AddLevel").Invoke(arcadePlugin, new object[] { path });
        }

        #endregion

        #region EventsCore

        public static Type eventsCorePlugin;

        #endregion

        #region EditorManagement

        public static Type rtEditor;
        public static Type editorPlugin;

        #endregion

        #region AdditionalFonts

        public static Type fontPlugin;

        public static Font GetFont(string _name)
        {
            if (fontPlugin == null)
                return Font.GetDefault();

            var dictionary = (Dictionary<string, Font>)fontPlugin.GetType().GetField("allFonts").GetValue(fontPlugin);
            if (dictionary.ContainsKey(_name))
                return dictionary[_name];
            return Font.GetDefault();
        }

        #endregion
    }
}
