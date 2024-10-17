using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{
    public  partial class TestClassForTryAsync:IAnswerable
    {

        public async Task<Answer> MethodReturningAnswer(Func<Task<Answers.Answer>> method,
            CancellationToken ct,
            TimeSpan? timeout = null)
        {

            return await TryAsync(method, ct, timeout);
        }


    }
}
