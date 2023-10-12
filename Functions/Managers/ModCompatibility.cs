using System;
using System.Reflection;
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

        void Awake()
        {
            inst = this;

            bepinex = GameObject.Find("BepInEx_Manager");

            if (bepinex.GetComponentByName("ObjectModifiersPlugin"))
            {
                var om = bepinex.GetComponentByName("ObjectModifiersPlugin");
                objectModifiersPlugin = om.GetType();
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

                    var cat = bepinex.GetComponentByName("CatalystBase");

                    if (cat)
                    {
                        try
                        {
                            Destroy(bepinex.GetComponentByName("CatalystBase"));
                            Debug.LogWarning($"{FunctionsPlugin.className}Catalyst functionality has moved to this mod, so no need to have it installed anymore.");
                        }
                        catch
                        {

                        }
                    }

                    //catalyst = cat.GetType();
                    //catalystInstance = catalyst.GetField("Instance").GetValue(bepinex.GetComponentByName("CatalystBase"));

                    //if ((string)catalyst.GetField("Name").GetValue(catalyst) == "Editor Catalyst")
                    //{
                    //    catalystType = CatalystType.Editor;
                    //}
                }
            }

            if (bepinex.GetComponentByName("ArcadePlugin"))
            {
                var arc = bepinex.GetComponentByName("ArcadePlugin");
                arcadePlugin = arc.GetType();
            }

            if (bepinex.GetComponentByName("PlayerPlugin"))
            {
                player = AccessTools.TypeByName("RTPlayer");
                var p = bepinex.GetComponentByName("PlayerPlugin");
                playerPlugin = p.GetType();
                playerPluginInstance = p;
            }

            if (bepinex.GetComponentByName("EditorPlugin"))
            {
                rtEditor = AccessTools.TypeByName("RTEditor");

                var ed = bepinex.GetComponentByName("EditorPlugin");

                editorPlugin = ed.GetType();
            }

            if (bepinex.GetComponentByName("FontPlugin"))
            {
                var f = bepinex.GetComponentByName("FontPlugin");
                fontPlugin = f.GetType();
            }

            if (bepinex.GetComponentByName("EventsCorePlugin"))
            {
                var ec = bepinex.GetComponentByName("EventsCorePlugin");
                eventsCorePlugin = ec.GetType();
            }

            if (bepinex.GetComponentByName("ExplorerBepInPlugin"))
            {
                try
                {
                    var ue = bepinex.GetComponentByName("ExplorerBepInPlugin");

                    var mod = new Mod(ue, ue.GetType());

                    var consoleController = AccessTools.TypeByName("UnityExplorer.CSConsole.ConsoleController");

                    if (consoleController != null)
                    {
                        var csmod = new Mod(null, consoleController);
                        csmod.methods.Add("Evaluate", consoleController.GetMethod("Evaluate", new Type[] { typeof(string), typeof(bool) }));
                        mod.components.Add("ConsoleController", csmod);
                    }

                    mods.Add("UnityExplorer", mod);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("{0}Error.\nMessage: {1}\nStackTrace: {2}", FunctionsPlugin.className, ex.Message, ex.StackTrace);
                }
            }


            if (bepinex.GetComponentByName("ShapesPlugin"))
            {
                var p = bepinex.GetComponentByName("ShapesPlugin");
                shapesPlugin = p.GetType();
                shapesPluginInstance = p;
            }
        }

        #region CreativePlayers

        public static Type player;
        public static Type playerPlugin;
        public static object playerPluginInstance;

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

        #region CustomShapes

        public static Type shapesPlugin;
        public static object shapesPluginInstance;

        #endregion

        public static Dictionary<string, Mod> mods = new Dictionary<string, Mod>();

        public class Mod
        {
            public Mod(object inst, Type type)
            {
                this.inst = inst;
                this.type = type;
            }

            public object inst;
            public Type type;

            public Dictionary<string, Mod> components = new Dictionary<string, Mod>();

            public Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

            public string version = "1.0.0";
            string className;
            public string ClassName
            {
                get => className;
                private set
                {
                    className = $"[{value}] {version}\n";
                }
            }

            public void StaticInvoke(string name, params object[] values)
            { if (methods.ContainsKey(name)) AccessTools.Method(type, name, values.ToTypes()).Invoke(type, values); }

            public void Invoke(string name, params object[] values)
            { if (methods.ContainsKey(name) && inst != null) methods[name].Invoke(inst, values); }
            public void InvokeStatic(string name, params object[] values)
            { if (methods.ContainsKey(name)) methods[name].Invoke(type, values); }
        }
    }
}
