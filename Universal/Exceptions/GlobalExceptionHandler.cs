using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Universal.IO.FastConsole;

namespace Universal.Exceptions
{
    public static class GlobalExceptionHandler
    {
        public static void Setup()
        {
            TaskScheduler.UnobservedTaskException += (_, exception) =>
            {
                FConsole.WriteLine($"Congrats you idiot. Look what you did: {exception.Exception.Message}");
                FConsole.WriteLine($"Congrats you idiot. Look what you did: {exception.Exception.StackTrace}");
                exception.SetObserved();
                Debugger.Break();
            };
            AppDomain.CurrentDomain.UnhandledException += (_, exception) =>
            {
                FConsole.WriteLine($"Congrats you idiot. Look what you did: {(exception.ExceptionObject as Exception)?.Message}");
                FConsole.WriteLine($"Congrats you idiot. Look what you did: {(exception.ExceptionObject as Exception)?.StackTrace}");
                Debugger.Break();
            };
        }
    }
}