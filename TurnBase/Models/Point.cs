namespace TurnBase
{
    public struct Point
    {
        public int X;
        public int Y;

        public override string ToString()
        {
            return $"{X}x{Y}";
        }

        public string PrintableName()
        {
            return $"({(char)('A' + this.X)}{this.Y})";
        }
    }
}