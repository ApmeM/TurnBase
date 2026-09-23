namespace TurnBase.KaNoBu
{
    public class HideEnemyCopier : IFieldCopier<Field2D>
    {
        public Field2D CopyForPlayer(Field2D source, int playerId)
        {
            var result = Field2D.Create(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    result.walls[x, y] = source.walls[x, y];
                    var figure = source.realField[x, y];
                    if (figure != null)
                    {
                        if (figure.PlayerId == playerId || playerId == -1)
                        {
                            result.realField[x, y] = figure.Clone();
                        }
                        else
                        {
                            result.realField[x, y] = ((KaNoBuFigure)figure).WithFigureType(KaNoBuFigure.FigureTypes.Unknown);
                        }
                    }
                }
            }
            return result;
        }
    }
}