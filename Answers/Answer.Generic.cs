using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers
{
    public class Answer<T> : Answer
    {
        private T _value;

        public Answer<T> WithValue(T value)
        {
            _value = value;
            State.HasValueSet = true;
            return this;
        }

        public T GetValue()
        {
            if (State.HasValueSet)
            {
                return _value;
            }
            throw new InvalidOperationException($"Expected a value of type {typeof(T)}.");
        }

        public bool Out(out T value)
        {
            value = _value;
            return IsSuccess;
        }
    }
}
