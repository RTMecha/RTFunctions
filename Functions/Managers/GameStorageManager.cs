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
            if (RTVideoManager.inst)
            {
                RTVideoManager.inst.SetCamera(perspectiveCam);
            }
        }

        public Camera perspectiveCam;
        public PostProcessLayer postProcessLayer;
    }
}
