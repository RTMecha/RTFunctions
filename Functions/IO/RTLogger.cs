using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using LSFunctions;

using RTFunctions.Functions.Components;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.IO
{
    public static class RTLogger
    {
        public static int LogsCap => FunctionsPlugin.LogPopupCap.Value;

        public static GameObject loggerCanvas;
        public static Transform loggerContent;
        public static CanvasScaler canvasScaler;

        public static void Init()
        {
            CreateLogPopup();
        }

        public static void CreateLogPopup()
        {
            loggerCanvas = new GameObject("Canvas");
            UnityEngine.Object.DontDestroyOnLoad(loggerCanvas);
            loggerCanvas.transform.localScale = Vector3.one * RTHelpers.screenScale;
            var interfaceRT = loggerCanvas.AddComponent<RectTransform>();
            interfaceRT.anchoredPosition = new Vector2(960f, 540f);
            interfaceRT.sizeDelta = new Vector2(1920f, 1080f);
            interfaceRT.pivot = new Vector2(0.5f, 0.5f);
            interfaceRT.anchorMin = Vector2.zero;
            interfaceRT.anchorMax = Vector2.zero;

            var canvas = loggerCanvas.AddComponent<Canvas>();
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Tangent;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.scaleFactor = RTHelpers.screenScale;
            canvas.sortingOrder = 1000;

            canvasScaler = loggerCanvas.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            canvasScaler.scaleFactor = RTHelpers.screenScale;

            loggerCanvas.AddComponent<GraphicRaycaster>();

            // Base
            {
                var t1 = UIManager.GenerateUIImage("Base", interfaceRT);
                var t1GO = (GameObject)t1["GameObject"];

                var t1RT = (RectTransform)t1["RectTransform"];

                t1RT.anchoredPosition = Vector2.zero;
                t1RT.sizeDelta = new Vector2(800f, 600f);

                var t1Im = (Image)t1["Image"];
                t1Im.color = new Color(0.0686792f, 0.0686792f, 0.0686792f);

                var t1SR = t1GO.AddComponent<ScrollRect>();
                t1SR.scrollSensitivity = 20f;

                var selection = t1GO.AddComponent<SelectGUI>();
                selection.OverrideDrag = true;
                selection.target = t1RT;

                var t2 = UIManager.GenerateUIImage("Panel", t1RT);
                var t2GO = (GameObject)t2["GameObject"];

                var t2RT = (RectTransform)t2["RectTransform"];

                t2RT.anchoredPosition = new Vector2(0f, 316f);
                t2RT.sizeDelta = new Vector2(800f, 32f);

                var t2Im = (Image)t2["Image"];
                t2Im.color = new Color(0.4524f, 0.4524f, 0.4524f);

                var t3 = UIManager.GenerateUIButton("Close", (RectTransform)t2["RectTransform"]);
                var t3GO = (GameObject)t3["GameObject"];

                var t3RT = (RectTransform)t3["RectTransform"];

                t3RT.anchoredPosition = new Vector2(416f, 0f);
                t3RT.sizeDelta = new Vector2(32f, 32f);

                var t3Bu = (Button)t3["Button"];

                var t3CB = t3Bu.colors;
                t3CB.normalColor = new Color(0.9569f, 0.2627f, 0.2118f);
                t3CB.highlightedColor = new Color(0.1647f, 0.1647f, 0.1647f);
                t3CB.pressedColor = new Color(0.1294f, 0.1294f, 0.1294f);
                t3CB.selectedColor = new Color(0.1647f, 0.1647f, 0.1647f);
                t3CB.disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f);
                t3CB.colorMultiplier = 1f;
                t3CB.fadeDuration = 0.1f;
                t3Bu.colors = t3CB;

                t3Bu.onClick.AddListener(delegate ()
                {
                    FunctionsPlugin.ShowLogPopup.Value = false;
                });

                var t4 = UIManager.GenerateUIImage("Image", (RectTransform)t3["RectTransform"]);

                var t4RT = (RectTransform)t4["RectTransform"];
                t4RT.anchoredPosition = Vector2.zero;
                t4RT.sizeDelta = new Vector2(32f, 32f);

                var t4Im = (Image)t4["Image"];

                FunctionsPlugin.inst.StartCoroutine(Managers.Networking.AlephNetworkManager.DownloadImageTexture($"file://{RTFile.ApplicationDirectory}BepInEx/plugins/Assets/editor_gui_close.png", delegate (Texture2D texture2D)
                {
                    t4Im.sprite = RTSpriteManager.CreateSprite(texture2D);
                }));

                var t5 = UIManager.GenerateUIText("Title", t2RT);

                var t5RT = (RectTransform)t5["RectTransform"];
                t5RT.anchoredPosition = new Vector2(-340f, 0f);
                t5RT.sizeDelta = new Vector2(100f, 32f);

                var t5Tx = (Text)t5["Text"];
                t5Tx.text = "RT Logger";
                t5Tx.alignment = TextAnchor.MiddleLeft;

                var t6 = UIManager.GenerateUIImage("Mask", t1RT);
                var t6GO = (GameObject)t6["GameObject"];

                var mask = t6GO.AddComponent<Mask>();
                mask.showMaskGraphic = false;

                var t6RT = (RectTransform)t6["RectTransform"];
                t6RT.anchoredPosition = Vector2.zero;
                t6RT.sizeDelta = Vector2.zero;
                t6RT.anchorMax = Vector2.one;
                t6RT.anchorMin = Vector2.zero;
                t6RT.pivot = new Vector2(0.5f, 0.5f);

                var t6Im = (Image)t6["Image"];
                t6Im.color = new Color(1f, 1f, 1f, 0.033f);

                var t7 = new GameObject("content");
                t7.transform.SetParent((RectTransform)t6["RectTransform"]);
                t7.transform.localScale = Vector3.one;

                var t7RT = t7.AddComponent<RectTransform>();
                t7RT.anchoredPosition = Vector2.zero;
                t7RT.sizeDelta = new Vector2(800f, 1796f);
                t7RT.anchorMax = new Vector2(0f, 1f);
                t7RT.anchorMin = new Vector2(0f, 1f);
                t7RT.pivot = new Vector2(0f, 1f);

                var t7CSF = t7.AddComponent<ContentSizeFitter>();
                //var t7GLG = t7.AddComponent<GridLayoutGroup>();
                var t7VLG = t7.AddComponent<VerticalLayoutGroup>();

                t7CSF.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                t7CSF.verticalFit = ContentSizeFitter.FitMode.MinSize;

                t7VLG.childControlHeight = false;
                t7VLG.childForceExpandHeight = false;
                t7VLG.spacing = 6f;

                //t7GLG.cellSize = new Vector2(1800f, 32f);
                //t7GLG.spacing = new Vector2(6f, 6f);
                //t7GLG.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                //t7GLG.constraintCount = 1;
                //t7GLG.startAxis = GridLayoutGroup.Axis.Vertical;
                //t7GLG.startCorner = GridLayoutGroup.Corner.UpperLeft;
                //t7GLG.childAlignment = TextAnchor.UpperLeft;

                t1SR.content = t7RT;
                loggerContent = t7RT;

                var t8 = UIManager.GenerateUIImage("Scrollbar", t1RT);
                var t8GO = (GameObject)t8["GameObject"];

                var t8SB = t8GO.AddComponent<Scrollbar>();
                t8SB.direction = Scrollbar.Direction.BottomToTop;

                var t8RT = (RectTransform)t8["RectTransform"];
                t8RT.anchoredPosition = new Vector2(0f, 0f);
                t8RT.anchorMax = Vector2.one;
                t8RT.anchorMin = new Vector2(1f, 0f);
                t8RT.pivot = new Vector2(0f, 0.5f);
                t8RT.sizeDelta = new Vector2(32f, 0f);

                var t8Im = (Image)t8["Image"];
                t8Im.color = new Color(0.1f, 0.1f, 0.1f);

                var t9 = new GameObject("Sliding Area");
                t9.transform.SetParent(t8RT);
                t9.transform.localScale = Vector3.one;
                var t9RT = t9.AddComponent<RectTransform>();
                t9RT.anchoredPosition = Vector2.zero;
                t9RT.anchorMax = Vector2.one;
                t9RT.anchorMin = Vector2.zero;
                t9RT.pivot = new Vector2(0.5f, 0.5f);
                t9RT.sizeDelta = new Vector2(-20f, -20f);

                var t10 = UIManager.GenerateUIImage("Handle", t9RT);

                var t10RT = (RectTransform)t10["RectTransform"];
                t10RT.anchoredPosition = Vector2.zero;
                t10RT.sizeDelta = new Vector2(20f, 20f);

                t8SB.handleRect = t10RT;
                t1SR.verticalScrollbar = t8SB;

                var t11 = UIManager.GenerateUIButton("Clear", (RectTransform)t2["RectTransform"]);
                var t11GO = (GameObject)t11["GameObject"];

                var t11RT = (RectTransform)t11["RectTransform"];

                t11RT.anchoredPosition = new Vector2(328f, 0f);
                t11RT.sizeDelta = new Vector2(128f, 32f);

                var t11Bu = (Button)t11["Button"];

                var t11CB = t11Bu.colors;
                t11CB.normalColor = new Color(0.1369f, 0.4627f, 0.8918f);
                t11CB.highlightedColor = new Color(0.1647f, 0.7047f, 1f);
                t11CB.pressedColor = new Color(0.5047f, 0.9047f, 1f);
                t11CB.selectedColor = new Color(0.5047f, 0.9047f, 1f);
                t11CB.disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f);
                t11CB.colorMultiplier = 1f;
                t11CB.fadeDuration = 0.1f;
                t11Bu.colors = t11CB;

                t11Bu.onClick.AddListener(delegate ()
                {
                    Clear();
                });

                var t12 = UIManager.GenerateUIText("Text", t11RT);

                var t12RT = (RectTransform)t12["RectTransform"];
                t12RT.anchoredPosition = Vector2.zero;
                t12RT.sizeDelta = new Vector2(64f, 32f);

                var t12Txt = (Text)t12["Text"];
                t12Txt.text = "Clear";
                t12Txt.alignment = TextAnchor.MiddleCenter;
            }


        }

        public static void RefreshLogPopup()
        {
            while (logs.Count > LogsCap)
                logs.RemoveAt(0);

            LSHelpers.DeleteChildren(loggerContent);

            canvasScaler.scaleFactor = RTHelpers.screenScale;

            for (int i = 0; i < logs.Count; i++)
            {
                var evenOdd = i % 2 == 0;

                var l1 = UIManager.GenerateUIInputField($"Log {i}", loggerContent);
                var l1IF = (InputField)l1["InputField"];
                l1IF.interactable = false;
                l1IF.textComponent.color = new Color(0.94f, 0.94f, 0.94f);
                l1IF.lineType = InputField.LineType.MultiLineNewline;

                var cb = l1IF.colors;
                var dis = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f);
                cb.normalColor = dis;
                cb.pressedColor = dis;
                cb.selectedColor = dis;
                cb.highlightedColor = dis;
                cb.disabledColor = dis;
                l1IF.colors = cb;

                l1IF.text = logs[i];
                l1IF.readOnly = true;

                var l1Im = (Image)l1["Image"];
                l1Im.color = evenOdd ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.25f, 0.25f, 0.25f);

                var l1RT = (RectTransform)l1["RectTransform"];
                l1RT.sizeDelta = new Vector2(0f, 32f * logs[i].GetLines().Count);
            }
        }

        public static void Update()
        {
            loggerCanvas?.SetActive(FunctionsPlugin.ShowLogPopup.Value);
        }

        public static void Clear()
        {
            logs.Clear();
            LSHelpers.DeleteChildren(loggerContent);
        }

        public static List<string> logs = new List<string>();

        public static void AddLog(string log)
        {
            if (!FunctionsPlugin.DebugsOn.Value || loggerCanvas == null || loggerContent == null)
                return;

            while (logs.Count > LogsCap)
                logs.RemoveAt(0);

            logs.Add(log);
            if (FunctionsPlugin.ShowLogPopup.Value)
                RefreshLogPopup();
        }
    }
}
