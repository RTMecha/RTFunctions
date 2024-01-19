using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

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
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
        }

        public Camera perspectiveCam;
        public PostProcessLayer postProcessLayer;
        public Transform extraBG;
        public Transform video;
        public Material bgMaterial;
    }
}
