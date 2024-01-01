using TMPro;
using UnityEngine;

namespace RTFunctions.Functions.Optimization.Objects.Visual
{
    public class TextObject : VisualObject
    {
        public override GameObject GameObject { get; set; }
        public override Transform Top { get; set; }
        public override Renderer Renderer { get; set; }
        public override Collider2D Collider { get; set; }

        public readonly TextMeshPro TextMeshPro;
        readonly float opacity;

        public TextObject(GameObject gameObject, Transform top, float opacity, string text)
        {
            GameObject = gameObject;
            Top = top;
            this.opacity = opacity;

            if (GameObject.TryGetComponent(out Renderer renderer))
                Renderer = renderer;

            TextMeshPro = gameObject.GetComponent<TextMeshPro>();
            TextMeshPro.enabled = true;
            TextMeshPro.text = text;
        }

        public override void SetColor(Color color) => TextMeshPro.color = new Color(color.r, color.g, color.b, color.a * opacity);
    }
}
