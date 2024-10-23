using Answers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnswerConsole
{
    //public class ConsoleUserDialog : IUserDialog
    //{
    //    public bool ContinueTimedOutYesNo(string errorMessage)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken ct)
    //    {
    //        Console.WriteLine(errorMessage);
    //        return Task.FromResult(false);
    //    }

    //    public bool YesNo(string errorMessage)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public async Task<bool> YesNoAsync(string errorMessage, CancellationToken ct)
    //    {
    //        Console.WriteLine(errorMessage);

    //        while (true)
    //        {
    //            if (ct.IsCancellationRequested)
    //            {
    //                Console.WriteLine("Operation cancelled.");
    //                return false; // Zwróć false w przypadku anulowania
    //            }

    //            // Odczyt z konsoli powinien być uruchomiony w osobnym zadaniu
    //            Task<string> inputTask = Task.Run(Console.ReadLine, ct);

    //            try
    //            {
    //                string input = await inputTask;

    //                if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    return true;
    //                }

    //                if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    return false;
    //                }

    //                Console.WriteLine("Invalid input. Please type 'y' or 'n'.");
    //            }
    //            catch (OperationCanceledException)
    //            {
    //                Console.WriteLine("Operation cancelled.");
    //                return false; // Zwróć false w przypadku anulowania
    //            }
    //        }
    //    }
    //}
}
