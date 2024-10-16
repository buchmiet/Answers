using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers.Tests
{
    public partial class Tier1 : IAnswerable
    {
        private readonly Tier2 _tier2;

        public Tier1(Tier2 tier2)
        {
            _tier2 = tier2;
        }

        public async Task<Answer> DoOperationAsync()
        {
            return await TryAsync(async () =>
            {
                var answer = await _tier2.DoOperationAsync();
                if (!answer.IsSuccess)
                {
                    return Answer.Prepare("Tier1 operation").Attach(answer);
                }
                return answer;
            }, CancellationToken.None);
        }

    }

    public partial class Tier2 : IAnswerable
    {
        private readonly Tier3 _tier3;

        public Tier2(Tier3 tier3)
        {
            _tier3 = tier3;
        }

        public async Task<Answer> DoOperationAsync()
        {
            return await TryAsync(async () =>
            {
                var answer = await _tier3.DoOperationAsync();
                if (!answer.IsSuccess)
                {
                    return Answer.Prepare("Tier2 operation").Attach(answer);
                }
                return answer;
            }, CancellationToken.None);
        }
    }

    public partial class Tier3 : IAnswerable
    {
        private readonly Tier4 _tier4;

        public Tier3(Tier4 tier4)
        {
            _tier4 = tier4;
        }

        public async Task<Answer> DoOperationAsync()
        {
            return await TryAsync(async () =>
            {
                var answer = await _tier4.DoOperationAsync();
                if (!answer.IsSuccess)
                {
                    return Answer.Prepare("Tier3 operation").Attach(answer);
                }
                return answer;
            }, CancellationToken.None);
        }


    }

    public partial class Tier4 : IAnswerable
    {


        public async Task<Answer> DoOperationAsync()
        {
            return await TryAsync(async () =>
            {
                // Simulate failure
                var answer = Answer.Prepare("Tier4 operation").Error("Tier4 operation failed");
                return answer;
            }, CancellationToken.None);
        }

    }
}
