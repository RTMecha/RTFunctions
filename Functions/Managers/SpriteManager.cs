using System;
using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Managers
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager inst;

        void Awake() => inst = this;

        public static void GetSprite(string _path, Image _image, TextureFormat _textureFormat = TextureFormat.ARGB32)
        {
            inst.StartCoroutine(GetSprite(_path, Vector2.zero, delegate (Sprite sprite)
            {
                _image.sprite = sprite;
            }, delegate (string onError)
            {
                _image.sprite = ArcadeManager.inst.defaultImage;
            }, _textureFormat));
        }
        
        public static Sprite GetSprite(string _path, TextureFormat _textureFormat = TextureFormat.ARGB32)
        {
            Sprite result = null;
            inst.StartCoroutine(GetSprite(_path, Vector2.zero, delegate (Sprite sprite)
            {
                result = sprite;
            }, delegate (string onError)
            {
                result = ArcadeManager.inst.defaultImage;
            }, _textureFormat));
            return result;
        }

        public static IEnumerator GetSprite(string _path, Vector2 _limits, Action<Sprite> callback, Action<string> onError, TextureFormat _textureFormat = TextureFormat.ARGB32)
        {
            yield return inst.StartCoroutine(LoadImageFileRaw(_path, delegate (Sprite _texture)
            {
                if (((float)_texture.texture.width > _limits.x && _limits.x > 0f) || ((float)_texture.texture.height > _limits.y && _limits.y > 0f))
                {
                    if (onError != null)
                        onError(_path);
                    return;
                }
                callback(_texture);
            }, delegate (string error)
            {
                if (onError != null)
                    onError(_path);
            }, _textureFormat));
            yield break;
        }

        public static IEnumerator LoadImageFileRaw(string _filepath, Action<Sprite> callback, Action<string> onError, TextureFormat _textureFormat = TextureFormat.ARGB32)
        {
            if (!RTFile.FileExists(_filepath))
            {
                if (onError != null)
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

        public static IEnumerator DownloadSprite(string path, Vector2Int textureSize, TextureFormat textureFormat = TextureFormat.RGBA32, bool mipChain = false, Action<Sprite> callback = null)
        {
            Texture2D tex = new Texture2D(textureSize.x, textureSize.y, textureFormat, mipChain);
            tex.requestedMipmapLevel = 3;
            Sprite sprite;
            using (var www = new WWW(path))
            {
                while (!www.isDone)
                    yield return (object)null;
                www.LoadImageIntoTexture(tex);
                tex.Apply(true);
                sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, (float)tex.width, (float)tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            if (callback != null)
                callback(sprite);

            tex = null;

            yield break;
        }

        public static IEnumerator LoadImageSprite(string path, Vector2Int textureSize, TextureFormat textureFormat = TextureFormat.RGBA32, bool mipChain = false, Action<Sprite> callback = null, Action<string> onError = null)
        {
            yield return inst.StartCoroutine(LoadImageFileBytes(path, textureSize, textureFormat, mipChain, delegate (Texture2D tex)
            {
                callback(Sprite.Create(tex, new Rect(0f, 0f, textureSize.x, textureSize.y), new Vector2(0.5f, 0.5f), 100f));
            }, onError));
        }

        public static IEnumerator LoadImageFileBytes(string path, Vector2Int textureSize, TextureFormat textureFormat = TextureFormat.RGBA32, bool mipChain = false, Action<Texture2D> callback = null, Action<string> onError = null)
        {
            if (!RTFile.FileExists(path) || textureSize.x <= 0 || textureSize.y <= 0)
            {
                if (onError != null)
                    onError(path);
            }
            else
            {
                var bytes = File.ReadAllBytes(path);
                var texture2d = new Texture2D(textureSize.x, textureSize.y, textureFormat, mipChain);
                texture2d.LoadImage(bytes);

                texture2d.wrapMode = TextureWrapMode.Clamp;
                texture2d.filterMode = FilterMode.Point;
                texture2d.Apply();

                callback(texture2d);
            }
            yield break;
        }

        public static IEnumerator LoadImageFileBytes(byte[] bytes, Vector2Int textureSize, TextureFormat textureFormat = TextureFormat.RGBA32, bool mipChain = false, Action<Texture2D> callback = null, Action<string> onError = null)
        {
            if (bytes == null || bytes.Length < 1 || textureSize.x <= 0 || textureSize.y <= 0)
            {
                if (onError != null)
                    onError("");
            }
            else
            {
                var texture2d = new Texture2D(textureSize.x, textureSize.y, textureFormat, mipChain);
                texture2d.LoadImage(bytes);

                texture2d.wrapMode = TextureWrapMode.Clamp;
                texture2d.filterMode = FilterMode.Point;
                texture2d.Apply();

                callback(texture2d);
            }

            yield break;
        }

        public static Sprite LoadSprite(string path, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = false, TextureWrapMode textureWrapMode = TextureWrapMode.Clamp, FilterMode filterMode = FilterMode.Point)
        {
            var texture2d = new Texture2D(2, 2, textureFormat, mipChain);
            var bytes = File.ReadAllBytes(path);
            texture2d.LoadImage(bytes);

            texture2d.wrapMode = textureWrapMode;
            texture2d.filterMode = filterMode;
            texture2d.Apply();

            return CreateSprite(texture2d);
        }

        public static void SaveSprite(Sprite sprite, string path) => File.WriteAllBytes(path, sprite.texture.EncodeToPNG());

        public static Sprite CreateSprite(Texture2D texture2D) => Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
    }
}
