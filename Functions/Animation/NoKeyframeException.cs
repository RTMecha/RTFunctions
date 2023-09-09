using System;

namespace RTFunctions.Functions.Animation
{
    public class NoKeyframeException : Exception
    {
        public NoKeyframeException(string message) : base(message)
        {
        }
    }
}
