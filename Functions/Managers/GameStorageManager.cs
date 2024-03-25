using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

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
        }

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
