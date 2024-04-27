using RTFunctions.Functions.Components;
using RTFunctions.Functions.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

using Axis = RTFunctions.Functions.Components.RTObject.Axis;

namespace RTFunctions.Functions.Managers
{
    public class GameStorageManager : MonoBehaviour
    {
        public static GameStorageManager inst;

        void Awake()
        {
            inst = this;
            perspectiveCam = GameManager.inst.CameraPerspective.GetComponent<Camera>();
            postProcessLayer = Camera.main.gameObject.GetComponent<PostProcessLayer>();
            extraBG = GameObject.Find("ExtraBG").transform;
            video = extraBG.GetChild(0);

            try
            {
                bgMaterial = BackgroundManager.inst.backgroundPrefab.GetComponent<MeshRenderer>().material;
                interfaceBlur = GameManager.inst.menuUI.GetComponentInChildren<Image>();
                playerGUICanvasScaler = GameManager.inst.playerGUI.GetComponent<CanvasScaler>();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            timelinePlayer = GameManager.inst.timeline.transform.Find("Base/position").GetComponent<Image>();
            timelineLeftCap = GameManager.inst.timeline.transform.Find("Base/Image").GetComponent<Image>();
            timelineRightCap = GameManager.inst.timeline.transform.Find("Base/Image 1").GetComponent<Image>();
            timelineLine = GameManager.inst.timeline.transform.Find("Base").GetComponent<Image>();

            var objectDragger = new GameObject("Dragger");
            objectDragger.transform.SetParent(GameManager.inst.transform.parent);
            objectDragger.transform.localScale = Vector3.one;
            this.objectDragger = objectDragger.transform;

            var rotator = ObjectManager.inst.objectPrefabs[1].options[4].transform.GetChild(0).gameObject.Duplicate(this.objectDragger, "Rotator");
            Destroy(rotator.GetComponent<SelectObjectInEditor>());
            rotator.tag = "Helper";
            rotator.transform.localScale = new Vector3(2f, 2f, 1f);
            var rotatorRenderer = rotator.GetComponent<Renderer>();
            rotatorRenderer.enabled = true;
            rotatorRenderer.material.color = new Color(0f, 0f, 1f);
            rotator.GetComponent<Collider2D>().enabled = true;
            objectRotator = rotator.AddComponent<RTRotator>();

            rotator.SetActive(true);

            objectDragger.SetActive(false);

            objectScalerTop = CreateScaler(Axis.PosY, Color.green);
            objectScalerLeft = CreateScaler(Axis.PosX, Color.red);
            objectScalerBottom = CreateScaler(Axis.NegY, Color.green);
            objectScalerRight = CreateScaler(Axis.NegX, Color.red);
        }

        void Update() => objectDragger.gameObject.SetActive(EditorManager.inst && EditorManager.inst.isEditing &&
                ModCompatibility.sharedFunctions.ContainsKey("SelectedObjectCount") && (int)ModCompatibility.sharedFunctions["SelectedObjectCount"] == 1 &&
                ModCompatibility.sharedFunctions.ContainsKey("CurrentSelection") && ModCompatibility.sharedFunctions["CurrentSelection"] is TimelineObject currentSelection &&
                (currentSelection.IsBeatmapObject && currentSelection.GetData<BeatmapObject>().objectType != BeatmapObject.ObjectType.Empty || currentSelection.IsPrefabObject) &&
                RTObject.Enabled);

        RTScaler CreateScaler(Axis axis, Color color)
        {
            var scaler = ObjectManager.inst.objectPrefabs[3].options[0].transform.GetChild(0).gameObject.Duplicate(objectDragger, "Scaler");
            Destroy(scaler.GetComponent<SelectObjectInEditor>());
            scaler.tag = "Helper";
            scaler.transform.localScale = new Vector3(2f, 2f, 1f);
            scaler.GetComponent<Collider2D>().enabled = true;

            var scalerRenderer = scaler.GetComponent<Renderer>();
            scalerRenderer.enabled = true;
            scalerRenderer.material.color = color;

            var s = scaler.AddComponent<RTScaler>();
            s.axis = axis;

            scaler.SetActive(true);

            return s;
        }

        public RTRotator objectRotator;
        public RTScaler objectScalerTop;
        public RTScaler objectScalerLeft;
        public RTScaler objectScalerRight;
        public RTScaler objectScalerBottom;

        public Transform objectDragger;

        public CanvasScaler playerGUICanvasScaler;

        public Image timelinePlayer;
        public Image timelineLine;
        public Image timelineLeftCap;
        public Image timelineRightCap;
        public List<Image> checkpointImages = new List<Image>();
        public Camera perspectiveCam;
        public PostProcessLayer postProcessLayer;
        public Transform extraBG;
        public Transform video;
        public Material bgMaterial;
        public Image interfaceBlur;

        public Dictionary<string, object> assets = new Dictionary<string, object>();
    }
}
