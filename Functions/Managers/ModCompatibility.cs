using System;
using System.Collections.Generic;

using UnityEngine;

namespace RTFunctions.Functions.Managers
{
    /// <summary>
    /// This class is used to share mod variables and functions, as well as check if a mod is installed.
    /// </summary>
    public class ModCompatibility : MonoBehaviour
    {
        public static ModCompatibility inst;

        public static GameObject bepinex;

        public static Dictionary<string, object> sharedFunctions = new Dictionary<string, object>();

        void Awake()
        {
            inst = this;

            bepinex = GameObject.Find("BepInEx_Manager");

            if (bepinex.GetComponentByName("ExplorerBepInPlugin"))
            {
                try
                {
                    var ue = bepinex.GetComponentByName("ExplorerBepInPlugin");

                    var mod = new Mod(ue, ue.GetType());

                    mods.Add("UnityExplorer", mod);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("{0}Error.\nMessage: {1}\nStackTrace: {2}", FunctionsPlugin.className, ex.Message, ex.StackTrace);
                }
            }
        }

        public static void Set(string id, object a)
        {
            if (!sharedFunctions.ContainsKey(id))
                sharedFunctions.Add(id, a);
            else
                sharedFunctions[id] = a;
        }

        public static bool EditorManagementInstalled => mods.ContainsKey("EditorManagement");

        public static bool CreativePlayersInstalled => mods.ContainsKey("CreativePlayers");

        public static bool ObjectModifiersInstalled => mods.ContainsKey("ObjectModifiers");

        public static bool ExampleCompanionInstalled => mods.ContainsKey("ExampleCompanion");

        public static bool EventsCoreInstalled => mods.ContainsKey("EventsCore");

        public static bool ArcadiaCustomsInstalled => mods.ContainsKey("ArcadiaCustoms");

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

            Dictionary<string, Delegate> methods = new Dictionary<string, Delegate>();
            public Dictionary<string, Delegate> Methods
            {
                get => methods;
                set => methods = value;
            }

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

            public void Invoke(string name, params object[] values)
            { if (Methods.ContainsKey(name)) Methods[name].DynamicInvoke(values); }
        }
    }
}
