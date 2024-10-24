
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Answers
{
    public interface IUserDialog
    {
        Task<bool> YesNoAsync(string errorMessage, CancellationToken ct);
        Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct);
        bool YesNo(string errorMessage);
        bool ContinueTimedOutYesNo(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct);

        bool IsAsyncAvailable { get; }
        bool IsSyncAvailable { get; }
    }

}
