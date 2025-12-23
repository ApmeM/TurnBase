namespace TurnBase.Core;

public interface IPlayerRotator
{
    public void MoveNext();

    public int GetCurrent();

    public int Size { get; set; }
}
