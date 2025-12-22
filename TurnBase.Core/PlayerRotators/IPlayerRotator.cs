namespace TurnBase.Core;

public interface IPlayerRotator
{
    public void moveNext();

    public int getCurrent();

    public int size { get; set; }
}
