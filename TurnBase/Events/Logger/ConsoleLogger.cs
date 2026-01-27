using System;

namespace TurnBase
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}