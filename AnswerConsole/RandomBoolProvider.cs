using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    public class RandomBoolProvider : IValueProvider<bool>
    {
        private readonly Random _random;

        public RandomBoolProvider(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public bool Next()
        {
            return _random.NextDouble() >= 0.5;
        }
    }

}
