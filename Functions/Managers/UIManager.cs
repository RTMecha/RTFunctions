﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static GameObject textMeshPro;
        public static Material fontMaterial;
        public static Font inconsolataFont = Font.GetDefault();

        void Awake()
        {
            var findButton = (from x in Resources.FindObjectsOfTypeAll<GameObject>()
                              where x.name == "Text Element"
                              select x).ToList();

            textMeshPro = findButton[0].transform.GetChild(1).gameObject;
            fontMaterial = textMeshPro.GetComponent<TextMeshProUGUI>().fontMaterial;
        }

        public static void SetRectTransform(RectTransform _rt, Vector2 _anchoredPos, Vector2 _anchorMax, Vector2 _anchorMin, Vector2 _pivot, Vector2 _sizeDelta)
        {
            _rt.anchoredPosition = _anchoredPos;
            _rt.anchorMax = _anchorMax;
            _rt.anchorMin = _anchorMin;
            _rt.pivot = _pivot;
            _rt.sizeDelta = _sizeDelta;
        }

        public static Image GetImage(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("Image"))
                return (Image)dictionary["Image"];
            return null;
        }
        
        public static Text GetText(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("Text"))
                return (Text)dictionary["Text"];
            return null;
        }
        
        public static RectTransform GetRectTransform(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("RectTransform"))
                return (RectTransform)dictionary["RectTransform"];
            return null;
        }
        
        public static InputField GetInputField(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("InputField"))
                return (InputField)dictionary["InputField"];
            return null;
        }
        
        public static Button GetButton(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("Button"))
                return (Button)dictionary["Button"];
            return null;
        }
        
        public static Toggle GetToggle(Dictionary<string, object> dictionary)
        {
            if (dictionary.ContainsKey("Toggle"))
                return (Toggle)dictionary["Toggle"];
            return null;
        }

        public static Dictionary<string, object> GenerateUIImage(string _name, Transform _parent)
        {
            var dictionary = new Dictionary<string, object>();
            var gameObject = new GameObject(_name);
            gameObject.transform.SetParent(_parent);
            gameObject.transform.localScale = Vector3.one;
            gameObject.layer = 5;

            dictionary.Add("GameObject", gameObject);
            dictionary.Add("RectTransform", gameObject.AddComponent<RectTransform>());
            dictionary.Add("CanvasRenderer", gameObject.AddComponent<CanvasRenderer>());
            dictionary.Add("Image", gameObject.AddComponent<Image>());

            return dictionary;
        }

        public static Dictionary<string, object> GenerateUIText(string _name, Transform _parent)
        {
            var dictionary = new Dictionary<string, object>();
            var gameObject = new GameObject(_name);
            gameObject.transform.SetParent(_parent);
            gameObject.transform.localScale = Vector3.one;
            gameObject.layer = 5;

            dictionary.Add("GameObject", gameObject);
            dictionary.Add("RectTransform", gameObject.AddComponent<RectTransform>());
            dictionary.Add("CanvasRenderer", gameObject.AddComponent<CanvasRenderer>());
            var text = gameObject.AddComponent<Text>();
            text.font = Font.GetDefault();
            text.fontSize = 20;
            dictionary.Add("Text", text);

            return dictionary;
        }

        public static Dictionary<string, object> GenerateUITextMeshPro(string _name, Transform _parent, bool _noFont = false)
        {
            var dictionary = new Dictionary<string, object>();
            var gameObject = Instantiate(textMeshPro);
            gameObject.name = _name;
            gameObject.transform.SetParent(_parent);
            gameObject.transform.localScale = Vector3.one;

            dictionary.Add("GameObject", gameObject);
            dictionary.Add("RectTransform", gameObject.GetComponent<RectTransform>());
            dictionary.Add("CanvasRenderer", gameObject.GetComponent<CanvasRenderer>());
            var text = gameObject.GetComponent<TextMeshProUGUI>();

            if (_noFont)
            {
                var refer = MaterialReferenceManager.instance;
                var dictionary2 = (Dictionary<int, TMP_FontAsset>)refer.GetType().GetField("m_FontAssetReferenceLookup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(refer);

                TMP_FontAsset tmpFont;
                if (dictionary2.ToList().Find(x => x.Value.name == "Arial").Value != null)
                {
                    tmpFont = dictionary2.ToList().Find(x => x.Value.name == "Arial").Value;
                }
                else
                {
                    tmpFont = dictionary2.ToList().Find(x => x.Value.name == "Liberation Sans SDF").Value;
                }

                text.font = tmpFont;
                text.fontSize = 20;
            }

            dictionary.Add("Text", text);

            return dictionary;
        }

        public static Dictionary<string, object> GenerateUIInputField(string _name, Transform _parent)
        {
            var dictionary = new Dictionary<string, object>();
            var image = GenerateUIImage(_name, _parent);
            var text = GenerateUIText("text", ((GameObject)image["GameObject"]).transform);
            var placeholder = GenerateUIText("placeholder", ((GameObject)image["GameObject"]).transform);

            SetRectTransform((RectTransform)text["RectTransform"], new Vector2(2f, 0f), Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(-12f, -8f));
            SetRectTransform((RectTransform)placeholder["RectTransform"], new Vector2(2f, 0f), Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(-12f, -8f));

            dictionary.Add("GameObject", image["GameObject"]);
            dictionary.Add("RectTransform", image["RectTransform"]);
            dictionary.Add("Image", image["Image"]);
            dictionary.Add("Text", text["Text"]);
            dictionary.Add("Placeholder", placeholder["Text"]);
            var inputField = ((GameObject)image["GameObject"]).AddComponent<InputField>();
            inputField.textComponent = (Text)text["Text"];
            inputField.placeholder = (Text)placeholder["Text"];
            dictionary.Add("InputField", inputField);

            return dictionary;
        }

        public static Dictionary<string, object> GenerateUIButton(string _name, Transform _parent)
        {
            var gameObject = GenerateUIImage(_name, _parent);
            gameObject.Add("Button", ((GameObject)gameObject["GameObject"]).AddComponent<Button>());

            return gameObject;
        }

        public static Dictionary<string, object> GenerateUIToggle(string _name, Transform _parent)
        {
            var dictionary = new Dictionary<string, object>();
            var gameObject = new GameObject(_name);
            gameObject.transform.SetParent(_parent);
            gameObject.transform.localScale = Vector3.one;
            dictionary.Add("GameObject", gameObject);
            dictionary.Add("RectTransform", gameObject.AddComponent<RectTransform>());

            var bg = GenerateUIImage("Background", gameObject.transform);
            dictionary.Add("Background", bg["GameObject"]);
            dictionary.Add("BackgroundRT", bg["RectTransform"]);
            dictionary.Add("BackgroundImage", bg["Image"]);

            var checkmark = GenerateUIImage("Checkmark", ((GameObject)bg["GameObject"]).transform);
            dictionary.Add("Checkmark", checkmark["GameObject"]);
            dictionary.Add("CheckmarkRT", checkmark["RectTransform"]);
            dictionary.Add("CheckmarkImage", checkmark["Image"]);

            var toggle = gameObject.AddComponent<Toggle>();
            toggle.image = (Image)bg["Image"];
            toggle.targetGraphic = (Image)bg["Image"];
            toggle.graphic = (Image)checkmark["Image"];
            dictionary.Add("Toggle", toggle);

            ((Image)checkmark["Image"]).color = new Color(0.1216f, 0.1216f, 0.1216f, 1f);

            GetImage((Image)checkmark["Image"], RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/editor_gui_checkmark.png");

            return dictionary;
        }

        public static Dictionary<string, object> GenerateUIDropdown(string _name, Transform _parent)
        {
            var dictionary = new Dictionary<string, object>();
            var dropdownBase = GenerateUIImage(_name, _parent);
            dictionary.Add("GameObject", dropdownBase["GameObject"]);
            dictionary.Add("RectTransform", dropdownBase["RectTransform"]);
            dictionary.Add("Image", dropdownBase["Image"]);
            var dropdownD = ((GameObject)dropdownBase["GameObject"]).AddComponent<Dropdown>();
            dictionary.Add("Dropdown", dropdownD);

            var label = GenerateUIText("Label", ((GameObject)dropdownBase["GameObject"]).transform);
            ((Text)label["Text"]).color = new Color(0.1961f, 0.1961f, 0.1961f, 1f);
            ((Text)label["Text"]).alignment = TextAnchor.MiddleLeft;

            var arrow = GenerateUIImage("Arrow", ((GameObject)dropdownBase["GameObject"]).transform);
            var arrowImage = (Image)arrow["Image"];
            arrowImage.color = new Color(0.2157f, 0.2157f, 0.2196f, 1f);
            GetImage(arrowImage, RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/editor_gui_left.png");
            ((GameObject)arrow["GameObject"]).transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            SetRectTransform((RectTransform)label["RectTransform"], new Vector2(-15.3f, 0f), Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(-46.6f, 0f));
            SetRectTransform((RectTransform)arrow["RectTransform"], new Vector2(-2f, -0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0f), new Vector2(32f, 32f));

            var template = GenerateUIImage("Template", ((GameObject)dropdownBase["GameObject"]).transform);
            SetRectTransform((RectTransform)template["RectTransform"], new Vector2(0f, 2f), Vector2.right, Vector2.zero, new Vector2(0.5f, 1f), new Vector2(0f, 192f));
            var scrollRect = ((GameObject)template["GameObject"]).AddComponent<ScrollRect>();


            var viewport = GenerateUIImage("Viewport", ((GameObject)template["GameObject"]).transform);
            SetRectTransform((RectTransform)viewport["RectTransform"], Vector2.zero, Vector2.one, Vector2.zero, Vector2.up, Vector2.zero);
            var mask = ((GameObject)viewport["GameObject"]).AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var scrollbar = GenerateUIImage("Scrollbar", ((GameObject)template["GameObject"]).transform);
            SetRectTransform((RectTransform)scrollbar["RectTransform"], Vector2.zero, Vector2.one, Vector2.right, Vector2.one, new Vector2(20f, 0f));
            var ssbar = ((GameObject)scrollbar["GameObject"]).AddComponent<Scrollbar>();

            var slidingArea = new GameObject("Sliding Area");
            slidingArea.transform.SetParent(((GameObject)scrollbar["GameObject"]).transform);
            slidingArea.layer = 5;
            var slidingAreaRT = slidingArea.AddComponent<RectTransform>();
            SetRectTransform(slidingAreaRT, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(-20f, -20f));

            var handle = GenerateUIImage("Handle", slidingArea.transform);
            SetRectTransform((RectTransform)handle["RectTransform"], Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(20f, 20f));
            ((Image)handle["Image"]).color = new Color(0.1216f, 0.1216f, 0.1216f, 1f);

            var content = new GameObject("Content");
            content.transform.SetParent(((GameObject)viewport["GameObject"]).transform);
            content.layer = 5;
            var contentRT = content.AddComponent<RectTransform>();
            SetRectTransform(contentRT, Vector2.zero, Vector2.one, Vector2.up, new Vector2(0.5f, 1f), new Vector2(0f, 32f));

            scrollRect.content = contentRT;
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.vertical = true;
            scrollRect.verticalScrollbar = ssbar;
            scrollRect.viewport = (RectTransform)viewport["RectTransform"];
            ssbar.handleRect = (RectTransform)handle["RectTransform"];
            ssbar.direction = Scrollbar.Direction.BottomToTop;
            ssbar.numberOfSteps = 0;

            var item = new GameObject("Item");
            item.transform.SetParent(content.transform);
            item.layer = 5;
            var itemRT = item.AddComponent<RectTransform>();
            SetRectTransform(itemRT, Vector2.zero, new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 32f));
            var itemToggle = item.AddComponent<Toggle>();

            var itemBackground = GenerateUIImage("Item Background", item.transform);
            SetRectTransform((RectTransform)itemBackground["RectTransform"], Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero);
            ((Image)itemBackground["Image"]).color = new Color(0.9608f, 0.9608f, 0.9608f, 1f);

            var itemCheckmark = GenerateUIImage("Item Checkmark", item.transform);
            SetRectTransform((RectTransform)itemCheckmark["RectTransform"], new Vector2(8f, 0f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(32f, 32f));
            var itemCheckImage = (Image)itemCheckmark["Image"];
            itemCheckImage.color = new Color(0.1216f, 0.1216f, 0.1216f, 1f);
            GetImage(itemCheckImage, RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/editor_gui_diamond.png");

            var itemLabel = GenerateUIText("Item Label", item.transform);
            SetRectTransform((RectTransform)itemLabel["RectTransform"], new Vector2(15f, 0.5f), Vector2.one, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(-50f, -3f));
            var itemLabelText = (Text)itemLabel["Text"];
            itemLabelText.alignment = TextAnchor.MiddleLeft;
            itemLabelText.font = inconsolataFont;
            itemLabelText.horizontalOverflow = HorizontalWrapMode.Wrap;
            itemLabelText.verticalOverflow = VerticalWrapMode.Truncate;
            itemLabelText.text = "Option A";
            itemLabelText.color = new Color(0.1961f, 0.1961f, 0.1961f, 1f);

            itemToggle.image = (Image)itemBackground["Image"];
            itemToggle.targetGraphic = (Image)itemBackground["Image"];
            itemToggle.graphic = itemCheckImage;

            dropdownD.captionText = (Text)label["Text"];
            dropdownD.itemText = itemLabelText;
            dropdownD.alphaFadeSpeed = 0.15f;
            dropdownD.template = (RectTransform)template["RectTransform"];
            ((GameObject)template["GameObject"]).SetActive(false);

            return dictionary;
        }

        public static ColorBlock SetColorBlock(ColorBlock cb, Color normal, Color highlighted, Color pressed, Color selected, Color disabled, float fade = 0.2f)
        {
            cb.normalColor = normal;
            cb.highlightedColor = highlighted;
            cb.pressedColor = pressed;
            cb.selectedColor = selected;
            cb.disabledColor = disabled;
            cb.fadeDuration = fade;
            return cb;
        }

        public static void SetLayoutGroup(HorizontalLayoutGroup layoutGroup, bool controlHeight, bool controlWidth, bool expandHeight, bool expandWidth, bool scaleHeight = false, bool scaleWidth = false)
        {
            layoutGroup.childControlHeight = controlHeight;
            layoutGroup.childControlWidth = controlWidth;
            layoutGroup.childForceExpandHeight = expandHeight;
            layoutGroup.childForceExpandWidth = expandWidth;
            layoutGroup.childScaleHeight = scaleHeight;
            layoutGroup.childScaleWidth = scaleWidth;
        }

        public static void SetLayoutGroup(VerticalLayoutGroup layoutGroup, bool controlHeight, bool controlWidth, bool expandHeight, bool expandWidth, bool scaleHeight = false, bool scaleWidth = false)
        {
            layoutGroup.childControlHeight = controlHeight;
            layoutGroup.childControlWidth = controlWidth;
            layoutGroup.childForceExpandHeight = expandHeight;
            layoutGroup.childForceExpandWidth = expandWidth;
            layoutGroup.childScaleHeight = scaleHeight;
            layoutGroup.childScaleWidth = scaleWidth;
        }

        public static void GetImage(Image _image, string _filePath)
        {
            if (RTFile.FileExists(_filePath))
            {
                DataManager.inst.StartCoroutine(GetSprite(_filePath, new Vector2(), delegate (Sprite cover)
                {
                    _image.sprite = cover;
                }, delegate (string errorFile)
                {
                    _image.sprite = ArcadeManager.inst.defaultImage;
                }));
            }
        }
        public static IEnumerator GetSprite(string _path, Vector2 _limits, Action<Sprite> callback, Action<string> onError, TextureFormat _textureFormat = TextureFormat.ARGB32)
        {
            yield return DataManager.inst.StartCoroutine(LoadImageFileRaw(_path, delegate (Sprite _texture)
            {
                if (((float)_texture.texture.width > _limits.x && _limits.x > 0f) || ((float)_texture.texture.height > _limits.y && _limits.y > 0f))
                {
                    onError(_path);
                    return;
                }
                callback(_texture);
            }, delegate (string error)
            {
                onError(_path);
            }, _textureFormat));
            yield break;
        }

        public static IEnumerator LoadImageFileRaw(string _filepath, Action<Sprite> callback, Action<string> onError, TextureFormat _textureFormat = TextureFormat.ARGB32)
        {
            if (!File.Exists(_filepath))
            {
                onError(_filepath);
            }
            else
            {
                Texture2D tex = new Texture2D(256, 256, _textureFormat, false);
                tex.requestedMipmapLevel = 3;
                Sprite sprite;
                using (WWW www = new WWW("file://" + _filepath))
                {
                    while (!www.isDone)
                        yield return (object)null;
                    www.LoadImageIntoTexture(tex);
                    tex.Apply(true);
                    sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, (float)tex.width, (float)tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
                callback(sprite);
                tex = (Texture2D)null;
            }
        }
    }
}
