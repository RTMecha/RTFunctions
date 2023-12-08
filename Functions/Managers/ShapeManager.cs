using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using TMPro;

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

        void Awake() => inst = this;

        void Start()
        {
            SetupMeshes();
        }

        void Update()
        {
            
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

        public static Dictionary<string, Shape> ShapesDictionary
        {
            get
            {
                var dictionary = new Dictionary<string, Shape>();
                foreach (var shape in Shapes)
                {
                    if (!dictionary.ContainsKey(shape.name))
                        dictionary.Add(shape.name, shape);
                }
                return dictionary;
            }
            set => shapes = value.Values.ToList();
        }
        
        public static Dictionary<string, Shape> Shapes3DDictionary
        {
            get
            {
                var dictionary = new Dictionary<string, Shape>();
                foreach (var shape in Shapes3D)
                {
                    if (!dictionary.ContainsKey(shape.name))
                        dictionary.Add(shape.name, shape);
                }
                return dictionary;
            }
            set => shapes3D = value.Values.ToList();
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
