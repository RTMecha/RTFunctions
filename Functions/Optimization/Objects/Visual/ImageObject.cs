using System;

using UnityEngine;

using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Managers.Networking;

namespace RTFunctions.Functions.Optimization.Objects.Visual
{
    public class ImageObject : VisualObject
    {
        public override GameObject GameObject { get; set; }
        public override Renderer Renderer { get; set; }
        public override Collider2D Collider { get; set; }

        readonly Material material;
        readonly float opacity;

        public ImageObject(GameObject gameObject, float opacity, string text)
        {
            GameObject = gameObject;
            this.opacity = opacity;

            if (GameObject.TryGetComponent(out Renderer renderer))
                Renderer = renderer;

            if (Renderer)
                material = Renderer.material;

            var regex = new System.Text.RegularExpressions.Regex(@"img\((.*?)\)");
            var match = regex.Match(text);

            string imagePath;

            if (match.Success)
                imagePath = RTFile.ApplicationDirectory + RTFile.basePath + match.Groups[1].ToString();
            else
                imagePath = RTFile.ApplicationDirectory + RTFile.basePath + text;

            if (RTFile.FileExists(imagePath))
                FunctionsPlugin.inst.StartCoroutine(AlephNetworkManager.DownloadImageTexture("file://" + imagePath, delegate (Texture2D x)
                {
                    ((SpriteRenderer)Renderer).sprite = RTSpriteManager.CreateSprite(x);
                }, delegate (string onError)
                {
                    ((SpriteRenderer)Renderer).sprite = ArcadeManager.inst.defaultImage;
                }));
            else ((SpriteRenderer)Renderer).sprite = ArcadeManager.inst.defaultImage;
        }

        public override void SetColor(Color color)
        {
            //if (material)
            //    material.color = new Color(color.r, color.g, color.b, color.a * opacity);

            material?.SetColor(new Color(color.r, color.g, color.b, color.a * opacity));
        }
    }
}
