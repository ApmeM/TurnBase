namespace TurnBase
{
    public interface IFigure
    {
        int PlayerId { get; }

        IFigure CopyForPlayer(int playerId);
    }
}