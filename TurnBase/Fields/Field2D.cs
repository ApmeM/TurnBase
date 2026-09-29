using System.Data.Common;
using Newtonsoft.Json;

namespace TurnBase
{

    public class Field2D : IField2D
    {
        [JsonProperty]
        private IFigure[,] realField;
        [JsonProperty]
        private bool[,] walls;

        [JsonIgnore]
        public int Width => this.realField.GetLength(0);
        [JsonIgnore]
        public int Height => this.realField.GetLength(1);

        public static Field2D Create(int width, int height)
        {
            return new Field2D(new IFigure[width, height], new bool[width, height]);
        }

        public Field2D(IFigure[,] realField, bool[,] walls)
        {
            this.realField = realField;
            this.walls = walls;
        }

        public IFigure GetFigure(Point key)
        {
            return this.realField[key.X, key.Y];
        }

        public void SetFigure(Point key, IFigure value)
        {
            this.realField[key.X, key.Y] = value;
        }

        public IFigure GetFigure(int x, int y)
        {
            return this.realField[x, y];
        }

        public void SetFigure(int x, int y, IFigure value)
        {
            this.realField[x, y] = value;
        }

        public bool GetWall(Point key)
        {
            return this.walls[key.X, key.Y];
        }

        public void SetWall(Point key, bool value)
        {
            this.walls[key.X, key.Y] = value;
        }

        public bool GetWall(int x, int y)
        {
            return this.walls[x, y];
        }

        public void SetWall(int x, int y, bool value)
        {
            this.walls[x, y] = value;
        }

        public bool IsInBounds(Point point)
        {
            return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
        }

        public Point RotatePointFor2Players(Point point, int playerNumber)
        {
            if (playerNumber > 1)
            {
                return default;
            }

            playerNumber = playerNumber % 2;

            int mainWidth = this.Width - 1;
            int mainHeight = this.Height - 1;

            int x = playerNumber * mainWidth - (playerNumber * 2 - 1) * point.X;
            int y = playerNumber * mainHeight - (playerNumber * 2 - 1) * point.Y;

            return new Point { X = x, Y = y };
        }

        public override string ToString()
        {
            string result = "";
            result += string.Format("    ");
            for (int j = 0; j < this.Width; j++)
            {
                result += $"  {(char)('A' + j)}";
            }
            result += string.Format("   ");
            result += "\n";

            for (int i = 0; i < this.Height; i++)
            {
                result += $"  {i,2}";
                for (int j = 0; j < this.Width; j++)
                {
                    if (this.walls[j, i])
                    {
                        result += $" ##";
                    }
                    else
                    {
                        var ship = this.realField[j, i];
                        result += $" {ship?.ToString() ?? "  "}";
                    }
                }

                result += $"   {i,2}\n";
            }

            result += string.Format("    ");
            for (int j = 0; j < this.Width; j++)
            {
                result += $"  {(char)('A' + j)}";
            }
            result += string.Format("    ");

            return result;
        }
    }
}