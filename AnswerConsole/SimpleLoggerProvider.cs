using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    public class SimpleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new SimpleLogger(categoryName);
        }

        public void Dispose()
        {
            // Jeśli potrzebna jest jakaś logika czyszczenia, można ją tutaj dodać
        }
    }
}
