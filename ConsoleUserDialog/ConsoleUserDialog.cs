using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Answers;

namespace ConsoleUserDialog
{
    public class ConsoleUserDialog : IUserDialog
    {
        public bool HasAsyncYesNo => true;
        public bool HasAsyncTimeoutDialog => true;
        public bool HasYesNo => true;
        public bool HasTimeoutDialog => true;

        private static List<string> _confirmations = ["yes", "y"];
        private static List<string> _negations = ["no", "n"];
        private const string ConfirmationQuery = "Please enter '(y)es' or '(n)o':";
        private const string InalidInputMessage = "Invalid input. Please try again.";
        private const string CancelationMessage = "\nOperation canceled.";

        public bool YesNo(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            while (true)
            {
                Console.Write(ConfirmationQuery);
                var input = Console.ReadLine();
                if (input == null)
                    continue;
                if (_confirmations.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (_negations.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
                Console.WriteLine(InalidInputMessage);
            }
        }

        public bool ContinueTimedOutYesNo(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct)
        {
            Console.WriteLine(errorMessage);
            Console.Write(ConfirmationQuery);

            var input = new StringBuilder();

            while (true)
            {
                if (localCancellationToken.IsCancellationRequested || ct.IsCancellationRequested)
                {
                    Console.WriteLine(CancelationMessage);
                    return false;
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();

                        var inputStr = input.ToString();
                        if (_confirmations.Contains(inputStr, StringComparer.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        if (_negations.Contains(inputStr, StringComparer.OrdinalIgnoreCase))
                        {
                            return false;
                        }

                        Console.WriteLine(InalidInputMessage);
                        input.Clear();
                        Console.Write(ConfirmationQuery);
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length > 0)
                        {
                            input.Length--;
                            Console.Write("\b \b"); // Erase character from console
                        }
                    }
                    else if (key.KeyChar != '\u0000') // Non-control character
                    {
                        input.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }

                Thread.Sleep(50); // Sleep for a short period to avoid busy waiting
            }
        }

        public async Task<bool> YesNoAsync(string errorMessage, CancellationToken ct)
        {
            Console.WriteLine(errorMessage);
            while (true)
            {
                Console.Write(ConfirmationQuery);
                var inputTask = ReadLineAsync(ct);

                try
                {
                    var input = await inputTask;

                    if (input == null)
                    {
                        // Cancellation requested
                        Console.WriteLine(CancelationMessage);
                        return false;
                    }


                    if (_confirmations.Contains(input, StringComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (_negations.Contains(input, StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    Console.WriteLine(InalidInputMessage);

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine(CancelationMessage);
                    return false;
                }
            }
        }

        public async Task<bool> ContinueTimedOutYesNoAsync(string errorMessage, CancellationToken localCancellationToken, CancellationToken ct)
        {
            Console.WriteLine(errorMessage);
            Console.Write(ConfirmationQuery);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(localCancellationToken, ct);

            while (true)
            {
                try
                {
                    var input = await ReadLineAsync(cts.Token);

                    if (input == null)
                    {
                        // Cancellation requested
                        Console.WriteLine(CancelationMessage);
                        return false;
                    }

                    if (_confirmations.Contains(input, StringComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (_negations.Contains(input, StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    Console.WriteLine(InalidInputMessage);
                    Console.Write(ConfirmationQuery);

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine(CancelationMessage);
                    return false;
                }
            }
        }

        private Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<string>();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var input = new StringBuilder();
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetResult(null);
                        return;
                    }

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);

                        if (key.Key == ConsoleKey.Enter)
                        {
                            Console.WriteLine();
                            tcs.TrySetResult(input.ToString());
                            return;
                        }
                        else if (key.Key == ConsoleKey.Backspace)
                        {
                            if (input.Length > 0)
                            {
                                input.Length--;
                                Console.Write("\b \b"); // Erase character from console
                            }
                        }
                        else if (key.KeyChar != '\u0000') // Non-control character
                        {
                            input.Append(key.KeyChar);
                            Console.Write(key.KeyChar);
                        }
                    }

                    Thread.Sleep(50);
                }
            }, null);

            cancellationToken.Register(() => tcs.TrySetCanceled());

            return tcs.Task;
        }
    }
}
