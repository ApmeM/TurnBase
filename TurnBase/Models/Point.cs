using System;

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

        public bool IsAdjacentTo(Point other)
        {
            return (this.X != other.X || this.Y != other.Y) &&
                   ((this.X == other.X && this.Y <= other.Y + 1 && this.Y >= other.Y - 1) ||
                    (this.Y == other.Y && this.X <= other.X + 1 && this.X >= other.X - 1));
        }


        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public int ManhattanDistanceTo(Point second)
        {
            var first = this;
            return Math.Abs(first.X - second.X) + Math.Abs(first.Y - second.Y);
        }

        public override string ToString()
        {
            return $"({(char)('A' + this.X)}{this.Y})";
        }
    }
}