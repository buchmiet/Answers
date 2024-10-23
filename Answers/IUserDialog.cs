
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Answers
{
    public interface IUserDialog
    {
        Task<bool> YesNoAsync(string errorMessage, CancellationToken ct);
        Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken ct);
        bool YesNo(string errorMessage);
        bool ContinueTimedOutYesNo(string errorMessage);

        bool IsAsyncAvailable { get; }
        bool IsSyncAvailable { get; }
    }

}
