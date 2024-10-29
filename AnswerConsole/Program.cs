#region AUTOEXEC.BAT

using AnswerConsole;
using Answers;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Spectre.Console;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new SimpleLoggerProvider());
});

var logger = loggerFactory.CreateLogger<Program>();
var answerService = new AnswerService(null, logger);
//answerService.AddTimeoutDialog(new ConsoleUserDialog());
var randomServie = new RandomService(logger);
var cancellationToken = new CancellationToken();
#endregion AUTOEXEC.BAT


var databaseClass = new DatabaseTierClass(randomServie, answerService);
var httpClass = new HttpTierClass(randomServie, answerService);
var businessLogic = new BusinessLogicClass(databaseClass, httpClass, answerService);
var presentationLayer = new PresentationLayer(businessLogic, answerService);
//presentationLayer.jednaMetoda();
//presentationLayer.drugametoda();
//presentationLayer.trzeciaMetoda();

await presentationLayer.ExecuteConcurrentOperations(new CancellationToken());




//await wyswietlacz.DisplayProductInformation(0, cancellationToken);




//public partial class TestClassName : IAnswerable
//{
//    private readonly IAnswerService _customAnswerService;
//}


//public class PresentationLayer
//{
//    IAnswerService _answerService;
//    //private BusinessLogicClass _utilityLayer;

//    //public PresentationLayer(Answers.IAnswerService answerService, BusinessLogicClass utilityLayer)
//    //{
//    //    _answerService = answerService;
//    //    _answerService.AddDialog(new ConsoleUserDialog());
//    //    this._utilityLayer = utilityLayer;
//    //}

//    //public async Task DisplayProductInformation(int id, CancellationToken ct)
//    //{
//    //    var response = await TryAsync(() => _utilityLayer.GetOrderAndProductsData(0, ct), ct);
//    //    if (response.IsSuccess)
//    //    {
//    //        Console.WriteLine(response.GetValue<string>());
//    //    }
//    //    else
//    //    {
//    //        DisplayError(response);
//    //    }
//    //}

//    //public void DisplayError(Answer answer)
//    //{
//    //    Console.ForegroundColor = ConsoleColor.Red;
//    //    Console.Write("Error:");
//    //    Console.ResetColor();
//    //    Console.WriteLine(answer.Message);
//    //}


//    private BusinessLogicClass _utilityLayer;
//    private BusinessLogicClass _anotherUtilityLayer;

//    public PresentationLayer(Answers.IAnswerService answerService, BusinessLogicClass utilityLayer, BusinessLogicClass anotherUtilityLayer)
//    {
//        _answerService = answerService;
//        _answerService.AddDialog(new ConsoleUserDialog());
//        this._utilityLayer = utilityLayer;
//        this._anotherUtilityLayer = anotherUtilityLayer;
//    }

//    public async Task SimulateConcurrentButtonClicks(CancellationToken ct)
//    {
//        var task1 = DisplayDatabaseInformation(1, ct);
//        var task2 = DisplayWebApiInformation(2, ct);

//        await Task.WhenAll(task1, task2);
//    }

//    public async Task DisplayDatabaseInformation(int id, CancellationToken ct)
//    {
//        var response = await TryAsync(() => _utilityLayer.GetOrderAndProductsData(id, ct), ct, TimeSpan.FromSeconds(7));
//        if (response.IsSuccess)
//        {
//            Console.WriteLine($"[DB] Success: {response.GetValue<string>()}");
//        }
//        else
//        {
//            Console.WriteLine($"[DB] Error: {response.Message}");
//        }
//    }

//    public async Task DisplayWebApiInformation(int id, CancellationToken ct)
//    {
//        var response = await TryAsync(() => _anotherUtilityLayer.GetOrderAndProductsData(id, ct), ct, TimeSpan.FromSeconds(7));
//        if (response.IsSuccess)
//        {
//            Console.WriteLine($"[WebAPI] Success: {response.GetValue<string>()}");
//        }
//        else
//        {
//            Console.WriteLine($"[WebAPI] Error: {response.Message}");
//        }
//    }




//    public async Task<Answers.Answer> TryAsync(
//        Func<Task<Answers.Answer>> method,
//        CancellationToken ct,
//        TimeSpan? timeout = null)
//    {
//        while (true)
//        {
//            Task<Answers.Answer> methodTask = method();
//            Task timeoutTask = null;
//            Answers.Answer answer;

//            if (timeout.HasValue)
//            {
//                // Create a delay task that completes after the specified timeout
//                timeoutTask = Task.Delay(timeout.Value, ct);
//            }

//            if (timeoutTask != null)
//            {
//                // Wait for either the method to complete or the timeout to occur
//                Task completedTask = await Task.WhenAny(methodTask, timeoutTask);

//                if (completedTask == methodTask)
//                {
//                    // The method completed before the timeout

//                    answer = await methodTask;
//                    if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
//                    {
//                        return answer;
//                    }

//                    // Method failed; prompt the user to retry
//                    if (await _answerService.AskYesNoAsync(answer.Message, ct))
//                    {
//                        continue;
//                    }

//                    answer.ConcludeDialog();
//                    return answer;
//                }

//                // The timeout occurred before the method completed
//                //if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
//                //        "The operation timed out. Do you want to retry?", ct))
//                //{
//                //    // Cannot prompt the user or user chose not to retry; return timed-out answer
//                //    answer = Answers.Answer.Prepare("Time out timer");
//                //    return answer.Error($"{timeout.Value.TotalSeconds} seconds elapsed");
//                //}

//                var currentActivity =System.Diagnostics.Activity.Current;
//                var message = currentActivity?.OperationName ?? "Unknown task";
//                if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
//                        $"The operation {message} timed out. Do you want to retry?", ct))
//                {
//                    answer =Answers.Answer.Prepare(message);
//                    return answer.Error($"{timeout.Value.TotalSeconds} seconds elapsed");
//                }


//                // User chose to retry; loop again
//                continue;
//            }

//            // No timeout specified; await the method normally
//            answer = await methodTask; // Let exceptions propagate if any

//            if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
//            {
//                return answer;
//            }

//            // Method failed; prompt the user to retry
//            if (await _answerService.AskYesNoAsync(answer.Message, ct))
//            {
//                continue;
//            }

//            answer.ConcludeDialog();
//            return answer;
//        }
//    }

//}
//public partial class DatabaseTierClass:IAnswerable
//{
//    private RandomService _randomService;

//    public DatabaseTierClass(RandomService randomService)
//    {
//        _randomService = randomService;
//    }

//    public async Task<Answer> GetOrderData(int orderId, CancellationToken ct)
//    {
//        using (var response = Answer.Prepare($"GetOrderData({orderId})"))
//        {
//            await Task.Delay(5000, ct); // Simulate work
//            if (_randomService.NextBool())
//            {
//                return response.WithValue($"Order {orderId}");
//            }
//            else
//            {
//                return response.Error($"Error fetching order {orderId}");
//            }
//        }
//    }

//    public async Task<Answer> GetProductData(int productId, CancellationToken ct)
//    {
//        using (var response = Answer.Prepare($"GetProductData({productId})"))
//        {
//            await Task.Delay(5000, ct); // Simulate work
//            if (_randomService.NextBool())
//            {
//                return response.WithValue($"Product {productId}");
//            }
//            else
//            {
//                return response.Error($"Error fetching product {productId}");
//            }
//        }
//    }
//}

//public partial class BusinessLogicClass:IAnswerable
//{
//    private DatabaseTierClass _serviceTier;

//    public BusinessLogicClass(DatabaseTierClass serviceTier)
//    {
//        this._serviceTier = serviceTier;
//    }

//    public async Task<Answer> MethodLevel1(int id, CancellationToken ct)
//    {
//        using (var answer = Answer.Prepare($"MethodLevel1({id})"))
//        {
//            Answer result = await TryAsync(() => MethodLevel2(id, ct), ct);
//            if (!result.IsSuccess)
//            {
//                return answer.Error($"Error in MethodLevel1: {result.Message}");
//            }
//            return answer.WithValue(result.GetValue<string>());
//        }
//    }

//    private async Task<Answer> MethodLevel2(int id, CancellationToken ct)
//    {
//        using (var answer = Answer.Prepare($"MethodLevel2({id})"))
//        {
//            Answer result = await TryAsync(() => MethodLevel3(id, ct), ct);
//            if (!result.IsSuccess)
//            {
//                return answer.Error($"Error in MethodLevel2: {result.Message}");
//            }
//            return answer.WithValue(result.GetValue<string>());
//        }
//    }

//    private async Task<Answer> MethodLevel3(int id, CancellationToken ct)
//    {
//        using (var answer = Answer.Prepare($"MethodLevel3({id})"))
//        {
//            // Simulate work
//            await Task.Delay(1000, ct);
//            if (/* some condition */ false)
//            {
//                return answer.Error("An error occurred in MethodLevel3");
//            }
//            return answer.WithValue($"Result from MethodLevel3({id})");
//        }
//    }
//}

public partial class PresentationLayer//:IAnswerable
{
    //private async System.Threading.Tasks.Task<Answers.Answer> TryAsync(
    //  System.Func<System.Threading.Tasks.Task<Answers.Answer>> method,
    //  System.Threading.CancellationToken ct,
    //  [System.Runtime.CompilerServices.CallerMemberName] System.String callerName = "",
    //  [System.Runtime.CompilerServices.CallerFilePath] System.String callerFilePath = "",
    //  [System.Runtime.CompilerServices.CallerLineNumber] System.Int32 callerLineNumber = 0)
    //{
    //    var timeoutValue = _answerService.HasTimeout ? _answerService.GetTimeout() : System.TimeSpan.Zero; // Pobiera i resetuje timeout
    //    System.Threading.Tasks.Task<Answers.Answer> methodTask = method();
    //    // repeat until method returns a successful answer or dialog is concluded
    //    var stopwatch =System.Diagnostics.Stopwatch.StartNew();
    //    stopwatch.Start();
    //    while (true)
    //    {
    //        //AnswerService has timeout set, so we need to wait for the method to complete or timeout to occur
    //        if (timeoutValue != System.TimeSpan.Zero)
    //        {
    //            Answers.Answer answer;
    //            try
    //            {
    //                answer = await methodTask.WaitAsync(timeoutValue, ct);
    //            }
    //            catch (System. TimeoutException)
    //            {
    //                // Wystąpił timeout
    //                System.String action = $"{callerName} at {System.IO.Path.GetFileName(callerFilePath)}:{callerLineNumber}";
    //                // if timeout dialogs are implemented
    //                if (_answerService.HasTimeOutDialog || _answerService.HasTimeOutAsyncDialog)
    //                {
    //                    System.String timeoutMessage = $"The operation '{action}' timed out. Do you want to retry?";
    //                    // async dialog has priority, but sync will run if async is not available
    //                    using System.Threading.CancellationTokenSource dialogCts = new System.Threading.CancellationTokenSource();
    //                    System.Threading.Tasks.Task<bool> dialogTask = ChooseBetweenAsyncAndNonAsyncDialogTask(timeoutMessage, dialogCts);
    //                    var response = await ProcessTimeOutDialog(dialogTask, timeoutMessage, dialogCts);
    //                    // response from Task<bool> was not successful
    //                    if (!response.IsSuccess)
    //                    {
    //                        // carry on waiting
    //                        continue;
    //                    }
    //                    // if user chose not to continue,return timeout answer without any value
    //                    if (response.GetValue<Answers.Answer>() is Answers.Answer { IsSuccess: false } dialogAnswer)
    //                    {
    //                        stopwatch.Stop();
    //                        return dialogAnswer;
    //                    }
    //                }
    //                // Użytkownik wybrał "No" lub brak dostępnych dialogów
    //                return TimedOutResponse();
    //            }
    //            catch (System.OperationCanceledException)
    //            {
    //                return Answers.Answer.Prepare("Cancelled").Error("Operation canceled by user");
    //            }

    //            var responseReceivedWithinTimeout = await ProcessAnswerAsync(answer);
    //            if (!responseReceivedWithinTimeout.IsSuccess)
    //            {
    //                // response from Task<Answer> was not successful
    //                // try again
    //                continue;
    //            }
    //            // response from Task<Answer> was successful
    //            // return the value
    //            stopwatch.Stop();
    //            return responseReceivedWithinTimeout.GetValue<Answers.Answer>();
    //        }
    //        // Brak określonego timeoutu
    //        var noTimeoutSetResponse = await ProcessAnswerAsync(await methodTask);
    //        if (!noTimeoutSetResponse.IsSuccess)
    //        {
    //            continue;
    //        }
    //        stopwatch.Stop();
    //        return noTimeoutSetResponse.GetValue<Answers.Answer>();
    //    }


    //    Answers.Answer TimedOutResponse()=> Answers.Answer.Prepare("Time out").Error($"{stopwatch.Elapsed.TotalSeconds} seconds elapsed");

    //    System.Threading.Tasks.Task<bool> ChooseBetweenAsyncAndNonAsyncDialogTask(string s, System.Threading.CancellationTokenSource cancellationTokenSource)
    //    {
    //        return _answerService.HasTimeOutAsyncDialog? _answerService.AskYesNoToWaitAsync(s, cancellationTokenSource.Token, ct):
    //            System.Threading.Tasks.Task.Run(() =>
    //                _answerService.AskYesNoToWait(s, cancellationTokenSource.Token, ct), ct);
    //    }

    //    async System.Threading.Tasks.Task<Answers.Answer> ProcessAnswerAsync(Answers.Answer localAnswer)
    //    {
    //        Answers.Answer returnAnswer=Answers.Answer.Prepare("ProcessAnswerAsync");
    //        if (localAnswer.IsSuccess || localAnswer.DialogConcluded || !(_answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
    //        {
    //            return returnAnswer.WithValue(localAnswer);
    //        }

    //        System.Boolean userResponse;
    //        if (_answerService.HasYesNoAsyncDialog)
    //        {
    //            userResponse = await _answerService.AskYesNoAsync(localAnswer.Message, ct);
    //        }
    //        else
    //        {
    //            userResponse = _answerService.AskYesNo(localAnswer.Message);
    //        }

    //        if (userResponse)
    //        {
    //            methodTask = method();
    //            returnAnswer.Error("Yes pressed"); // Użytkownik wybrał "Yes", ponawiamy operację
    //        }

    //        localAnswer.ConcludeDialog();
    //        return returnAnswer.WithValue(localAnswer); // Użytkownik wybrał "No", kończymy
    //    }

    //    async System.Threading.Tasks.Task<Answers.Answer> ProcessTimeOutDialog(
    //        System.Threading.Tasks.Task<bool> dialogTask, 
    //        System.String timeoutMessage,System.Threading.CancellationTokenSource dialogCts)
    //    {
    //        Answers.Answer response = Answers.Answer.Prepare("ProcessAnswerAsync");
    //        System.Threading.Tasks.Task dialogOutcomeTask;
    //        try
    //        {
    //            dialogOutcomeTask = await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask);
    //        }
    //        catch (System. OperationCanceledException ex)
    //        {
    //            return Answers.Answer.Prepare("Cancelled").Error(ex.Message);
    //        }

    //        if (dialogOutcomeTask == methodTask)
    //        {
    //            var localAnswer = await methodTask;
    //            await dialogCts.CancelAsync();
    //            return response.WithValue(localAnswer);
    //        }
    //        if (await dialogTask)
    //        {
    //            return response.Error("User wishes to continue");
    //        }
    //        return response.WithValue(Answers.Answer.Prepare("Timeout").Error("User wishes not to wait").ConcludeDialog());
    //    }
    //}


    private async System.Threading.Tasks.Task<Answers.Answer> TryAsync(
        System.Func<System.Threading.Tasks.Task<Answers.Answer>> method,
        System.Threading.CancellationToken ct,
        [System.Runtime.CompilerServices.CallerMemberName] System.String callerName = "",
        [System.Runtime.CompilerServices.CallerFilePath] System.String callerFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] System.Int32 callerLineNumber = 0)
    {
        System.TimeSpan timeoutValue = _answerService.HasTimeout ? _answerService.GetTimeout() : System.TimeSpan.Zero;
        System.Threading.Tasks.Task<Answers.Answer> methodTask = method();
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (true)
        {
            try
            {
                Answers.Answer methodResult;

                if (timeoutValue != System.TimeSpan.Zero)
                {
                    methodResult = await WaitWithTimeoutAsync(methodTask, timeoutValue, ct);
                }
                else
                {
                    methodResult = await methodTask;
                }

                Answers.Answer processedAnswer = await ProcessAnswerAsync(methodResult, ct);

                if (!processedAnswer.IsSuccess)
                {
                    if (processedAnswer.DialogConcluded)
                    {
                        stopwatch.Stop();
                        return processedAnswer;
                    }

                    // If methodTask is completed (unsuccessfully), we need to restart it.
                    if (methodTask.IsCompleted)
                    {
                        methodTask = method();
                    }

                    // Continue waiting for the methodTask to complete.
                    continue;
                }

                stopwatch.Stop();
                return processedAnswer.GetValue<Answers.Answer>();
            }
            catch (System.TimeoutException)
            {
                (System.Boolean ShouldRetry, Answers.Answer Answer) timeoutResponse = await HandleTimeoutAsync(
                    methodTask, ct, callerName, callerFilePath, callerLineNumber, stopwatch);

                if (timeoutResponse.ShouldRetry)
                {
                    // Continue waiting for the existing methodTask.
                    continue;
                }

                stopwatch.Stop();
                return timeoutResponse.Answer;
            }
            catch (System.OperationCanceledException)
            {
                stopwatch.Stop();
                return Answers.Answer.Prepare("Cancelled").Error("Operation canceled by user");
            }
        }


    }

    private async System.Threading.Tasks.Task<Answers.Answer> WaitWithTimeoutAsync(
        System.Threading.Tasks.Task<Answers.Answer> task,
        System.TimeSpan timeout,
        System.Threading.CancellationToken ct)
    {
        using System.Threading.CancellationTokenSource cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            return await task.WaitAsync(cts.Token);
        }
        catch (System.OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new System.TimeoutException();
        }
    }


    private async System.Threading.Tasks.Task<Answers.Answer> ProcessAnswerAsync(
        Answers.Answer methodResult,
        System.Threading.CancellationToken ct)
    {
        if (methodResult.IsSuccess || methodResult.DialogConcluded || !(_answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
        {
            return Answers.Answer.Prepare("Success").WithValue(methodResult);
        }

        System.Boolean userWantsToRetry;

        if (_answerService.HasYesNoAsyncDialog)
        {
            userWantsToRetry = await _answerService.AskYesNoAsync(methodResult.Message, ct);
        }
        else
        {
            userWantsToRetry = _answerService.AskYesNo(methodResult.Message);
        }

        if (userWantsToRetry)
        {
            // User chose to retry; we'll continue waiting for methodTask or restart it if it's completed.
            return Answers.Answer.Prepare("Retry").Error("User chose to retry");
        }

        methodResult.ConcludeDialog();
        return Answers.Answer.Prepare("DialogConcluded").WithValue(methodResult);
    }

    private async System.Threading.Tasks.Task<(System.Boolean ShouldRetry, Answers.Answer Answer)> HandleTimeoutAsync(
    System.Threading.Tasks.Task<Answers.Answer> methodTask,
    System.Threading.CancellationToken ct,
    System.String callerName,
    System.String callerFilePath,
    System.Int32 callerLineNumber,
    System.Diagnostics.Stopwatch stopwatch)
    {
        System.String action = $"{callerName} at {System.IO.Path.GetFileName(callerFilePath)}:{callerLineNumber}";
        System.String timeoutMessage = $"The operation '{action}' timed out. Do you want to wait?";

        using var dialogCts = new System.Threading.CancellationTokenSource();

        System.Threading.Tasks.Task<bool> dialogTask;

        if (_answerService.HasTimeOutAsyncDialog)
        {
            dialogTask = _answerService.AskYesNoToWaitAsync(timeoutMessage, dialogCts.Token, ct);
        }
        else if (_answerService.HasTimeOutDialog)
        {
            dialogTask = System.Threading.Tasks.Task.Run(() =>
                _answerService.AskYesNoToWait(timeoutMessage, dialogCts.Token, ct), ct);
        }
        else
        {
            // No dialog available; return a timeout answer.
            return (false, Answers.Answer.Prepare("Timeout")
                .Error($"{stopwatch.Elapsed.TotalSeconds} seconds elapsed")
                .ConcludeDialog());
        }

        // Wait for either the methodTask to complete or the dialog response
        if (await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask) == methodTask)
        {
            // Method completed before user responded; cancel the dialog
            dialogCts.Cancel();

            // Get the method result
            Answers.Answer methodResult = await methodTask;

            return (false, methodResult);
        }

        // Get the user's response
        System.Boolean userWantsToWait = await dialogTask;

        if (userWantsToWait)
        {
            // User wants to wait; continue waiting for methodTask
            return (true, null);
        }
        else
        {
            // User does not want to wait; return a timeout answer
            return (false, Answers.Answer.Prepare("Timeout")
                .Error("User chose not to wait")
                .ConcludeDialog());
        }
    }


    private IAnswerService _answerService;
    private BusinessLogicClass _utilityLayer;
    public PresentationLayer(BusinessLogicClass utilityLayer, Answers.IAnswerService answerService)
    {
        _answerService = answerService;
       _answerService.AddDialog(new UserDialogStub([true,false],null));
        _utilityLayer = utilityLayer;
     //   LogDetailedInfo();
    }


    public void jednaMetoda()
    {
    //    LogDetailedInfo();
    }

    public void drugametoda()
    {
     //   LogDetailedInfo();
    }

    public void trzeciaMetoda()
    {
      //  LogDetailedInfo();
    }

    public async Task ExecuteConcurrentOperations(CancellationToken ct)
    {
        AnsiConsole.MarkupLine("[cyan]public[/] [green]async Task[/][cyan]<Answer>[/] [green]ExecuteConcurrentOperations[/][white]([cyan]CancellationToken[/] [white]ct[/])[/]");
        AnsiConsole.WriteLine("level 1");
        //LogDetailedInfo();
        _answerService.SetTimeout(TimeSpan.FromSeconds(1));
        Task<Answer> task1 = FetchDatabaseData(1, ct);
     //   Task<Answer> task2 = FetchWebApiData(2, ct);

        // Poczekaj na wszystkie taski
        await Task.WhenAll(task1);//, task2);

        // Pobierz wyniki z tasków
        Answer result1 = await task1;
        //Answer result2 = await task2;

        // Teraz możesz operować na odpowiedziach
        if (result1.IsSuccess)
        {
            AnsiConsole.WriteLine($"Result of operation : {result1.GetValue<string>()}");
        }
        else
        {
            AnsiConsole.WriteLine($"Error in FetchDatabaseData: {result1.Message}");
        }

        //if (result2.IsSuccess)
        //{
        //    Console.WriteLine($"Result from FetchWebApiData: {result2.GetValue<string>()}");
        //}
        //else
        //{
        //    Console.WriteLine($"Error in FetchWebApiData: {result2.Message}");
        //}
    }


    private async Task<Answer> FetchDatabaseData(int id, CancellationToken ct)
    {
        AnsiConsole.MarkupLine("[cyan]public[/] [green]async Task[/][cyan]<Answer>[/] [green]FetchDatabaseData[/][white]([cyan]int[/] [white]id[/], [cyan]CancellationToken[/] [white]ct[/])[/]");
        AnsiConsole.WriteLine("level 2");
 //       _answerService.SetTimeout(TimeSpan.FromSeconds(2));
        Answer answer = Answer.Prepare("[PresentationLayer] Fetching data from database");
        var response= await TryAsync(() => _utilityLayer.GetDatabaseData(id, ct), ct);
        return answer.Attach(response);
    }

    private async Task<Answer> FetchWebApiData(int id, CancellationToken ct)
    {
        _answerService.SetTimeout(TimeSpan.FromSeconds(3));
        Answer answer = Answer.Prepare("[PresentationLayer] Fetching data from web api");
        return answer.Attach(await TryAsync(() => _utilityLayer.GetWebApiData(id, ct), ct));
    }

}



public partial class BusinessLogicClass(DatabaseTierClass databaseTier, HttpTierClass httpTier) : IAnswerable
{
    public async Task<Answer> GetDatabaseData(int id, CancellationToken ct)
    {
        AnsiConsole.MarkupLine("[cyan]public[/] [green]async Task[/][cyan]<Answer>[/] [green]GetDatabaseData[/][white]([cyan]int[/] [white]id[/], [cyan]CancellationToken[/] [white]ct[/])[/]");
        AnsiConsole.WriteLine("level 3");
        var answer = Answer.Prepare($"[BusinessLogicClass] GetDatabaseData({id})");
        Answer result = await TryAsync(() => databaseTier.GetDataFromDatabase(id, ct), ct);
        return answer.Attach(result);
    }

    public async Task<Answer> GetWebApiData(int id, CancellationToken ct)
    {
        var answer = Answer.Prepare($"[BusinessLogicClass] GetWebApiData({id})");
        Answer result = await TryAsync(() => httpTier.GetDataFromWebApi(id, ct), ct);
        return answer.Attach(result);
    }
}


public partial class DatabaseTierClass(RandomService randomService) : IAnswerable
{
    public async Task<Answer> GetDataFromDatabase(int id, CancellationToken ct)
    {
        AnsiConsole.MarkupLine("[cyan]private[/] [green]async Task[/][cyan]<Answer>[/] [green]GetDataFromDatabase[/][white]([cyan]int[/] {id}, [cyan]CancellationToken[/] [white]ct[/])[/]");
        AnsiConsole.WriteLine("level 4");
        var answer = Answer.Prepare($"[DatabaseTierClass] GetDataFromDatabase({id})");
        // Simulate work
        await Task.Delay(2000, ct);
        var randomValue = randomService.NextBool();
        AnsiConsole.MarkupLine($"Next random value is [green]{randomValue}[/]");
        if (randomValue)
        {
      //      AnsiConsole.MarkupLine($"I am returning answer with [green]IsSuccess=true[/] and value='[yellow]DatabaseData_{id}[/]'");
            return answer.WithValue($"[yellow]DatabaseData_{id}[/]");
        }

        //AnsiConsole.MarkupLine(
        //    $"I am returning answer with [red]IsSuccess=false[/] and message='[red]Error fetching data from database for ID {id}[/]'");
        return answer.Error($"[red]Error fetching data from database for ID {id}[/]");
    }

}

public partial class HttpTierClass(RandomService randomService) : IAnswerable
{
    public async Task<Answer> GetDataFromWebApi(int id, CancellationToken ct)
    {
      
        var answer = Answer.Prepare($"[HttpTierClass] GetDataFromWebApi({id})");
        // Simulate work
        await Task.Delay(2000, ct);
        var randomValue = randomService.NextBool();
        AnsiConsole.MarkupLine($"Next random value is {randomValue}");
        if (randomValue)
        {
            AnsiConsole.MarkupLine($"I am returning answer with IsSuccess=true and value='WebApiData_{ id}'");
            return answer.WithValue($"WebApiData_{id}");
        }

        AnsiConsole.MarkupLine(
            $"I am returning answer with IsSuccess=false and message='Error fetching data from Web API for ID {id}");
        return answer.WithValue($"WebApiData_{id}");
        
    }
}



//public partial class DatabaseTierClass : IAnswerable
//{
//    private RandomService _randomService;
//    public DatabaseTierClass(RandomService randomService)
//    {
//        _randomService = randomService;
//    }
//    public async Task<Answer> GetOrderData(int orderId, CancellationToken ct)
//    {
//        var response = Answer.Prepare($"GetOrderData({orderId}, {ct.ToString()})");
//        return _randomService.NextBool() ?
//            response.WithValue($"Order {orderId}") :
//            response.Error($"There has been an error fetching order {orderId}");
//    }

//    public async Task<Answer> GetProductData(int productId, CancellationToken ct)
//    {
//        var response = Answer.Prepare($"GetProductData({productId}, {ct.ToString()})");
//        return _randomService.NextBool() ?
//            response.WithValue($"Product {productId}") :
//            response.Error($"There has been an error fetching product {productId}");
//    }

//    public async Task<Answer> ExtractPlentyOfData(CancellationToken ct)
//    {
//        var waitTime = _randomService.NextInt(10);
//        var response = Answer.Prepare($"ExtractPlentyOfData({waitTime}, {ct.ToString()})");
//        await Task.Delay(waitTime, ct);
//        return response.WithValue("Data extracted");
//    }

//}

//public partial class BusinessLogicClass : IAnswerable
//{
//    private DatabaseTierClass _serviceTier;

//    public BusinessLogicClass(DatabaseTierClass serviceTier)
//    {
//        this._serviceTier = serviceTier;
//    }

//    public async Task<Answer> GetOrderAndProductsData(int orderId, CancellationToken ct)
//    {

//        var response = Answer.Prepare($"GetOrderAndProductsData({orderId}, {ct.ToString()})");

//        Answer resp = await TryAsync(() => _serviceTier.GetOrderData(orderId, ct), ct);
//        if (!resp.IsSuccess)
//        { return resp.Error($"Could not fetch order {orderId}"); }
//        Answer resp2 = await TryAsync(() => _serviceTier.GetProductData(orderId, ct), ct);
//        return !resp2.IsSuccess ? resp.Error($"Could not fetch product {orderId}") : response.WithValue(resp.GetValue<string>() + " " + resp2.GetValue<string>());
//    }

//    public async Task<Answer> SimulateLongWaitingTask(CancellationToken ct)
//    {
//        var response = Answer.Prepare("Simulating long waiting task");
//        Answer resp = await TryAsync(() => _serviceTier.ExtractPlentyOfData(ct), ct);
//        return resp.IsSuccess
//            ? resp : response.Attach(resp);
//    }
//}
