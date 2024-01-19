using System;

using UnityEngine;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Managers.Networking;

namespace RTFunctions.Functions.Optimization.Objects.Visual
{
    /// <summary>
    /// Class for special image objects.
    /// </summary>
    public class ImageObject : VisualObject
    {
        public override GameObject GameObject { get; set; }
        public override Transform Top { get; set; }
        public override Renderer Renderer { get; set; }
        public override Collider2D Collider { get; set; }

        readonly Material material;
        readonly float opacity;

        public string Path { get; set; }

        public ImageObject(GameObject gameObject, Transform top, float opacity, string text, bool background)
        {
            GameObject = gameObject;
            Top = top;
            this.opacity = opacity;

            if (GameObject.TryGetComponent(out Renderer renderer))
                Renderer = renderer;

            if (background)
            {
                GameObject.layer = 9;
                //Renderer.material = GameStorageManager.inst.bgMaterial;
            }

            if (Renderer)
                material = Renderer.material;

            var local = GameObject.transform.localPosition;

            var regex = new System.Text.RegularExpressions.Regex(@"img\((.*?)\)");
            var match = regex.Match(text);

            Path = match.Success ? RTFile.BasePath + match.Groups[1].ToString() : RTFile.BasePath + text;

            if (RTFile.FileExists(Path))
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadImageTexture("file://" + Path, delegate (Texture2D x)
                {
                    ((SpriteRenderer)Renderer).sprite = SpriteManager.CreateSprite(x);
                    GameObject.transform.localPosition = local;
                    GameObject.transform.localPosition = local;
                    GameObject.transform.localPosition = local;
                }, delegate (string onError)
                {
                    ((SpriteRenderer)Renderer).sprite = ArcadeManager.inst.defaultImage;
                }));
            //{
            //    ((SpriteRenderer)Renderer).sprite = SpriteManager.LoadSprite(Path);
            //    GameObject.transform.localPosition = local;
            //    GameObject.transform.localPosition = local;
            //    GameObject.transform.localPosition = local;
            //}
            else ((SpriteRenderer)Renderer).sprite = ArcadeManager.inst.defaultImage;
        }

        public override void SetColor(Color color) => material?.SetColor(new Color(color.r, color.g, color.b, color.a * opacity));
    }
}
