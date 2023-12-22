using System.IO;
using System.Linq;
using System.Collections.Generic;

using SimpleJSON;
using UnityEngine;

using RTFunctions.Functions;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.Data.Player
{
    public class PlayerData : MonoBehaviour
    {
        public static void SavePlayer(PlayerModel _model, string _name, string _path = "")
        {
            string path = _path;
            if (path == "")
            {
                if (!RTFile.DirectoryExists(RTFile.ApplicationDirectory + "beatmaps/players"))
                {
                    Directory.CreateDirectory(RTFile.ApplicationDirectory + "beatmaps/players");
                }
                path = RTFile.ApplicationDirectory + "beatmaps/players/" + _name.ToLower().Replace(" ", "_") + ".lspl";
                //int num = 0;
                
                //while (RTFile.FileExists(path))
                //{
                //    path = RTFile.ApplicationDirectory + "beatmaps/players/" + _name.ToLower().Replace(" ", "_") + "_" + num.ToString() + ".lspl";
                //    num++;
                //}
            }

            var jn = SavePlayer(_model, JSON.Parse(RTFile.FileExists(path) ? RTFile.ReadFromFile(path) : "{}"));

            RTFile.WriteToFile(path, jn.ToString(3));
        }

        public static JSONNode SavePlayer(PlayerModel _model, JSONNode _jn = null)
        {
            JSONNode jn = null;
            if (jn != null)
            {
                jn = _jn;
            }
            else
            {
                jn = JSON.Parse("{}");
            }

            #region Base

            jn["base"]["name"] = (string)_model.values["Base Name"];
            jn["base"]["id"] = (string)_model.values["Base ID"];
            jn["base"]["health"] = ((int)_model.values["Base Health"]).ToString();

            jn["base"]["move_speed"] = ((float)_model.values["Base Move Speed"]).ToString();
            jn["base"]["boost_speed"] = ((float)_model.values["Base Boost Speed"]).ToString();
            jn["base"]["boost_cooldown"] = ((float)_model.values["Base Boost Cooldown"]).ToString();
            jn["base"]["boost_min_time"] = ((float)_model.values["Base Min Boost Time"]).ToString();
            jn["base"]["boost_max_time"] = ((float)_model.values["Base Max Boost Time"]).ToString();

            jn["base"]["rotate_mode"] = ((int)_model.values["Base Rotate Mode"]).ToString();
            jn["base"]["collision_acc"] = ((bool)_model.values["Base Collision Accurate"]).ToString();
            jn["base"]["sprsneak"] = ((bool)_model.values["Base Sprint Sneak Active"]).ToString();

            #endregion

            #region Stretch

            jn["stretch"]["active"] = ((bool)_model.values["Stretch Active"]).ToString();
            jn["stretch"]["amount"] = ((float)_model.values["Stretch Amount"]).ToString();
            jn["stretch"]["easing"] = ((int)_model.values["Stretch Easing"]).ToString();

            #endregion

            #region Health

            jn["gui"]["health"]["active"] = ((bool)_model.values["GUI Health Active"]).ToString();
            jn["gui"]["health"]["mode"] = ((int)_model.values["GUI Health Mode"]).ToString();

            #endregion

            #region Head

            if (((Vector2Int)_model.values["Head Shape"]).x != 0)
                jn["head"]["s"] = ((Vector2Int)_model.values["Head Shape"]).x.ToString();
            if (((Vector2Int)_model.values["Head Shape"]).y != 0)
                jn["head"]["so"] = ((Vector2Int)_model.values["Head Shape"]).y.ToString();
            jn["head"]["pos"]["x"] = ((Vector2)_model.values["Head Position"]).x.ToString();
            jn["head"]["pos"]["y"] = ((Vector2)_model.values["Head Position"]).y.ToString();
            jn["head"]["sca"]["x"] = ((Vector2)_model.values["Head Scale"]).x.ToString();
            jn["head"]["sca"]["y"] = ((Vector2)_model.values["Head Scale"]).y.ToString();
            jn["head"]["rot"]["x"] = ((float)_model.values["Head Rotation"]).ToString();

            jn["head"]["col"]["x"] = ((int)_model.values["Head Color"]).ToString();
            jn["head"]["col"]["hex"] = (string)_model.values["Head Custom Color"];
            jn["head"]["opa"]["hex"] = ((float)_model.values["Head Opacity"]).ToString();

            #endregion

            #region Head Trail

            jn["head"]["trail"]["em"] = ((bool)_model.values["Head Trail Emitting"]).ToString();
            jn["head"]["trail"]["t"] = ((float)_model.values["Head Trail Time"]).ToString();
            jn["head"]["trail"]["w"]["start"] = ((float)_model.values["Head Trail Start Width"]).ToString();
            jn["head"]["trail"]["w"]["end"] = ((float)_model.values["Head Trail End Width"]).ToString();
            jn["head"]["trail"]["c"]["start"] = ((int)_model.values["Head Trail Start Color"]).ToString();
            jn["head"]["trail"]["c"]["end"] = ((int)_model.values["Head Trail End Color"]).ToString();
            jn["head"]["trail"]["o"]["start"] = ((float)_model.values["Head Trail Start Opacity"]).ToString();
            jn["head"]["trail"]["o"]["end"] = ((float)_model.values["Head Trail End Opacity"]).ToString();
            jn["head"]["trail"]["pos"]["x"] = ((Vector2)_model.values["Head Trail Position Offset"]).x;
            jn["head"]["trail"]["pos"]["y"] = ((Vector2)_model.values["Head Trail Position Offset"]).y;

            #endregion

            #region Head Particles
            jn["head"]["particles"]["em"] = ((bool)_model.values["Head Particles Emitting"]).ToString();
            if (((Vector2Int)_model.values["Head Particles Shape"]).x != 0)
                jn["head"]["particles"]["s"] = ((Vector2Int)_model.values["Head Particles Shape"]).x.ToString();
            if (((Vector2Int)_model.values["Head Particles Shape"]).y != 0)
                jn["head"]["particles"]["so"] = ((Vector2Int)_model.values["Head Particles Shape"]).y.ToString();
            jn["head"]["particles"]["col"] = ((int)_model.values["Head Particles Color"]).ToString();
            jn["head"]["particles"]["opa"]["start"] = ((float)_model.values["Head Particles Start Opacity"]).ToString();
            jn["head"]["particles"]["opa"]["end"] = ((float)_model.values["Head Particles End Opacity"]).ToString();
            jn["head"]["particles"]["sca"]["start"] = ((float)_model.values["Head Particles Start Scale"]).ToString();
            jn["head"]["particles"]["sca"]["end"] = ((float)_model.values["Head Particles End Scale"]).ToString();
            jn["head"]["particles"]["rot"] = ((float)_model.values["Head Particles Rotation"]).ToString();
            jn["head"]["particles"]["lt"] = ((float)_model.values["Head Particles Lifetime"]).ToString();
            jn["head"]["particles"]["sp"] = ((float)_model.values["Head Particles Speed"]).ToString();
            jn["head"]["particles"]["am"] = ((float)_model.values["Head Particles Amount"]).ToString();
            jn["head"]["particles"]["frc"]["x"] = ((Vector2)_model.values["Head Particles Force"]).x.ToString();
            jn["head"]["particles"]["frc"]["y"] = ((Vector2)_model.values["Head Particles Force"]).y.ToString();
            jn["head"]["particles"]["trem"] = ((bool)_model.values["Head Particles Trail Emitting"]).ToString();
            #endregion

            #region Face

            jn["face"]["position"]["x"] = ((Vector2)_model.values["Face Position"]).x.ToString();
            jn["face"]["position"]["y"] = ((Vector2)_model.values["Face Position"]).y.ToString();
            jn["face"]["con_active"] = ((bool)_model.values["Face Control Active"]).ToString();

            #endregion

            #region Boost

            jn["boost"]["active"] = ((bool)_model.values["Boost Active"]).ToString();
            if (((Vector2Int)_model.values["Boost Shape"]).x != 0)
                jn["boost"]["s"] = ((Vector2Int)_model.values["Boost Shape"]).x.ToString();
            if (((Vector2Int)_model.values["Boost Shape"]).y != 0)
                jn["boost"]["so"] = ((Vector2Int)_model.values["Boost Shape"]).y.ToString();
            jn["boost"]["pos"]["x"] = ((Vector2)_model.values["Boost Position"]).x.ToString();
            jn["boost"]["pos"]["y"] = ((Vector2)_model.values["Boost Position"]).y.ToString();
            jn["boost"]["sca"]["x"] = ((Vector2)_model.values["Boost Scale"]).x.ToString();
            jn["boost"]["sca"]["y"] = ((Vector2)_model.values["Boost Scale"]).y.ToString();
            jn["boost"]["rot"]["x"] = ((float)_model.values["Boost Rotation"]).ToString();

            jn["boost"]["col"]["x"] = ((int)_model.values["Boost Color"]).ToString();
            jn["boost"]["col"]["hex"] = (string)_model.values["Boost Custom Color"];
            jn["boost"]["opa"]["hex"] = ((float)_model.values["Boost Opacity"]).ToString();

            #endregion

            #region Boost Trail

            jn["boost"]["trail"]["em"] = ((bool)_model.values["Boost Trail Emitting"]).ToString();
            jn["boost"]["trail"]["t"] = ((float)_model.values["Boost Trail Time"]).ToString();
            jn["boost"]["trail"]["w"]["start"] = ((float)_model.values["Boost Trail Start Width"]).ToString();
            jn["boost"]["trail"]["w"]["end"] = ((float)_model.values["Boost Trail End Width"]).ToString();
            jn["boost"]["trail"]["c"]["start"] = ((int)_model.values["Boost Trail Start Color"]).ToString();
            jn["boost"]["trail"]["c"]["end"] = ((int)_model.values["Boost Trail End Color"]).ToString();
            jn["boost"]["trail"]["o"]["start"] = ((float)_model.values["Boost Trail Start Opacity"]).ToString();
            jn["boost"]["trail"]["o"]["end"] = ((float)_model.values["Boost Trail End Opacity"]).ToString();

            #endregion

            #region Boost Particles

            jn["boost"]["particles"]["em"] = ((bool)_model.values["Boost Particles Emitting"]).ToString();
            if (((Vector2Int)_model.values["Boost Particles Shape"]).x != 0)
                jn["boost"]["particles"]["s"] = ((Vector2Int)_model.values["Boost Particles Shape"]).x.ToString();
            if (((Vector2Int)_model.values["Boost Particles Shape"]).y != 0)
                jn["boost"]["particles"]["so"] = ((Vector2Int)_model.values["Boost Particles Shape"]).y.ToString();
            jn["boost"]["particles"]["col"] = ((int)_model.values["Boost Particles Color"]).ToString();
            jn["boost"]["particles"]["opa"]["start"] = ((float)_model.values["Boost Particles Start Opacity"]).ToString();
            jn["boost"]["particles"]["opa"]["end"] = ((float)_model.values["Boost Particles End Opacity"]).ToString();
            jn["boost"]["particles"]["sca"]["start"] = ((float)_model.values["Boost Particles Start Scale"]).ToString();
            jn["boost"]["particles"]["sca"]["end"] = ((float)_model.values["Boost Particles End Scale"]).ToString();
            jn["boost"]["particles"]["rot"] = ((float)_model.values["Boost Particles Rotation"]).ToString();
            jn["boost"]["particles"]["lt"] = ((float)_model.values["Boost Particles Lifetime"]).ToString();
            jn["boost"]["particles"]["sp"] = ((float)_model.values["Boost Particles Speed"]).ToString();
            jn["boost"]["particles"]["am"] = ((int)_model.values["Boost Particles Amount"]).ToString();
            jn["boost"]["particles"]["frc"]["x"] = ((Vector2)_model.values["Boost Particles Force"]).x.ToString();
            jn["boost"]["particles"]["frc"]["y"] = ((Vector2)_model.values["Boost Particles Force"]).y.ToString();
            jn["boost"]["particles"]["trem"] = ((bool)_model.values["Boost Particles Trail Emitting"]).ToString();

            #endregion

            #region Pulse

            jn["pulse"]["active"] = ((bool)_model.values["Pulse Active"]).ToString();

            if (((Vector2Int)_model.values["Pulse Shape"]).x != 0)
                jn["pulse"]["s"] = ((Vector2Int)_model.values["Pulse Shape"]).x.ToString();
            if (((Vector2Int)_model.values["Pulse Shape"]).y != 0)
                jn["pulse"]["so"] = ((Vector2Int)_model.values["Pulse Shape"]).y.ToString();

            jn["pulse"]["rothead"] = ((bool)_model.values["Pulse Rotate to Head"]).ToString();

            jn["pulse"]["col"]["start"] = ((int)_model.values["Pulse Start Color"]).ToString();
            jn["pulse"]["col"]["starthex"] = (string)_model.values["Pulse Start Custom Color"];
            jn["pulse"]["col"]["end"] = ((int)_model.values["Pulse End Color"]).ToString();
            jn["pulse"]["col"]["endhex"] = (string)_model.values["Pulse End Custom Color"];
            jn["pulse"]["col"]["easing"] = ((int)_model.values["Pulse Easing Color"]).ToString();

            jn["pulse"]["opa"]["start"] = ((float)_model.values["Pulse Start Opacity"]).ToString();
            jn["pulse"]["opa"]["end"] = ((float)_model.values["Pulse End Opacity"]).ToString();
            jn["pulse"]["opa"]["easing"] = ((int)_model.values["Pulse Easing Opacity"]).ToString();

            jn["pulse"]["d"] = ((float)_model.values["Pulse Depth"]).ToString();

            jn["pulse"]["pos"]["start"]["x"] = ((Vector2)_model.values["Pulse Start Position"]).x.ToString();
            jn["pulse"]["pos"]["start"]["y"] = ((Vector2)_model.values["Pulse Start Position"]).y.ToString();
            jn["pulse"]["pos"]["end"]["x"] = ((Vector2)_model.values["Pulse End Position"]).x.ToString();
            jn["pulse"]["pos"]["end"]["y"] = ((Vector2)_model.values["Pulse End Position"]).y.ToString();
            jn["pulse"]["pos"]["easing"] = ((int)_model.values["Pulse Easing Position"]).ToString();

            jn["pulse"]["sca"]["start"]["x"] = ((Vector2)_model.values["Pulse Start Scale"]).x.ToString();
            jn["pulse"]["sca"]["start"]["y"] = ((Vector2)_model.values["Pulse Start Scale"]).y.ToString();
            jn["pulse"]["sca"]["end"]["x"] = ((Vector2)_model.values["Pulse End Scale"]).x.ToString();
            jn["pulse"]["sca"]["end"]["y"] = ((Vector2)_model.values["Pulse End Scale"]).y.ToString();
            jn["pulse"]["sca"]["easing"] = ((int)_model.values["Pulse Easing Scale"]).ToString();

            jn["pulse"]["rot"]["start"] = ((float)_model.values["Pulse Start Rotation"]).ToString();
            jn["pulse"]["rot"]["end"] = ((float)_model.values["Pulse End Rotation"]).ToString();
            jn["pulse"]["rot"]["easing"] = ((int)_model.values["Pulse Easing Rotation"]).ToString();

            jn["pulse"]["lt"] = ((float)_model.values["Pulse Duration"]).ToString();

            #endregion

            #region Bullet

            jn["bullet"]["active"] = ((bool)_model.values["Bullet Active"]).ToString();

            jn["bullet"]["ak"] = ((bool)_model.values["Bullet AutoKill"]).ToString();

            jn["bullet"]["speed"] = ((float)_model.values["Bullet Speed Amount"]).ToString();

            jn["bullet"]["lt"] = ((float)_model.values["Bullet Lifetime"]).ToString();

            jn["bullet"]["delay"] = ((float)_model.values["Bullet Delay Amount"]).ToString();

            jn["bullet"]["constant"] = ((bool)_model.values["Bullet Constant"]).ToString();

            jn["bullet"]["hit"] = ((bool)_model.values["Bullet Hurt Players"]).ToString();

            jn["bullet"]["o"]["x"] = ((Vector2)_model.values["Bullet Origin"]).x.ToString();
            jn["bullet"]["o"]["y"] = ((Vector2)_model.values["Bullet Origin"]).y.ToString();

            if (((Vector2Int)_model.values["Bullet Shape"]).x != 0)
                jn["bullet"]["s"] = ((Vector2Int)_model.values["Bullet Shape"]).x.ToString();
            if (((Vector2Int)_model.values["Bullet Shape"]).y != 0)
                jn["bullet"]["so"] = ((Vector2Int)_model.values["Bullet Shape"]).y.ToString();

            jn["bullet"]["col"]["start"] = ((int)_model.values["Bullet Start Color"]).ToString();
            jn["bullet"]["col"]["starthex"] = (string)_model.values["Bullet Start Custom Color"];
            jn["bullet"]["col"]["end"] = ((int)_model.values["Bullet End Color"]).ToString();
            jn["bullet"]["col"]["endhex"] = (string)_model.values["Bullet End Custom Color"];
            jn["bullet"]["col"]["easing"] = ((int)_model.values["Bullet Easing Color"]).ToString();
            jn["bullet"]["col"]["dur"] = ((float)_model.values["Bullet Duration Color"]).ToString();

            jn["bullet"]["opa"]["start"] = ((float)_model.values["Bullet Start Opacity"]).ToString();
            jn["bullet"]["opa"]["end"] = ((float)_model.values["Bullet End Opacity"]).ToString();
            jn["bullet"]["opa"]["easing"] = ((int)_model.values["Bullet Easing Opacity"]).ToString();
            jn["bullet"]["opa"]["dur"] = ((float)_model.values["Bullet Duration Opacity"]).ToString();

            jn["bullet"]["d"] = ((float)_model.values["Bullet Depth"]).ToString();

            jn["bullet"]["pos"]["start"]["x"] = ((Vector2)_model.values["Bullet Start Position"]).x.ToString();
            jn["bullet"]["pos"]["start"]["y"] = ((Vector2)_model.values["Bullet Start Position"]).y.ToString();
            jn["bullet"]["pos"]["end"]["x"] = ((Vector2)_model.values["Bullet End Position"]).x.ToString();
            jn["bullet"]["pos"]["end"]["y"] = ((Vector2)_model.values["Bullet End Position"]).y.ToString();
            jn["bullet"]["pos"]["easing"] = ((int)_model.values["Bullet Easing Position"]).ToString();
            jn["bullet"]["pos"]["dur"] = ((float)_model.values["Bullet Duration Position"]).ToString();

            jn["bullet"]["sca"]["start"]["x"] = ((Vector2)_model.values["Bullet Start Scale"]).x.ToString();
            jn["bullet"]["sca"]["start"]["y"] = ((Vector2)_model.values["Bullet Start Scale"]).y.ToString();
            jn["bullet"]["sca"]["end"]["x"] = ((Vector2)_model.values["Bullet End Scale"]).x.ToString();
            jn["bullet"]["sca"]["end"]["y"] = ((Vector2)_model.values["Bullet End Scale"]).y.ToString();
            jn["bullet"]["sca"]["easing"] = ((int)_model.values["Bullet Easing Scale"]).ToString();
            jn["bullet"]["sca"]["dur"] = ((float)_model.values["Bullet Duration Scale"]).ToString();

            jn["bullet"]["rot"]["start"] = ((float)_model.values["Bullet Start Rotation"]).ToString();
            jn["bullet"]["rot"]["end"] = ((float)_model.values["Bullet End Rotation"]).ToString();
            jn["bullet"]["rot"]["easing"] = ((int)_model.values["Bullet Easing Rotation"]).ToString();
            jn["bullet"]["rot"]["dur"] = ((float)_model.values["Bullet Duration Rotation"]).ToString();

            #endregion

            #region Tail

            jn["tail_base"]["distance"] = ((float)_model.values["Tail Base Distance"]).ToString();
            jn["tail_base"]["mode"] = ((int)_model.values["Tail Base Mode"]).ToString();
            jn["tail_base"]["grows"] = ((bool)_model.values["Tail Base Grows"]).ToString();

            jn["tail_boost"]["active"] = ((bool)_model.values["Tail Boost Active"]).ToString();

            if (((Vector2Int)_model.values["Tail Boost Shape"]).x != 0)
                jn["tail_boost"]["s"] = ((Vector2Int)_model.values["Tail Boost Shape"]).x.ToString();
            if (((Vector2Int)_model.values["Tail Boost Shape"]).y != 0)
                jn["tail_boost"]["so"] = ((Vector2Int)_model.values["Tail Boost Shape"]).y.ToString();
            jn["tail_boost"]["pos"]["x"] = ((Vector2)_model.values["Tail Boost Position"]).x.ToString();
            jn["tail_boost"]["pos"]["y"] = ((Vector2)_model.values["Tail Boost Position"]).y.ToString();
            jn["tail_boost"]["sca"]["x"] = ((Vector2)_model.values["Tail Boost Scale"]).x.ToString();
            jn["tail_boost"]["sca"]["y"] = ((Vector2)_model.values["Tail Boost Scale"]).y.ToString();
            jn["tail_boost"]["rot"]["x"] = ((float)_model.values["Tail Boost Rotation"]).ToString();

            jn["tail_boost"]["col"]["x"] = ((int)_model.values["Tail Boost Color"]).ToString();
            jn["tail_boost"]["col"]["hex"] = (string)_model.values["Tail Boost Custom Color"];
            jn["tail_boost"]["opa"]["hex"] = ((float)_model.values["Tail Boost Opacity"]).ToString();

            for (int i = 1; i < 4; i++)
            {
                jn["tail"][i - 1]["active"] = ((bool)_model.values[string.Format("Tail {0} Active", i)]).ToString();
                if (((Vector2Int)_model.values[string.Format("Tail {0} Shape", i)]).x != 0)
                    jn["tail"][i - 1]["s"] = ((Vector2Int)_model.values[string.Format("Tail {0} Shape", i)]).x.ToString();
                if (((Vector2Int)_model.values[string.Format("Tail {0} Shape", i)]).y != 0)
                    jn["tail"][i - 1]["so"] = ((Vector2Int)_model.values[string.Format("Tail {0} Shape", i)]).y.ToString();
                jn["tail"][i - 1]["pos"]["x"] = ((Vector2)_model.values[string.Format("Tail {0} Position", i)]).x.ToString();
                jn["tail"][i - 1]["pos"]["y"] = ((Vector2)_model.values[string.Format("Tail {0} Position", i)]).y.ToString();
                jn["tail"][i - 1]["sca"]["x"] = ((Vector2)_model.values[string.Format("Tail {0} Scale", i)]).x.ToString();
                jn["tail"][i - 1]["sca"]["y"] = ((Vector2)_model.values[string.Format("Tail {0} Scale", i)]).y.ToString();
                jn["tail"][i - 1]["rot"]["x"] = ((float)_model.values[string.Format("Tail {0} Rotation", i)]).ToString();
                jn["tail"][i - 1]["col"]["x"] = ((int)_model.values[string.Format("Tail {0} Color", i)]).ToString();
                jn["tail"][i - 1]["col"]["hex"] = (string)_model.values[string.Format("Tail {0} Custom Color", i)];
                jn["tail"][i - 1]["opa"]["x"] = ((float)_model.values[string.Format("Tail {0} Opacity", i)]).ToString();

                jn["tail"][i - 1]["trail"]["em"] = ((bool)_model.values[string.Format("Tail {0} Trail Emitting", i)]).ToString();
                jn["tail"][i - 1]["trail"]["t"] = ((float)_model.values[string.Format("Tail {0} Trail Time", i)]).ToString();
                jn["tail"][i - 1]["trail"]["w"]["start"] = ((float)_model.values[string.Format("Tail {0} Trail Start Width", i)]).ToString();
                jn["tail"][i - 1]["trail"]["w"]["end"] = ((float)_model.values[string.Format("Tail {0} Trail End Width", i)]).ToString();
                jn["tail"][i - 1]["trail"]["c"]["start"] = ((int)_model.values[string.Format("Tail {0} Trail Start Color", i)]).ToString();
                jn["tail"][i - 1]["trail"]["c"]["start_hex"] = (string)_model.values[string.Format("Tail {0} Trail Start Custom Color", i)];
                jn["tail"][i - 1]["trail"]["c"]["end"] = ((int)_model.values[string.Format("Tail {0} Trail End Color", i)]).ToString();
                jn["tail"][i - 1]["trail"]["c"]["end_hex"] = (string)_model.values[string.Format("Tail {0} Trail End Custom Color", i)];
                jn["tail"][i - 1]["trail"]["o"]["start"] = ((float)_model.values[string.Format("Tail {0} Trail Start Opacity", i)]).ToString();
                jn["tail"][i - 1]["trail"]["o"]["end"] = ((float)_model.values[string.Format("Tail {0} Trail End Opacity", i)]).ToString();

                jn["tail"][i - 1]["particles"]["em"] = ((bool)_model.values[string.Format("Tail {0} Particles Emitting", i)]).ToString();
                if (((Vector2Int)_model.values[string.Format("Tail {0} Particles Shape", i)]).x != 0)
                    jn["tail"][i - 1]["particles"]["s"] = ((Vector2Int)_model.values[string.Format("Tail {0} Particles Shape", i)]).x.ToString();
                if (((Vector2Int)_model.values[string.Format("Tail {0} Particles Shape", i)]).y != 0)
                    jn["tail"][i - 1]["particles"]["so"] = ((Vector2Int)_model.values[string.Format("Tail {0} Particles Shape", i)]).y.ToString();
                jn["tail"][i - 1]["particles"]["col"] = ((int)_model.values[string.Format("Tail {0} Particles Color", i)]).ToString();
                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["particles"]["col_hex"]))
                    jn["tail"][i - 1]["particles"]["col_hex"] = (string)_model.values[string.Format("Tail {0} Particles Custom Color", i)];

                jn["tail"][i - 1]["particles"]["opa"]["start"] = ((float)_model.values[string.Format("Tail {0} Particles Start Opacity", i)]).ToString();
                jn["tail"][i - 1]["particles"]["opa"]["end"] = ((float)_model.values[string.Format("Tail {0} Particles End Opacity", i)]).ToString();
                jn["tail"][i - 1]["particles"]["sca"]["start"] = ((float)_model.values[string.Format("Tail {0} Particles Start Scale", i)]).ToString();
                jn["tail"][i - 1]["particles"]["sca"]["end"] = ((float)_model.values[string.Format("Tail {0} Particles End Scale", i)]).ToString();
                jn["tail"][i - 1]["particles"]["rot"] = ((float)_model.values[string.Format("Tail {0} Particles Rotation", i)]).ToString();
                jn["tail"][i - 1]["particles"]["lt"] = ((float)_model.values[string.Format("Tail {0} Particles Lifetime", i)]).ToString();
                jn["tail"][i - 1]["particles"]["sp"] = ((float)_model.values[string.Format("Tail {0} Particles Speed", i)]).ToString();
                jn["tail"][i - 1]["particles"]["am"] = ((float)_model.values[string.Format("Tail {0} Particles Amount", i)]).ToString();
                jn["tail"][i - 1]["particles"]["frc"]["x"] = ((Vector2)_model.values[string.Format("Tail {0} Particles Force", i)]).x.ToString();
                jn["tail"][i - 1]["particles"]["frc"]["y"] = ((Vector2)_model.values[string.Format("Tail {0} Particles Force", i)]).y.ToString();
                jn["tail"][i - 1]["particles"]["trem"] = ((bool)_model.values[string.Format("Tail {0} Particles Trail Emitting", i)]).ToString();
            }
            #endregion

            #region Custom Objects

            Dictionary<string, object> dictionary = (Dictionary<string, object>)_model.values["Custom Objects"];
            if (dictionary != null && dictionary.Count > 0)
                for (int i = 0; i < dictionary.Count; i++)
                {
                    var customObj = (Dictionary<string, object>)dictionary.ElementAt(i).Value;

                    jn["custom_objects"][i]["id"] = (string)customObj["ID"];
                    jn["custom_objects"][i]["n"] = (string)customObj["Name"];

                    if (((Vector2Int)customObj["Shape"]).x != 0)
                        jn["custom_objects"][i]["s"] = ((Vector2Int)customObj["Shape"]).x.ToString();
                    if (((Vector2Int)customObj["Shape"]).y != 0)
                        jn["custom_objects"][i]["so"] = ((Vector2Int)customObj["Shape"]).y.ToString();
                    jn["custom_objects"][i]["p"] = ((int)customObj["Parent"]).ToString();
                    jn["custom_objects"][i]["ppo"] = ((float)customObj["Parent Position Offset"]).ToString();
                    jn["custom_objects"][i]["pso"] = ((float)customObj["Parent Scale Offset"]).ToString();
                    jn["custom_objects"][i]["pro"] = ((float)customObj["Parent Rotation Offset"]).ToString();
                    jn["custom_objects"][i]["psa"] = ((bool)customObj["Parent Scale Active"]).ToString();
                    jn["custom_objects"][i]["pra"] = ((bool)customObj["Parent Rotation Active"]).ToString();
                    jn["custom_objects"][i]["d"] = ((float)customObj["Depth"]).ToString();
                    jn["custom_objects"][i]["pos"]["x"] = ((Vector2)customObj["Position"]).x.ToString();
                    jn["custom_objects"][i]["pos"]["y"] = ((Vector2)customObj["Position"]).y.ToString();
                    jn["custom_objects"][i]["sca"]["x"] = ((Vector2)customObj["Scale"]).x.ToString();
                    jn["custom_objects"][i]["sca"]["y"] = ((Vector2)customObj["Scale"]).y.ToString();
                    jn["custom_objects"][i]["rot"]["x"] = ((float)customObj["Rotation"]).ToString();
                    jn["custom_objects"][i]["col"]["x"] = ((int)customObj["Color"]).ToString();
                    if (((int)customObj["Color"]) == 24)
                    {
                        jn["custom_objects"][i]["col"]["hex"] = (string)customObj["Custom Color"];
                    }

                    if (((float)customObj["Opacity"]) != 1f)
                        jn["custom_objects"][i]["opa"]["x"] = ((float)customObj["Opacity"]).ToString();

                    if ((int)customObj["Visibility"] != 0)
                        jn["custom_objects"][i]["v"] = ((int)customObj["Visibility"]).ToString();

                    if ((int)customObj["Visibility"] > 3)
                        jn["custom_objects"][i]["vhp"] = ((float)customObj["Visibility Value"]).ToString();

                    if ((bool)customObj["Visibility Not"] != false)
                        jn["custom_objects"][i]["vn"] = ((bool)customObj["Visibility Not"]).ToString();
                }
            #endregion
            return jn;
        }

        public static PlayerModel LoadPlayer(string _name, string _path = "")
        {
            string path = _path;
            if (path == "")
            {
                path = RTFile.ApplicationDirectory + "beatmaps/players/" + _name.ToLower().Replace(" ", "") + ".lspl";
            }

            if (RTFile.FileExists(path))
            {
                string json = RTFile.ReadFromFile(path);
                var jn = JSON.Parse(json);
                return LoadPlayer(jn);
            }

            return null;
        }

        public static PlayerModel LoadPlayer(JSONNode jn)
        {
            var model = new PlayerModel();

            #region Base

            model.values["Base Name"] = (string)jn["base"]["name"];
            if (!string.IsNullOrEmpty(jn["base"]["id"]))
            {
                model.values["Base ID"] = (string)jn["base"]["id"];
            }
            else
            {
                model.values["Base ID"] = LSFunctions.LSText.randomNumString(16);
            }
            if (!string.IsNullOrEmpty(jn["base"]["health"]))
            {
                model.values["Base Health"] = int.Parse(jn["base"]["health"]);
            }
            else
            {
                model.values["Base Health"] = 3;
            }

            if (!string.IsNullOrEmpty(jn["base"]["move_speed"]))
            {
                model.values["Base Move Speed"] = float.Parse(jn["base"]["move_speed"]);
            }
            if (!string.IsNullOrEmpty(jn["base"]["boost_speed"]))
            {
                model.values["Base Boost Speed"] = float.Parse(jn["base"]["boost_speed"]);
            }
            if (!string.IsNullOrEmpty(jn["base"]["boost_cooldown"]))
            {
                model.values["Base Boost Cooldown"] = float.Parse(jn["base"]["boost_cooldown"]);
            }
            if (!string.IsNullOrEmpty(jn["base"]["boost_min_time"]))
            {
                model.values["Base Min Boost Time"] = float.Parse(jn["base"]["boost_min_time"]);
            }
            if (!string.IsNullOrEmpty(jn["base"]["boost_max_time"]))
            {
                model.values["Base Max Boost Time"] = float.Parse(jn["base"]["boost_max_time"]);
            }

            if (!string.IsNullOrEmpty(jn["base"]["rotate_mode"]))
            {
                model.values["Base Rotate Mode"] = int.Parse(jn["base"]["rotate_mode"]);
            }

            if (!string.IsNullOrEmpty(jn["base"]["collision_acc"]))
            {
                model.values["Base Collision Accurate"] = bool.Parse(jn["base"]["collision_acc"]);
            }

            if (!string.IsNullOrEmpty(jn["base"]["sprsneak"]))
            {
                model.values["Base Sprint Sneak Active"] = bool.Parse(jn["base"]["sprsneak"]);
            }

            #endregion

            #region Stretch

            if (!string.IsNullOrEmpty(jn["stretch"]["active"]))
            {
                model.values["Stretch Active"] = bool.Parse(jn["stretch"]["active"]);
            }

            if (!string.IsNullOrEmpty(jn["stretch"]["amount"]))
            {
                model.values["Stretch Amount"] = float.Parse(jn["stretch"]["amount"]);
            }

            if (!string.IsNullOrEmpty(jn["stretch"]["easing"]))
            {
                model.values["Stretch Easing"] = int.Parse(jn["stretch"]["easing"]);
            }

            #endregion

            #region GUI

            if (!string.IsNullOrEmpty(jn["gui"]["health"]["active"]))
            {
                model.values["GUI Health Active"] = bool.Parse(jn["gui"]["health"]["active"]);
            }

            if (!string.IsNullOrEmpty(jn["gui"]["health"]["mode"]))
            {
                model.values["GUI Health Mode"] = int.Parse(jn["gui"]["health"]["mode"]);
            }

            #endregion

            #region Head

            int headS = 0;
            int headSO = 0;
            if (!string.IsNullOrEmpty(jn["head"]["s"]))
            {
                headS = int.Parse(jn["head"]["s"]);
            }
            if (!string.IsNullOrEmpty(jn["head"]["so"]))
            {
                headSO = int.Parse(jn["head"]["so"]);
            }

            model.values["Head Shape"] = new Vector2Int(headS, headSO);
            model.values["Head Position"] = new Vector2(float.Parse(jn["head"]["pos"]["x"]), float.Parse(jn["head"]["pos"]["y"]));
            model.values["Head Scale"] = new Vector2(float.Parse(jn["head"]["sca"]["x"]), float.Parse(jn["head"]["sca"]["y"]));
            model.values["Head Rotation"] = float.Parse(jn["head"]["rot"]["x"]);

            if (jn["head"]["col"] != null && !string.IsNullOrEmpty(jn["head"]["col"]["x"]))
                model.values["Head Color"] = int.Parse(jn["head"]["col"]["x"]);
            if (jn["head"]["col"] != null && !string.IsNullOrEmpty(jn["head"]["col"]["hex"]))
                model.values["Head Custom Color"] = (string)jn["head"]["col"]["hex"];
            if (jn["head"]["opa"] != null && !string.IsNullOrEmpty(jn["head"]["opa"]["x"]))
                model.values["Head Opacity"] = float.Parse(jn["head"]["opa"]["x"]);

            #endregion

            #region Head Trail

            model.values["Head Trail Emitting"] = bool.Parse(jn["head"]["trail"]["em"]);
            model.values["Head Trail Time"] = float.Parse(jn["head"]["trail"]["t"]);
            model.values["Head Trail Start Width"] = float.Parse(jn["head"]["trail"]["w"]["start"]);
            model.values["Head Trail End Width"] = float.Parse(jn["head"]["trail"]["w"]["end"]);
            model.values["Head Trail Start Color"] = int.Parse(jn["head"]["trail"]["c"]["start"]);
            model.values["Head Trail End Color"] = int.Parse(jn["head"]["trail"]["c"]["end"]);
            model.values["Head Trail Start Opacity"] = float.Parse(jn["head"]["trail"]["o"]["start"]);
            model.values["Head Trail End Opacity"] = float.Parse(jn["head"]["trail"]["o"]["end"]);

            float x = 0f;
            float y = 0f;
            if (!string.IsNullOrEmpty(jn["head"]["trail"]["pos"]["x"]))
                x = float.Parse(jn["head"]["trail"]["pos"]["x"]);
            if (!string.IsNullOrEmpty(jn["head"]["trail"]["pos"]["y"]))
                y = float.Parse(jn["head"]["trail"]["pos"]["y"]);

            model.values["Head Trail Position Offset"] = new Vector2(x, y);

            #endregion

            #region Head Particles

            model.values["Head Particles Emitting"] = bool.Parse(jn["head"]["particles"]["em"]);

            int headPS = 0;
            int headPSO = 0;
            if (!string.IsNullOrEmpty(jn["head"]["particles"]["s"]))
            {
                headPS = int.Parse(jn["head"]["particles"]["s"]);
            }
            if (!string.IsNullOrEmpty(jn["head"]["particles"]["so"]))
            {
                headPSO = int.Parse(jn["head"]["particles"]["so"]);
            }

            model.values["Head Particles Shape"] = new Vector2Int(headPS, headPSO);
            model.values["Head Particles Color"] = int.Parse(jn["head"]["particles"]["col"]);
            model.values["Head Particles Start Opacity"] = float.Parse(jn["head"]["particles"]["opa"]["start"]);
            model.values["Head Particles End Opacity"] = float.Parse(jn["head"]["particles"]["opa"]["end"]);
            model.values["Head Particles Start Scale"] = float.Parse(jn["head"]["particles"]["sca"]["start"]);
            model.values["Head Particles End Scale"] = float.Parse(jn["head"]["particles"]["sca"]["end"]);
            model.values["Head Particles Rotation"] = float.Parse(jn["head"]["particles"]["rot"]);
            model.values["Head Particles Lifetime"] = float.Parse(jn["head"]["particles"]["lt"]);
            model.values["Head Particles Speed"] = float.Parse(jn["head"]["particles"]["sp"]);
            model.values["Head Particles Amount"] = float.Parse(jn["head"]["particles"]["am"]);
            model.values["Head Particles Force"] = new Vector2(float.Parse(jn["head"]["particles"]["frc"]["x"]), float.Parse(jn["head"]["particles"]["frc"]["y"]));
            model.values["Head Particles Trail Emitting"] = bool.Parse(jn["head"]["particles"]["trem"]);

            #endregion

            #region Face

            if (jn["face"] != null)
            {
                if (!string.IsNullOrEmpty(jn["face"]["position"]["x"]) && !string.IsNullOrEmpty(jn["face"]["position"]["y"]))
                {
                    model.values["Face Position"] = new Vector2(float.Parse(jn["face"]["position"]["x"]), float.Parse(jn["face"]["position"]["y"]));
                }

                if (!string.IsNullOrEmpty(jn["face"]["con_active"]))
                {
                    model.values["Face Control Active"] = bool.Parse(jn["face"]["con_active"]);
                }
            }

            #endregion

            #region Boost

            if (!string.IsNullOrEmpty(jn["boost"]["active"]))
                model.values["Boost Active"] = bool.Parse(jn["boost"]["active"]);

            int boostS = 0;
            int boostSO = 0;
            if (!string.IsNullOrEmpty(jn["boost"]["s"]))
                boostS = int.Parse(jn["boost"]["s"]);
            if (!string.IsNullOrEmpty(jn["boost"]["so"]))
                boostSO = int.Parse(jn["boost"]["so"]);

            model.values["Boost Shape"] = new Vector2Int(boostS, boostSO);
            model.values["Boost Position"] = new Vector2(float.Parse(jn["boost"]["pos"]["x"]), float.Parse(jn["boost"]["pos"]["y"]));
            model.values["Boost Scale"] = new Vector2(float.Parse(jn["boost"]["sca"]["x"]), float.Parse(jn["boost"]["sca"]["y"]));
            model.values["Boost Rotation"] = float.Parse(jn["boost"]["rot"]["x"]);

            if (jn["boost"]["col"] != null && !string.IsNullOrEmpty(jn["boost"]["col"]["x"]))
                model.values["Boost Color"] = int.Parse(jn["boost"]["col"]["x"]);
            if (jn["boost"]["col"] != null && !string.IsNullOrEmpty(jn["boost"]["col"]["hex"]))
                model.values["Boost Custom Color"] = (string)jn["boost"]["col"]["hex"];
            if (jn["boost"]["opa"] != null && !string.IsNullOrEmpty(jn["boost"]["opa"]["x"]))
                model.values["Boost Opacity"] = float.Parse(jn["boost"]["opa"]["x"]);

            #endregion

            #region Boost Trail

            model.values["Boost Trail Emitting"] = bool.Parse(jn["boost"]["trail"]["em"]);
            model.values["Boost Trail Time"] = float.Parse(jn["boost"]["trail"]["t"]);
            model.values["Boost Trail Start Width"] = float.Parse(jn["boost"]["trail"]["w"]["start"]);
            model.values["Boost Trail End Width"] = float.Parse(jn["boost"]["trail"]["w"]["end"]);
            model.values["Boost Trail Start Color"] = int.Parse(jn["boost"]["trail"]["c"]["start"]);
            model.values["Boost Trail End Color"] = int.Parse(jn["boost"]["trail"]["c"]["end"]);
            model.values["Boost Trail Start Opacity"] = float.Parse(jn["boost"]["trail"]["o"]["start"]);
            model.values["Boost Trail End Opacity"] = float.Parse(jn["boost"]["trail"]["o"]["end"]);

            #endregion

            #region Boost particles

            model.values["Boost Particles Emitting"] = bool.Parse(jn["boost"]["particles"]["em"]);

            int boostPS = 0;
            int boostPSO = 0;
            if (!string.IsNullOrEmpty(jn["boost"]["particles"]["s"]))
                boostPS = int.Parse(jn["boost"]["particles"]["s"]);
            if (!string.IsNullOrEmpty(jn["boost"]["particles"]["so"]))
                boostPSO = int.Parse(jn["boost"]["particles"]["so"]);

            model.values["Boost Particles Shape"] = new Vector2Int(boostPS, boostPSO);
            model.values["Boost Particles Color"] = int.Parse(jn["boost"]["particles"]["col"]);
            model.values["Boost Particles Start Opacity"] = float.Parse(jn["boost"]["particles"]["opa"]["start"]);
            model.values["Boost Particles End Opacity"] = float.Parse(jn["boost"]["particles"]["opa"]["end"]);
            model.values["Boost Particles Start Scale"] = float.Parse(jn["boost"]["particles"]["sca"]["start"]);
            model.values["Boost Particles End Scale"] = float.Parse(jn["boost"]["particles"]["sca"]["end"]);
            model.values["Boost Particles Rotation"] = float.Parse(jn["boost"]["particles"]["rot"]);
            model.values["Boost Particles Lifetime"] = float.Parse(jn["boost"]["particles"]["lt"]);
            model.values["Boost Particles Speed"] = float.Parse(jn["boost"]["particles"]["sp"]);
            model.values["Boost Particles Amount"] = int.Parse(jn["boost"]["particles"]["am"]);
            model.values["Boost Particles Force"] = new Vector2(float.Parse(jn["boost"]["particles"]["frc"]["x"]), float.Parse(jn["boost"]["particles"]["frc"]["y"]));
            model.values["Boost Particles Trail Emitting"] = bool.Parse(jn["boost"]["particles"]["trem"]);

            #endregion

            #region Pulse

            if (!string.IsNullOrEmpty(jn["pulse"]["active"]))
                model.values["Pulse Active"] = bool.Parse(jn["pulse"]["active"]);

            int pulseS = 0;
            int pulseSO = 0;
            if (!string.IsNullOrEmpty(jn["pulse"]["s"]))
                pulseS = int.Parse(jn["pulse"]["s"]);
            if (!string.IsNullOrEmpty(jn["pulse"]["so"]))
                pulseSO = int.Parse(jn["pulse"]["so"]);

            model.values["Pulse Shape"] = new Vector2Int(pulseS, pulseSO);

            if (!string.IsNullOrEmpty(jn["pulse"]["rothead"]))
                model.values["Pulse Rotate to Head"] = bool.Parse(jn["pulse"]["rothead"]);

            if (!string.IsNullOrEmpty(jn["pulse"]["col"]["start"]))
                model.values["Pulse Start Color"] = int.Parse(jn["pulse"]["col"]["start"]);
            if (!string.IsNullOrEmpty(jn["pulse"]["col"]["end"]))
                model.values["Pulse End Color"] = int.Parse(jn["pulse"]["col"]["end"]);
            if (!string.IsNullOrEmpty(jn["pulse"]["col"]["easing"]))
                model.values["Pulse Easing Color"] = int.Parse(jn["pulse"]["col"]["easing"]);

            if (!string.IsNullOrEmpty(jn["pulse"]["opa"]["start"]))
                model.values["Pulse Start Opacity"] = float.Parse(jn["pulse"]["opa"]["start"]);
            if (!string.IsNullOrEmpty(jn["pulse"]["opa"]["end"]))
                model.values["Pulse End Opacity"] = float.Parse(jn["pulse"]["opa"]["end"]);
            if (!string.IsNullOrEmpty(jn["pulse"]["opa"]["easing"]))
                model.values["Pulse Easing Opacity"] = int.Parse(jn["pulse"]["opa"]["easing"]);

            if (!string.IsNullOrEmpty(jn["pulse"]["d"]))
                model.values["Pulse Depth"] = float.Parse(jn["pulse"]["d"]);

            if (!string.IsNullOrEmpty(jn["pulse"]["pos"]["start"]["x"]) && !string.IsNullOrEmpty(jn["pulse"]["pos"]["start"]["y"]))
                model.values["Pulse Start Position"] = new Vector2(float.Parse(jn["pulse"]["pos"]["start"]["x"]), float.Parse(jn["pulse"]["pos"]["start"]["y"]));
            if (!string.IsNullOrEmpty(jn["pulse"]["pos"]["end"]["x"]) && !string.IsNullOrEmpty(jn["pulse"]["pos"]["end"]["y"]))
                model.values["Pulse End Position"] = new Vector2(float.Parse(jn["pulse"]["pos"]["end"]["x"]), float.Parse(jn["pulse"]["pos"]["end"]["y"]));
            if (!string.IsNullOrEmpty(jn["pulse"]["pos"]["easing"]))
                model.values["Pulse Easing Position"] = int.Parse(jn["pulse"]["pos"]["easing"]);

            if (!string.IsNullOrEmpty(jn["pulse"]["sca"]["start"]["x"]) && !string.IsNullOrEmpty(jn["pulse"]["sca"]["start"]["y"]))
                model.values["Pulse Start Scale"] = new Vector2(float.Parse(jn["pulse"]["sca"]["start"]["x"]), float.Parse(jn["pulse"]["sca"]["start"]["y"]));
            if (!string.IsNullOrEmpty(jn["pulse"]["sca"]["end"]["x"]) && !string.IsNullOrEmpty(jn["pulse"]["sca"]["end"]["y"]))
                model.values["Pulse End Scale"] = new Vector2(float.Parse(jn["pulse"]["sca"]["end"]["x"]), float.Parse(jn["pulse"]["sca"]["end"]["y"]));
            if (!string.IsNullOrEmpty(jn["pulse"]["sca"]["easing"]))
                model.values["Pulse Easing Scale"] = int.Parse(jn["pulse"]["sca"]["easing"]);

            if (!string.IsNullOrEmpty(jn["pulse"]["rot"]["start"]))
                model.values["Pulse Start Rotation"] = float.Parse(jn["pulse"]["rot"]["start"]);
            if (!string.IsNullOrEmpty(jn["pulse"]["rot"]["end"]))
                model.values["Pulse End Rotation"] = float.Parse(jn["pulse"]["rot"]["end"]);
            if (!string.IsNullOrEmpty(jn["pulse"]["rot"]["easing"]))
                model.values["Pulse Easing Rotation"] = int.Parse(jn["pulse"]["rot"]["easing"]);

            if (!string.IsNullOrEmpty(jn["pulse"]["lt"]))
                model.values["Pulse Duration"] = float.Parse(jn["pulse"]["lt"]);

            #endregion

            #region Bullet

            if (jn["bullet"] != null)
            {
                if (!string.IsNullOrEmpty(jn["bullet"]["active"]))
                    model.values["Bullet Active"] = bool.Parse(jn["bullet"]["active"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["ak"]))
                    model.values["Bullet AutoKill"] = bool.Parse(jn["bullet"]["ak"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["speed"]))
                    model.values["Bullet Speed Amount"] = float.Parse(jn["bullet"]["speed"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["lt"]))
                    model.values["Bullet Lifetime"] = float.Parse(jn["bullet"]["lt"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["delay"]))
                    model.values["Bullet Delay Amount"] = float.Parse(jn["bullet"]["delay"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["constant"]))
                    model.values["Bullet Constant"] = bool.Parse(jn["bullet"]["constant"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["hit"]))
                    model.values["Bullet Hurt Players"] = bool.Parse(jn["bullet"]["hit"]);

                if (jn["bullet"]["o"] != null && !string.IsNullOrEmpty(jn["bullet"]["o"]["x"]) && !string.IsNullOrEmpty(jn["bullet"]["o"]["y"]))
                    model.values["Bullet Origin"] = new Vector2(float.Parse(jn["bullet"]["o"]["x"]), float.Parse(jn["bullet"]["o"]["y"]));

                int bulletS = 0;
                int bulletSO = 0;
                if (!string.IsNullOrEmpty(jn["bullet"]["s"]))
                {
                    bulletS = int.Parse(jn["bullet"]["s"]);
                }
                if (!string.IsNullOrEmpty(jn["bullet"]["so"]))
                {
                    bulletSO = int.Parse(jn["bullet"]["so"]);
                }

                model.values["Bullet Shape"] = new Vector2Int(bulletS, bulletSO);

                if (!string.IsNullOrEmpty(jn["bullet"]["col"]["start"]))
                    model.values["Bullet Start Color"] = int.Parse(jn["bullet"]["col"]["start"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["col"]["end"]))
                    model.values["Bullet End Color"] = int.Parse(jn["bullet"]["col"]["end"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["col"]["easing"]))
                    model.values["Bullet Easing Color"] = int.Parse(jn["bullet"]["col"]["easing"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["col"]["dur"]))
                    model.values["Bullet Duration Color"] = float.Parse(jn["bullet"]["col"]["dur"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["opa"]["start"]))
                    model.values["Bullet Start Opacity"] = float.Parse(jn["bullet"]["opa"]["start"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["opa"]["end"]))
                    model.values["Bullet End Opacity"] = float.Parse(jn["bullet"]["opa"]["end"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["opa"]["easing"]))
                    model.values["Bullet Easing Opacity"] = int.Parse(jn["bullet"]["opa"]["easing"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["opa"]["dur"]))
                    model.values["Bullet Duration Opacity"] = float.Parse(jn["bullet"]["opa"]["dur"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["d"]))
                    model.values["Bullet Depth"] = float.Parse(jn["bullet"]["d"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["pos"]["start"]["x"]) && !string.IsNullOrEmpty(jn["bullet"]["pos"]["start"]["y"]))
                    model.values["Bullet Start Position"] = new Vector2(float.Parse(jn["bullet"]["pos"]["start"]["x"]), float.Parse(jn["bullet"]["pos"]["start"]["y"]));
                if (!string.IsNullOrEmpty(jn["bullet"]["pos"]["end"]["x"]) && !string.IsNullOrEmpty(jn["bullet"]["pos"]["end"]["y"]))
                    model.values["Bullet End Position"] = new Vector2(float.Parse(jn["bullet"]["pos"]["end"]["x"]), float.Parse(jn["bullet"]["pos"]["end"]["y"]));
                if (!string.IsNullOrEmpty(jn["bullet"]["pos"]["easing"]))
                    model.values["Bullet Easing Position"] = int.Parse(jn["bullet"]["pos"]["easing"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["pos"]["dur"]))
                    model.values["Bullet Duration Position"] = float.Parse(jn["bullet"]["pos"]["dur"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["sca"]["start"]["x"]) && !string.IsNullOrEmpty(jn["bullet"]["sca"]["start"]["y"]))
                    model.values["Bullet Start Scale"] = new Vector2(float.Parse(jn["bullet"]["sca"]["start"]["x"]), float.Parse(jn["bullet"]["sca"]["start"]["y"]));
                if (!string.IsNullOrEmpty(jn["bullet"]["sca"]["end"]["x"]) && !string.IsNullOrEmpty(jn["bullet"]["sca"]["end"]["y"]))
                    model.values["Bullet End Scale"] = new Vector2(float.Parse(jn["bullet"]["sca"]["end"]["x"]), float.Parse(jn["bullet"]["sca"]["end"]["y"]));
                if (!string.IsNullOrEmpty(jn["bullet"]["sca"]["easing"]))
                    model.values["Bullet Easing Scale"] = int.Parse(jn["bullet"]["sca"]["easing"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["sca"]["dur"]))
                    model.values["Bullet Duration Scale"] = float.Parse(jn["bullet"]["sca"]["dur"]);

                if (!string.IsNullOrEmpty(jn["bullet"]["rot"]["start"]))
                    model.values["Bullet Start Rotation"] = float.Parse(jn["bullet"]["rot"]["start"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["rot"]["end"]))
                    model.values["Bullet End Rotation"] = float.Parse(jn["bullet"]["rot"]["end"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["rot"]["easing"]))
                    model.values["Bullet Easing Rotation"] = int.Parse(jn["bullet"]["rot"]["easing"]);
                if (!string.IsNullOrEmpty(jn["bullet"]["rot"]["dur"]))
                    model.values["Bullet Duration Rotation"] = float.Parse(jn["bullet"]["rot"]["dur"]);
            }

            #endregion

            #region Tail

            model.values["Tail Base Distance"] = float.Parse(jn["tail_base"]["distance"]);
            model.values["Tail Base Mode"] = int.Parse(jn["tail_base"]["mode"]);

            if (!string.IsNullOrEmpty(jn["tail_base"]["grows"]))
                model.values["Tail Base Grows"] = bool.Parse(jn["tail_base"]["grows"]);

            if (!string.IsNullOrEmpty(jn["tail_boost"]["active"]))
                model.values["Tail Boost Active"] = bool.Parse(jn["tail_boost"]["active"]);

            int tailBS = 0;
            int tailBSO = 0;
            if (!string.IsNullOrEmpty(jn["tail_boost"]["s"]))
                tailBS = int.Parse(jn["tail_boost"]["s"]);
            if (!string.IsNullOrEmpty(jn["tail_boost"]["so"]))
                tailBSO = int.Parse(jn["tail_boost"]["so"]);
            model.values["Tail Boost Shape"] = new Vector2Int(tailBS, tailBSO);

            if (!string.IsNullOrEmpty(jn["tail_boost"]["pos"]["x"]) && !string.IsNullOrEmpty(jn["tail_boost"]["pos"]["y"]))
                model.values["Tail Boost Position"] = new Vector2(float.Parse(jn["tail_boost"]["pos"]["x"]), float.Parse(jn["tail_boost"]["pos"]["y"]));

            if (!string.IsNullOrEmpty(jn["tail_boost"]["sca"]["x"]) && !string.IsNullOrEmpty(jn["tail_boost"]["sca"]["y"]))
                model.values["Tail Boost Scale"] = new Vector2(float.Parse(jn["tail_boost"]["sca"]["x"]), float.Parse(jn["tail_boost"]["sca"]["y"]));

            if (!string.IsNullOrEmpty(jn["tail_boost"]["rot"]["x"]))
                model.values["Tail Boost Rotation"] = float.Parse(jn["tail_boost"]["rot"]["x"]);

            if (jn["tail_boost"]["col"] != null && !string.IsNullOrEmpty(jn["tail_boost"]["col"]["x"]))
                model.values["Tail Boost Color"] = int.Parse(jn["tail_boost"]["col"]["x"]);
            if (jn["tail_boost"]["col"] != null && !string.IsNullOrEmpty(jn["tail_boost"]["col"]["hex"]))
                model.values["Tail Boost Custom Color"] = (string)jn["tail_boost"]["col"]["hex"];
            if (jn["tail_boost"]["opa"] != null && !string.IsNullOrEmpty(jn["tail_boost"]["opa"]["x"]))
                model.values["Tail Boost Opacity"] = float.Parse(jn["tail_boost"]["opa"]["x"]);

            for (int i = 1; i < jn["tail"].Count + 1; i++)
            {
                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["active"]))
                    model.values[string.Format("Tail {0} Active", i)] = bool.Parse(jn["tail"][i - 1]["active"]);

                int tailS = 0;
                int tailSO = 0;
                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["s"]))
                    tailS = int.Parse(jn["tail"][i - 1]["s"]);
                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["so"]))
                    tailSO = int.Parse(jn["tail"][i - 1]["so"]);
                model.values[string.Format("Tail {0} Shape", i)] = new Vector2Int(tailS, tailSO);
                model.values[string.Format("Tail {0} Position", i)] = new Vector2(float.Parse(jn["tail"][i - 1]["pos"]["x"]), float.Parse(jn["tail"][i - 1]["pos"]["y"]));
                model.values[string.Format("Tail {0} Scale", i)] = new Vector2(float.Parse(jn["tail"][i - 1]["sca"]["x"]), float.Parse(jn["tail"][i - 1]["sca"]["y"]));
                model.values[string.Format("Tail {0} Rotation", i)] = float.Parse(jn["tail"][i - 1]["rot"]["x"]);

                if (jn["tail"][i - 1]["col"] != null && !string.IsNullOrEmpty(jn["tail"][i - 1]["col"]["x"]))
                    model.values[string.Format("Tail {0} Color", i)] = int.Parse(jn["tail"][i - 1]["col"]["x"]);
                if (jn["tail"][i - 1]["col"] != null && !string.IsNullOrEmpty(jn["tail"][i - 1]["col"]["hex"]))
                    model.values[string.Format("Tail {0} Custom Color", i)] = (string)jn["tail"][i - 1]["col"]["hex"];
                if (jn["tail"][i - 1]["opa"] != null && !string.IsNullOrEmpty(jn["tail"][i - 1]["opa"]["x"]))
                    model.values[string.Format("Tail {0} Opacity", i)] = float.Parse(jn["tail"][i - 1]["opa"]["x"]);

                model.values[string.Format("Tail {0} Trail Emitting", i)] = bool.Parse(jn["tail"][i - 1]["trail"]["em"]);
                model.values[string.Format("Tail {0} Trail Time", i)] = float.Parse(jn["tail"][i - 1]["trail"]["t"]);
                model.values[string.Format("Tail {0} Trail Start Width", i)] = float.Parse(jn["tail"][i - 1]["trail"]["w"]["start"]);
                model.values[string.Format("Tail {0} Trail End Width", i)] = float.Parse(jn["tail"][i - 1]["trail"]["w"]["end"]);

                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["trail"]["c"]["start_hex"]))
                    model.values[string.Format("Tail {0} Trail Start Custom Color", i)] = (string)jn["tail"][i - 1]["trail"]["c"]["start_hex"];

                model.values[string.Format("Tail {0} Trail Start Color", i)] = int.Parse(jn["tail"][i - 1]["trail"]["c"]["start"]);

                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["trail"]["c"]["end_hex"]))
                    model.values[string.Format("Tail {0} Trail End Custom Color", i)] = (string)jn["tail"][i - 1]["trail"]["c"]["end_hex"];

                model.values[string.Format("Tail {0} Trail End Color", i)] = int.Parse(jn["tail"][i - 1]["trail"]["c"]["end"]);
                model.values[string.Format("Tail {0} Trail Start Opacity", i)] = float.Parse(jn["tail"][i - 1]["trail"]["o"]["start"]);
                model.values[string.Format("Tail {0} Trail End Opacity", i)] = float.Parse(jn["tail"][i - 1]["trail"]["o"]["end"]);

                model.values[string.Format("Tail {0} Particles Emitting", i)] = bool.Parse(jn["tail"][i - 1]["particles"]["em"]);

                int tailPS = 0;
                int tailPSO = 0;
                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["particles"]["s"]))
                {
                    tailPS = int.Parse(jn["tail"][i - 1]["particles"]["s"]);
                }
                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["particles"]["so"]))
                {
                    tailPSO = int.Parse(jn["tail"][i - 1]["particles"]["so"]);
                }

                model.values[string.Format("Tail {0} Particles Shape", i)] = new Vector2Int(tailPS, tailPSO);
                model.values[string.Format("Tail {0} Particles Color", i)] = int.Parse(jn["tail"][i - 1]["particles"]["col"]);
                if (!string.IsNullOrEmpty(jn["tail"][i - 1]["particles"]["col_hex"]))
                    model.values[string.Format("Tail {0} Particles Custom Color", i)] = (string)jn["tail"][i - 1]["particles"]["col_hex"];

                model.values[string.Format("Tail {0} Particles Start Opacity", i)] = float.Parse(jn["tail"][i - 1]["particles"]["opa"]["start"]);
                model.values[string.Format("Tail {0} Particles End Opacity", i)] = float.Parse(jn["tail"][i - 1]["particles"]["opa"]["end"]);
                model.values[string.Format("Tail {0} Particles Start Scale", i)] = float.Parse(jn["tail"][i - 1]["particles"]["sca"]["start"]);
                model.values[string.Format("Tail {0} Particles End Scale", i)] = float.Parse(jn["tail"][i - 1]["particles"]["sca"]["end"]);
                model.values[string.Format("Tail {0} Particles Rotation", i)] = float.Parse(jn["tail"][i - 1]["particles"]["rot"]);
                model.values[string.Format("Tail {0} Particles Lifetime", i)] = float.Parse(jn["tail"][i - 1]["particles"]["lt"]);
                model.values[string.Format("Tail {0} Particles Speed", i)] = float.Parse(jn["tail"][i - 1]["particles"]["sp"]);
                model.values[string.Format("Tail {0} Particles Amount", i)] = float.Parse(jn["tail"][i - 1]["particles"]["am"]);
                model.values[string.Format("Tail {0} Particles Force", i)] = new Vector2(float.Parse(jn["tail"][i - 1]["particles"]["frc"]["x"]), float.Parse(jn["tail"][i - 1]["particles"]["frc"]["y"]));
                model.values[string.Format("Tail {0} Particles Trail Emitting", i)] = bool.Parse(jn["tail"][i - 1]["particles"]["trem"]);
            }

            #endregion

            #region Custom Objects
            
            var dictionary = (Dictionary<string, object>)model.values["Custom Objects"];
            if (jn["custom_objects"] != null && jn["custom_objects"].Count > 0)
                for (int i = 0; i < jn["custom_objects"].Count; i++)
                {
                    var id = (string)jn["custom_objects"][i]["id"];
                    dictionary.Add(id, new Dictionary<string, object>());

                    ((Dictionary<string, object>)dictionary[id]).Add("ID", id);

                    string n = "Object Name";
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["n"]))
                        n = jn["custom_objects"][i]["n"];

                    ((Dictionary<string, object>)dictionary[id]).Add("Name", n);

                    int tailS = 0;
                    int tailSO = 0;
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["s"]))
                    {
                        tailS = int.Parse(jn["custom_objects"][i]["s"]);
                    }
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["so"]))
                    {
                        tailSO = int.Parse(jn["custom_objects"][i]["so"]);
                    }

                    ((Dictionary<string, object>)dictionary[id]).Add("Shape", new Vector2Int(tailS, tailSO));
                    ((Dictionary<string, object>)dictionary[id]).Add("Parent", int.Parse(jn["custom_objects"][i]["p"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Parent Position Offset", float.Parse(jn["custom_objects"][i]["ppo"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Parent Scale Offset", float.Parse(jn["custom_objects"][i]["pso"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Parent Rotation Offset", float.Parse(jn["custom_objects"][i]["pro"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Parent Scale Active", bool.Parse(jn["custom_objects"][i]["psa"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Parent Rotation Active", bool.Parse(jn["custom_objects"][i]["pra"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Depth", float.Parse(jn["custom_objects"][i]["d"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Position", new Vector2(float.Parse(jn["custom_objects"][i]["pos"]["x"]), float.Parse(jn["custom_objects"][i]["pos"]["y"])));
                    ((Dictionary<string, object>)dictionary[id]).Add("Scale", new Vector2(float.Parse(jn["custom_objects"][i]["sca"]["x"]), float.Parse(jn["custom_objects"][i]["sca"]["y"])));
                    ((Dictionary<string, object>)dictionary[id]).Add("Rotation", float.Parse(jn["custom_objects"][i]["rot"]["x"]));
                    ((Dictionary<string, object>)dictionary[id]).Add("Color", int.Parse(jn["custom_objects"][i]["col"]["x"]));

                    string hex = "FFFFFF";
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["col"]["hex"]))
                    {
                        hex = (string)jn["custom_objects"][i]["col"]["hex"];
                    }
                    ((Dictionary<string, object>)dictionary[id]).Add("Custom Color", hex);

                    float opacity = 1f;
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["opa"]["x"]))
                    {
                        opacity = float.Parse(jn["custom_objects"][i]["opa"]["x"]);
                    }

                    ((Dictionary<string, object>)dictionary[id]).Add("Opacity", opacity);

                    int visib = 0;
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["v"]))
                        visib = int.Parse(jn["custom_objects"][i]["v"]);

                    ((Dictionary<string, object>)dictionary[id]).Add("Visibility", visib);

                    float visip = 100f;
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["vhp"]))
                        visip = float.Parse(jn["custom_objects"][i]["vhp"]);

                    ((Dictionary<string, object>)dictionary[id]).Add("Visibility Value", visip);

                    bool visin = false;
                    if (!string.IsNullOrEmpty(jn["custom_objects"][i]["vn"]))
                        visin = bool.Parse(jn["custom_objects"][i]["vn"]);

                    ((Dictionary<string, object>)dictionary[id]).Add("Visibility Not", visin);
                }
            #endregion

            return model;
        }
    }
}
