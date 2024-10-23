using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    public class FixedValueProvider<T> : IValueProvider<T>
    {
        private readonly T _value;

        public FixedValueProvider(T value)
        {
            _value = value;
        }

        public T Next()
        {
            return _value;
        }
    }

}
