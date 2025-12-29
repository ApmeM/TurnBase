namespace TurnBase
{
    public class UnknownFigure : IFigure
    {
        public UnknownFigure(int playerId)
        {
            PlayerId = playerId;
        }

        public int PlayerId { get; }
    }
}