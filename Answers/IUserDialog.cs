
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Answers
{
    public interface IUserDialog
    {
        Task<bool> YesNoAsync(string errorMessage, CancellationToken ct);
        Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct);
        bool YesNo(string errorMessage, CancellationToken ct);
        bool ContinueTimedOutYesNo(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct);

        bool HasAsyncYesNo { get; }
        bool HasAsyncTimeoutDialog { get; }
        bool HasYesNo { get; }
        bool HasTimeoutDialog { get; }
    }

}
