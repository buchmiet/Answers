using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    public class SimpleLogger : ILogger
    {
        private readonly string _name;

        public SimpleLogger(string name)
        {
            _name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Nie obsługujemy zasięgu (scopes), zwracamy null
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Zakładamy, że wszystkie poziomy są włączone.
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Zapisywanie aktualnych ustawień koloru
            var originalForeground = Console.ForegroundColor;
            var originalBackground = Console.BackgroundColor;

            // Ustawienie kolorów na podstawie poziomu logowania
            switch (logLevel)
            {
                case LogLevel.Information:
                    Console.ResetColor(); // Domyślne kolory konsoli
                    Console.Write("[Info] ");
                    break;
                case LogLevel.Warning:
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("[Warning] ");
                    break;
                case LogLevel.Error:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[Error] ");
                    break;
                default:
                    Console.ResetColor(); // Inne poziomy logowania z domyślnymi kolorami
                    Console.Write($"[{logLevel}] ");
                    break;
            }

            // Przywrócenie standardowych kolorów po wypisaniu etykiety
            Console.BackgroundColor = originalBackground;
            Console.ForegroundColor = originalForeground;

            // Wypisywanie sformatowanej wiadomości
            string message = formatter(state, exception);
            Console.WriteLine($"{message}");

            // Jeśli istnieje wyjątek, także go wypisz
            if (exception != null)
            {
                Console.WriteLine(exception.ToString());
            }
        }
    }
}
