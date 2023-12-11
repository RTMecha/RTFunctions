using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.Components;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Optimization;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;
using BaseBackground = DataManager.GameData.BackgroundObject;
using BaseEditorData = DataManager.GameData.BeatmapObject.EditorData;

namespace RTFunctions.Functions.Managers
{
    public class ShapeManager : MonoBehaviour
    {
        public static ShapeManager inst;

        public static Dictionary<string, Sprite> shapeSprites = new Dictionary<string, Sprite>();

        public static string ShapesPath => "beatmaps/shapes/";
        public static string ShapesSetup => $"{ShapesPath}setup.lss";

        bool gameHasLoaded;
        bool loadedShapes;

        void Awake() => inst = this;

        void Start()
        {
            SetupMeshes();
        }

        void Update()
        {
            if (ObjectManager.inst && !gameHasLoaded)
            {
                gameHasLoaded = true;
                Load();
            }
            else if (!ObjectManager.inst)
            {
                gameHasLoaded = false;
            }
        }

        void SetupMeshes()
        {
            shapes3D = new List<Shape>();
            shapes = new List<Shape>();

            var files = System.IO.Directory.GetFiles(RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/Shape Assets", "*.lssh");

            foreach (var file in files)
            {
                var jn = SimpleJSON.JSON.Parse(RTFile.ReadFromFile(file));
                var shape = Shape.Parse(jn);

                if (file.Replace("\\", "/").Contains("Shape Assets/bg_"))
                    shapes3D.Add(shape);
                else
                    shapes.Add(shape);
            }
        }

        public void Save()
        {
            var jn = JSON.Parse("{}");
            for (int i = 0; i < ObjectManager.inst.objectPrefabs.Count; i++)
            {
                for (int j = 0; j < ObjectManager.inst.objectPrefabs[i].options.Count; j++)
                {
                    var name = ObjectManager.inst.objectPrefabs[i].options[j].name;
                    jn["type"][i]["option"][j]["path"] = $"beatmaps/shapes/{name}";

                    var fullPath = $"{RTFile.ApplicationDirectory}beatmaps/shapes/{name}";
                    if (!RTFile.DirectoryExists(fullPath))
                        Directory.CreateDirectory(fullPath);

                    var sjn = JSON.Parse("{}");

                    sjn["name"] = name;
                    sjn["s"] = i;
                    sjn["so"] = j;

                    if (i != 4 && i != 6)
                    {
                        var mesh = ObjectManager.inst.objectPrefabs[i].options[j].transform.GetChild(0).GetComponent<MeshFilter>().mesh;

                        for (int k = 0; k < mesh.vertices.Length; k++)
                        {
                            sjn["verts"][k]["x"] = mesh.vertices[k].x.ToString();
                            sjn["verts"][k]["y"] = mesh.vertices[k].y.ToString();
                            sjn["verts"][k]["z"] = mesh.vertices[k].z.ToString();
                        }

                        for (int k = 0; k < mesh.triangles.Length; k++)
                        {
                            sjn["tris"][k] = mesh.triangles[k].ToString();
                        }
                    }
                    else
                    {
                        sjn["p"] = i == 4 ? 1 : 2;
                    }

                    RTFile.WriteToFile(fullPath + "/data.lssh", jn.ToString());
                }
            }

            RTFile.WriteToFile(RTFile.ApplicationDirectory + "beatmaps/shapes/setup.lss", jn.ToString(3));
        }

        public void Load()
        {
            if (!RTFile.FileExists(RTFile.ApplicationDirectory + ShapesSetup))
                System.Windows.Forms.MessageBox.Show("Shapes Setup file does not exist.\nYou may run into issues with playing the game from here on, so it is recommended to\ndownload the proper assets from Github and place them into the appropriate folders.", "Error!");

            Shapes2D = new List<List<Shape>>();

            var jn = JSON.Parse(RTFile.ReadFromFile(RTFile.ApplicationDirectory + ShapesSetup));
            for (int i = 0; i < jn["type"].Count; i++)
            {
                Shapes2D.Add(new List<Shape>());
                for (int j = 0; j < jn["type"][i]["option"].Count; j++)
                {
                    var fullPath = RTFile.ApplicationDirectory + jn["type"][i]["option"][j]["path"];

                    Debug.Log($"{FunctionsPlugin.className}Loading shape from: {jn["type"][i]["option"][j]["path"]}");
                    var sjn = JSON.Parse(RTFile.ReadFromFile(fullPath + "/data.lssh"));

                    Mesh mesh = null;
                    if (i != 4 && i != 6 && sjn["verts"] != null && sjn["tris"] != null)
                    {
                        Debug.Log($"{FunctionsPlugin.className}Setting mesh data for {sjn["name"]}");
                        mesh = new Mesh();
                        mesh.name = sjn["name"];
                        Vector3[] vertices = new Vector3[sjn["verts"].Count];
                        for (int k = 0; k < sjn["verts"].Count; k++)
                            vertices[k] = new Vector3(sjn["verts"][k]["x"].AsFloat, sjn["verts"][k]["y"].AsFloat, sjn["verts"][k]["z"].AsFloat);

                        int[] triangles = new int[sjn["tris"].Count];
                        for (int k = 0; k < sjn["tris"].Count; k++)
                            triangles[k] = sjn["tris"][k].AsInt;

                        mesh.vertices = vertices;
                        mesh.triangles = triangles;
                    }

                    if (ObjectManager.inst.objectPrefabs.Count < i + 1)
                    {
                        Debug.Log($"{FunctionsPlugin.className}Adding new ObjectPrefabHolder [{i}]");
                        var p = new ObjectManager.ObjectPrefabHolder();
                        p.options = new List<GameObject>();
                        ObjectManager.inst.objectPrefabs.Add(p);
                    }

                    if (ObjectManager.inst.objectPrefabs[i].options.Count < j + 1 && mesh != null)
                    {
                        Debug.Log($"{FunctionsPlugin.className}Adding new ObjectPrefab [{i}, {j}]");
                        var gameObject = ObjectManager.inst.objectPrefabs[1].options[0].Duplicate(null, sjn["name"]);

                        gameObject.transform.GetChild(0).GetComponent<MeshFilter>().mesh = mesh;
                        gameObject.transform.GetChild(0).GetComponent<PolygonCollider2D>().points = mesh.vertices.Select(x => new Vector2(x.x, x.y)).ToArray();

                        gameObject.hideFlags = HideFlags.HideAndDontSave;

                        ObjectManager.inst.objectPrefabs[i].options.Add(gameObject);
                    }

                    if (!loadedShapes)
                    {
                        var shape = new Shape(sjn["name"], sjn["s"].AsInt, sjn["so"].AsInt, mesh, null, string.IsNullOrEmpty(sjn["p"]) ? Shape.Property.RegularObject : (Shape.Property)sjn["p"].AsInt);

                        //if (RTFile.FileExists(fullPath + "/icon.png"))
                        //{
                        //    Debug.Log($"{FunctionsPlugin.className}Setting Icon for {sjn["name"]}");
                        //    Networking.AlephNetworkManager.inst.StartCoroutine(Networking.AlephNetworkManager.DownloadImageTexture("file://" + fullPath + "/icon.png", delegate (Texture2D texture2D)
                        //    {
                        //        var s = Shapes2D[i][j];
                        //        s.Icon = RTSpriteManager.CreateSprite(texture2D);
                        //        Shapes2D[i][j] = s;
                        //    }));
                        //}

                        shape.GameObject = ObjectManager.inst.objectPrefabs[i].options[j];

                        Shapes2D[i].Add(shape);
                    }
                }
            }

            StartCoroutine(LoadIcons());

            loadedShapes = true;
        }

        IEnumerator LoadIcons()
        {
            for (int i = 0; i < Shapes2D.Count; i++)
            {
                for (int j = 0; j < Shapes2D[i].Count; j++)
                {
                    string fullPath = RTFile.ApplicationDirectory + ShapesPath + Shapes2D[i][j].name + "/icon.png";
                    if (RTFile.FileExists(fullPath))
                    {
                        Debug.Log($"{FunctionsPlugin.className}Setting Icon for {Shapes2D[i][j].name}");
                        yield return Networking.AlephNetworkManager.inst.StartCoroutine(Networking.AlephNetworkManager.DownloadImageTexture("file://" + fullPath, delegate (Texture2D texture2D)
                        {
                            var s = Shapes2D[i][j];
                            s.Icon = RTSpriteManager.CreateSprite(texture2D);
                            Shapes2D[i][j] = s;
                        }));
                    }
                }
            }

            yield break;
        }

        public List<List<Shape>> Shapes2D { get; set; }

        static List<Shape> shapes;
        public static List<Shape> Shapes
        {
            get => shapes;
            set => shapes = value;
        }
        
        static List<Shape> shapes3D;
        public static List<Shape> Shapes3D
        {
            get => shapes3D;
            set => shapes3D = value;
        }

        public static Shape GetShape(int shape, int shapeOption)
        {
            if (!Shapes.Has(x => x.Type == shape && x.Option == shapeOption))
                return Shapes[0];

            return Shapes.Find(x => x.Type == shape && x.Option == shapeOption);
        }

        public static Shape GetShape3D(int shape, int shapeOption) => Shapes3D.Find(x => x.Type == shape && x.Option == shapeOption);
    }
}
