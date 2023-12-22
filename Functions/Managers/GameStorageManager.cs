using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
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
        }

        public Camera perspectiveCam;

    }
}
