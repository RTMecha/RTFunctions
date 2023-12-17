namespace RTFunctions.Functions
{
    public class Exists
    {
        public static implicit operator bool(Exists exists) => exists != null;
    }
}
