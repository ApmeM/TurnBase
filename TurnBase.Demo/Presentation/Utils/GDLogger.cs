using Godot;

namespace TurnBase.Demo
{
    public class GDLogger: ILogger
    {
        public void Log(string message)
        {
            GD.Print(message);
        }
    }
}