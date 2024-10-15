using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    public class RandomService
    {
        private readonly Random _random;

        ILogger _logger;
        public RandomService(ILogger logger, int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            _logger = logger;
        }

        public bool NextBool()
        {
            var value = _random.NextDouble() >= 0.5;
            _logger.LogInformation($"Generated random boolean: {value}");
            return value;
        }

        public int NextInt(int maxValue)
        {
            var value = _random.Next(maxValue);
            _logger.LogInformation($"Generated random integer: {value}");
            return value;
        }
    }
}
