namespace TurnBase.Core;

public class PlayerRotatorNormal : IPlayerRotator
{
    public int size { get; set; }
    private int currentPlayer = 0;

    public void moveNext()
    {
        this.currentPlayer++;
        this.currentPlayer = this.currentPlayer % size;
    }

    public int getCurrent()
    {
        return this.currentPlayer;
    }
}