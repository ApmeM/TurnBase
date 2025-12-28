namespace TurnBase.Core
{
    public struct Point
    {
        public int X;
        public int Y;

        override public string ToString()
        {
            return $"{X}x{Y}";
        }
    }
}