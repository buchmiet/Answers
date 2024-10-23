using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    public class SequenceValueProvider<T> : IValueProvider<T>
    {
        private readonly IList<T> _values;
        private int _currentIndex = 0;

        public SequenceValueProvider(IEnumerable<T> values)
        {
            _values = values.ToList();
            if (_values.Count == 0)
                throw new ArgumentException("Kolekcja wartości nie może być pusta.");
        }

        public T Next()
        {
            var value = _values[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _values.Count;
            return value;
        }
    }

}
