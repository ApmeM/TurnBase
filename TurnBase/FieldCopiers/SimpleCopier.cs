namespace TurnBase
{
    public class SimpleCopier: IFieldCopier<Field2D>
    {
        public Field2D CopyForPlayer(Field2D source, int playerId)
        {
            var result = Field2D.Create(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    result.SetWall(x, y, source.GetWall(x, y));
                    result.SetFigure(x, y, source.GetFigure(x, y)?.Clone());
                }
            }
            return result;
        }
    }
}