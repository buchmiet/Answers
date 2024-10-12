using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers
{
    public interface IUserDialog
    {
        Task<bool> YesNoAsync(string errorMessage, CancellationToken ct);

    }
}
