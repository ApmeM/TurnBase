using Godot;
using TurnBase;

public class GDLogger : ILogger
{
    public void Log(string message)
    {
        GD.Print(message);
    }
}