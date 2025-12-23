namespace TurnBase.Core;

public class PlayerRotatorNormal : IPlayerRotator
{
    public int Size { get; set; }
    private int currentPlayer = 0;

    public void MoveNext()
    {
        this.currentPlayer++;
        this.currentPlayer = this.currentPlayer % Size;
    }

    public int GetCurrent()
    {
        return this.currentPlayer;
    }
}