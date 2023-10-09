namespace RTFunctions.Functions.Optimization.Objects

/// <summary>
/// Represents a level object.
/// </summary>
{
    public interface ILevelObject
    {
        public float StartTime { get; }
        public float KillTime { get; }

        public string ID { get; }

        public void SetActive(bool active);

        /// <param name="time">Seconds since the level has started.</param>
        public void Interpolate(float time);
    }
}