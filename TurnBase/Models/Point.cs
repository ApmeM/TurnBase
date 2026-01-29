namespace TurnBase
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        public override string ToString()
        {
            return $"({(char)('A' + this.X)}{this.Y})";
        }
    }
}