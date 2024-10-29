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
    //    public bool HasAsyncYesNo => true;
    //    public bool HasAsyncTimeoutDialog => true;
    //    public bool HasYesNo => true;
    //    public bool HasTimeoutDialog => true;

    //    public bool YesNo(string errorMessage, CancellationToken ct)
    //    {
    //        Console.WriteLine(errorMessage);
    //        while (true)
    //        {
    //            Console.Write("Please enter 'yes' or 'no': ");
    //            var input = Console.ReadLine();
    //            if (input == null)
    //                continue;

    //            if (input.Equals("yes", StringComparison.OrdinalIgnoreCase))
    //                return true;
    //            else if (input.Equals("no", StringComparison.OrdinalIgnoreCase))
    //                return false;
    //            else
    //                Console.WriteLine("Invalid input. Please try again.");
    //        }
    //    }

    //    public bool ContinueTimedOutYesNo(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct)
    //    {
    //        Console.WriteLine(errorMessage);
    //        Console.Write("Please enter 'yes' or 'no': ");

    //        var input = new StringBuilder();

    //        while (true)
    //        {
    //            if (localCancellationToken.IsCancellationRequested || ct.IsCancellationRequested)
    //            {
    //                Console.WriteLine("\nOperation canceled.");
    //                return false;
    //            }

    //            if (Console.KeyAvailable)
    //            {
    //                var key = Console.ReadKey(intercept: true);

    //                if (key.Key == ConsoleKey.Enter)
    //                {
    //                    Console.WriteLine();

    //                    var inputStr = input.ToString();
    //                    if (inputStr.Equals("yes", StringComparison.OrdinalIgnoreCase))
    //                        return true;
    //                    else if (inputStr.Equals("no", StringComparison.OrdinalIgnoreCase))
    //                        return false;
    //                    else
    //                    {
    //                        Console.WriteLine("Invalid input. Please try again.");
    //                        input.Clear();
    //                        Console.Write("Please enter 'yes' or 'no': ");
    //                    }
    //                }
    //                else if (key.Key == ConsoleKey.Backspace)
    //                {
    //                    if (input.Length > 0)
    //                    {
    //                        input.Length--;
    //                        Console.Write("\b \b"); // Erase character from console
    //                    }
    //                }
    //                else if (key.KeyChar != '\u0000') // Non-control character
    //                {
    //                    input.Append(key.KeyChar);
    //                    Console.Write(key.KeyChar);
    //                }
    //            }

    //            Thread.Sleep(50); // Sleep for a short period to avoid busy waiting
    //        }
    //    }

    //    public async Task<bool> YesNoAsync(string errorMessage, CancellationToken ct)
    //    {
    //        Console.WriteLine(errorMessage);
    //        while (true)
    //        {
    //            Console.Write("Please enter 'yes' or 'no': ");
    //            var inputTask = ReadLineAsync(ct);

    //            try
    //            {
    //                var input = await inputTask;

    //                if (input == null)
    //                {
    //                    // Cancellation requested
    //                    Console.WriteLine("\nOperation canceled.");
    //                    return false;
    //                }

    //                if (input.Equals("yes", StringComparison.OrdinalIgnoreCase))
    //                    return true;
    //                else if (input.Equals("no", StringComparison.OrdinalIgnoreCase))
    //                    return false;
    //                else
    //                    Console.WriteLine("Invalid input. Please try again.");
    //            }
    //            catch (OperationCanceledException)
    //            {
    //                Console.WriteLine("\nOperation canceled.");
    //                return false;
    //            }
    //        }
    //    }

    //    public async Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct)
    //    {
    //        Console.WriteLine(errorMessage);
    //        Console.Write("Please enter 'yes' or 'no': ");

    //        var cts = CancellationTokenSource.CreateLinkedTokenSource(localCancellationToken, ct);

    //        while (true)
    //        {
    //            try
    //            {
    //                var input = await ReadLineAsync(cts.Token);

    //                if (input == null)
    //                {
    //                    // Cancellation requested
    //                    Console.WriteLine("\nOperation canceled.");
    //                    return false;
    //                }

    //                if (input.Equals("yes", StringComparison.OrdinalIgnoreCase))
    //                    return true;
    //                else if (input.Equals("no", StringComparison.OrdinalIgnoreCase))
    //                    return false;
    //                else
    //                {
    //                    Console.WriteLine("Invalid input. Please try again.");
    //                    Console.Write("Please enter 'yes' or 'no': ");
    //                }
    //            }
    //            catch (OperationCanceledException)
    //            {
    //                Console.WriteLine("\nOperation canceled.");
    //                return false;
    //            }
    //        }
    //    }

    //    private Task<string> ReadLineAsync(CancellationToken cancellationToken)
    //    {
    //        var tcs = new TaskCompletionSource<string>();

    //        ThreadPool.QueueUserWorkItem(_ =>
    //        {
    //            var input = new StringBuilder();
    //            while (true)
    //            {
    //                if (cancellationToken.IsCancellationRequested)
    //                {
    //                    tcs.TrySetResult(null);
    //                    return;
    //                }

    //                if (Console.KeyAvailable)
    //                {
    //                    var key = Console.ReadKey(intercept: true);

    //                    if (key.Key == ConsoleKey.Enter)
    //                    {
    //                        Console.WriteLine();
    //                        tcs.TrySetResult(input.ToString());
    //                        return;
    //                    }
    //                    else if (key.Key == ConsoleKey.Backspace)
    //                    {
    //                        if (input.Length > 0)
    //                        {
    //                            input.Length--;
    //                            Console.Write("\b \b"); // Erase character from console
    //                        }
    //                    }
    //                    else if (key.KeyChar != '\u0000') // Non-control character
    //                    {
    //                        input.Append(key.KeyChar);
    //                        Console.Write(key.KeyChar);
    //                    }
    //                }

    //                Thread.Sleep(50);
    //            }
    //        }, null);

    //        cancellationToken.Register(() => tcs.TrySetCanceled());

    //        return tcs.Task;
    //    }
    //}
}
