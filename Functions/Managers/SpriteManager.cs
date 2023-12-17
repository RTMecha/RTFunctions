using System.IO;

using UnityEngine;

namespace RTFunctions.Functions.Managers
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager inst;

        void Awake() => inst = this;

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
