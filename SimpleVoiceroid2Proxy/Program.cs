using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleVoiceroid2Proxy
{
    public static class Program
    {
        static Program()
        {
            KillDuplicatedProcesses();
        }

        public static readonly ILogger Logger = new LoggerImpl();
        public static readonly HttpServer Server = new();
        private static readonly KeyEvent KeyEvent = new();
        public static readonly AIVoiceroidEngine AIVoiceroidEngine = new();

        public static void Main()
        {
            Task.Run(() => KeyEvent.ConsumeAsync());
            Task.Run(async () =>
            {
                Program.AIVoiceroidEngine.PrintHelp();
                await Program.AIVoiceroidEngine.TalkAsync("準備完了！");

                await Server.ConsumeAsync();

            }).Wait();


    }

        private static void KillDuplicatedProcesses()
        {
            var currentProcess = Process.GetCurrentProcess();
            var imageName = Assembly.GetExecutingAssembly()
                .Location
                .Split(Path.DirectorySeparatorChar)
                .Last()
                .Replace(".exe", "");

            foreach (var process in Process.GetProcessesByName(imageName).Where(x => x.Id != currentProcess.Id))
            {
                try
                {
                    process.Kill();
                    Logger.Info($"{imageName}.exe (PID: {process.Id}) has been killed.");
                }
                catch
                {
                    Logger.Warn($"Failed to kill {imageName}.exe (PID: {process.Id}).");
                }
            }
        }

        private class LoggerImpl : ILogger
        {
            public void Break()
            {
                Console.WriteLine("");
            }
            public void Info(string message)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Write("INFO", message);
            }
            public void System(string message)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Write("SYSM", message);
            }
            public void  Play(string message)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Write("PLAY", message);
            }
            public void Busy(string message)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Write("BUSY", message);
            }
            public void Warn(string message)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Write("WARN", message);
            }

            public void Error(Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Write("EROR", exception.ToString());
            }

            private static void Write(string level, string message)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
                Console.ResetColor();
            }
        }
    }

    public interface ILogger
    {
        public void Break();
        public void System(string message);
        public void Info(string message);
        public void Play(string message);
        public void Busy(string message);
        public void Warn(string message);
        public void Error(Exception exception);
    }
}
