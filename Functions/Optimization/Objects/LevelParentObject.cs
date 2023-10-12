﻿using UnityEngine;

using RTFunctions.Functions.Animation;

namespace RTFunctions.Functions.Optimization.Objects
{
    public class LevelParentObject
    {
        public Sequence<Vector2> PositionSequence { get; set; }
        public Sequence<Vector3> Position3DSequence { get; set; }
        public Sequence<Vector2> ScaleSequence { get; set; }
        public Sequence<float> RotationSequence { get; set; }

        public float TimeOffset { get; set; }

        public bool ParentAnimatePosition { get; set; }
        public bool ParentAnimateScale { get; set; }
        public bool ParentAnimateRotation { get; set; }

        public float ParentOffsetPosition { get; set; }
        public float ParentOffsetScale { get; set; }
        public float ParentOffsetRotation { get; set; }

        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }
    }
}