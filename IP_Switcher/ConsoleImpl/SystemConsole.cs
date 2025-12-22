using System;

namespace IP_Switcher
{
    public class SystemConsole : IConsole
    {
        public void Clear() => Console.Clear();
        public void WriteLine(string message) => Console.WriteLine(message);
        public void Write(string message) => Console.Write(message);
        public string ReadLine() => Console.ReadLine();
        public ConsoleKeyInfo ReadKey() => Console.ReadKey();
    }
}
