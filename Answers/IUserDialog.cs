
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Answers
{
    public interface IUserDialog
    {
        Task<bool> YesNoAsync(string errorMessage, CancellationToken ct);
        Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken ct);

    }
}
