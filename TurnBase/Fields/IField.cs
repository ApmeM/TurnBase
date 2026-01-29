namespace TurnBase
{
    public interface IField
    {
        IField copyForPlayer(int playerNumber = -1);
    }
}