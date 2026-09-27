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
                    result.SetWall(x, y, source.GetWall(x, y));
                    var figure = source.GetFigure(x, y);
                    if (figure != null)
                    {
                        if (figure.PlayerId == playerId || playerId == -1)
                        {
                            result.SetFigure(x, y, figure.Clone());
                        }
                        else
                        {
                            result.SetFigure(x, y, ((KaNoBuFigure)figure).WithFigureType(KaNoBuFigure.FigureTypes.Unknown));
                        }
                    }
                }
            }
            return result;
        }
    }
}