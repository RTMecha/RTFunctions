using UnityEngine;

namespace RTFunctions.Functions.Optimization.Objects.Visual
{
    /// <summary>
    /// Class for regular shape objects.
    /// </summary>
    public class SolidObject : VisualObject
    {
        public override GameObject GameObject { get; set; }
        public override Transform Top { get; set; }
        public override Renderer Renderer { get; set; }
        public override Collider2D Collider { get; set; }

        readonly Material material;
        readonly float opacity;

        public SolidObject(GameObject gameObject, Transform top, float opacity, bool hasCollider, bool solid = false)
        {
            GameObject = gameObject;
            Top = top;

            this.opacity = opacity;

            Renderer = gameObject.GetComponent<Renderer>();
            Renderer.enabled = true;
            material = Renderer.material;

            Collider = gameObject.GetComponent<Collider2D>();

            if (Collider != null)
            {
                Collider.enabled = true;
                if (hasCollider)
                    Collider.tag = "Helper";
                if (solid)
                    Collider.isTrigger = false;
            }
        }

        public override void SetColor(Color color) => material?.SetColor(new Color(color.r, color.g, color.b, color.a * opacity));
    }
}
