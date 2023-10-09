using UnityEngine;

namespace RTFunctions.Functions.Optimization.Objects.Visual
{
    public abstract class VisualObject
    {
        public abstract GameObject GameObject { get; set; }

        public abstract Renderer Renderer { get; set; }

        public abstract Collider2D Collider { get; set; }

        public abstract void SetColor(Color color);
    }
}
