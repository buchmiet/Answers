#region AUTOEXEC.BAT

using AnswerConsole;
using Answers;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Spectre.Console;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;
using System.Text;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new SimpleLoggerProvider());
});

var logger = loggerFactory.CreateLogger<Program>();
var answerService = new AnswerService( logger);

var randomServie = new RandomService(logger);
var cancellationToken = new CancellationToken();
#endregion AUTOEXEC.BAT


var databaseClass = new DatabaseTierClass(randomServie, answerService);
var httpClass = new HttpTierClass(randomServie, answerService);
var businessLogic = new BusinessLogicClass(databaseClass, httpClass, answerService);
var presentationLayer = new PresentationLayer(businessLogic,new QuickBooksPseudoClass(), answerService);
//presentationLayer.jednaMetoda();
//presentationLayer.drugametoda();
//presentationLayer.trzeciaMetoda();
var res=await presentationLayer.SimulateSomething();
await presentationLayer.ExecuteConcurrentOperations(new CancellationToken());






public partial class PresentationLayer//:IAnswerable
{
    private async System.Threading.Tasks.Task<Answer> TryAsync(
      System.Func<System.Threading.Tasks.Task<Answer>> method,
      System.Threading.CancellationToken ct,
      [System.Runtime.CompilerServices.CallerMemberName] System.String callerName = "",
      [System.Runtime.CompilerServices.CallerFilePath] System.String callerFilePath = "",
      [System.Runtime.CompilerServices.CallerLineNumber] System.Int32 callerLineNumber = 0)
    {

    var timeoutValue = _answerService.HasTimeout ? _answerService.GetTimeout() : System.TimeSpan.Zero; // Pobiera i resetuje timeout
        System.Threading.Tasks.Task<Answers.Answer> methodTask = method();
        // repeat until method returns a successful answer or dialog is concluded
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatch.Start();
        while (true)
        {
            //AnswerService has timeout set, so we need to wait for the method to complete or timeout to occur
            if (timeoutValue != System.TimeSpan.Zero)
            {
                Answer answer;
                try
                {
                    answer = await methodTask.WaitAsync(timeoutValue, ct);
                }
                catch (System.TimeoutException)
                {
                    string warningMessage = string.Format(
                        _answerService.Strings.WarningMessageFormat,
                        callerName,
                        System.IO.Path.GetFileName(callerFilePath),
                        callerLineNumber,
                        "Operation timed out"
                    );
                    _answerService.LogWarning(warningMessage);

                    System.String action = string.Format(
                        _answerService.Strings.CallerMessageFormat,
                        callerName,
                        System.IO.Path.GetFileName(callerFilePath),
                        callerLineNumber
                    ); 
                    // if timeout dialogs are implemented
                    if (_answerService.HasTimeOutDialog || _answerService.HasTimeOutAsyncDialog)
                    {
                        System.String timeoutMessage = string.Format(_answerService.Strings.TimeoutMessage, action); 
                        // async dialog has priority, but sync will run if async is not available
                        using var dialogCts = new System.Threading.CancellationTokenSource();
                        using var linkedCts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct, dialogCts.Token);
                        System.Threading.Tasks.Task<bool> dialogTask = ChooseBetweenAsyncAndNonAsyncDialogTask(timeoutMessage, linkedCts.Token);
                        var response = await ProcessTimeOutDialog(dialogTask, dialogCts);

                        switch (response.Response)
                        {
                            case Answers.Dialogs.DialogResponse.Continue:
                            // carry on waiting
                                continue;
                            case Answers.Dialogs.DialogResponse.Cancel:
                                stopwatch.Stop();
                                return response.Answer;
                            case Answers.Dialogs.DialogResponse.DoNotWait:
                                stopwatch.Stop();
                                return response.Answer;
                            case Answers.Dialogs.DialogResponse.Answered:
                                if (response.Answer.IsSuccess)
                                {
                                    stopwatch.Stop();
                                    return response.Answer;
                                }
                                methodTask = method();
                                continue;
                        }
                        
                    }
                    // Użytkownik wybrał "No" lub brak dostępnych dialogów
                    return TimedOutResponse();
                }
                catch (System.OperationCanceledException)
                {
                    return Answer.Prepare(_answerService.Strings.CancelledText).Error(_answerService.Strings.CancelMessage);
                }

                var responseReceivedWithinTimeout = await ProcessAnswerAsync(answer);
                switch (responseReceivedWithinTimeout.Response)
                {
                    case Answers.Dialogs.DialogResponse.Answered:
                        stopwatch.Stop();
                        return responseReceivedWithinTimeout.Answer;
                    case Answers.Dialogs.DialogResponse.DoNotRepeat:
                        stopwatch.Stop();
                        return responseReceivedWithinTimeout.Answer;
                    case Answers.Dialogs.DialogResponse.Continue:
                        continue;
                }
            }
            // Brak określonego timeoutu

            var noTimeoutSetResponse = await ProcessAnswerAsync(await methodTask);
            switch (noTimeoutSetResponse.Response)
            {
                case Answers.Dialogs.DialogResponse.Answered:
                    stopwatch.Stop();
                    return noTimeoutSetResponse.Answer;
                case Answers.Dialogs.DialogResponse.DoNotRepeat:
                    stopwatch.Stop();
                    return noTimeoutSetResponse.Answer;
                case Answers.Dialogs.DialogResponse.Continue:
                    continue;
            }
        }


        Answer TimedOutResponse() => Answer.Prepare(_answerService.Strings.TimeOutText).Error(string.Format(_answerService.Strings.TimeoutElapsedMessage, stopwatch.Elapsed.TotalSeconds));

        System.Threading.Tasks.Task<bool> ChooseBetweenAsyncAndNonAsyncDialogTask(string s, System.Threading.CancellationToken linkedCts) =>
         _answerService.HasTimeOutAsyncDialog ? _answerService.AskYesNoToWaitAsync(s, linkedCts) :
                System.Threading.Tasks.Task.Run(() =>
                    _answerService.AskYesNoToWait(s, linkedCts), ct);
        

        async System.Threading.Tasks.Task<(Answers.Dialogs.DialogResponse Response, Answer Answer)> ProcessAnswerAsync(Answer localAnswer)
        {
            if (!localAnswer.IsSuccess)
            {
                string errorMessage = string.Format(
                    _answerService.Strings.ErrorMessageFormat,
                    callerName,
                    System.IO. Path.GetFileName(callerFilePath),
                    callerLineNumber,
                    localAnswer.Message
                );
                _answerService.LogError(errorMessage);
            }
            if (localAnswer.IsSuccess || localAnswer.DialogConcluded || !(_answerService.HasYesNoDialog || _answerService.HasYesNoAsyncDialog))
            {
                return (Answers.Dialogs.DialogResponse.Answered,localAnswer);
            }
            
            System.Boolean userResponse= _answerService.HasYesNoAsyncDialog? await _answerService.AskYesNoAsync(localAnswer.Message, ct) :
                _answerService.AskYesNo(localAnswer.Message);
            
            if (userResponse)
            {
                methodTask = method();
                return (Answers.Dialogs.DialogResponse.Continue, null);
            }

            localAnswer.ConcludeDialog();
            string userCancelledMessage = string.Format(
                _answerService.Strings.UserCancelledMessageFormat,
                callerName,
                System.IO.Path.GetFileName(callerFilePath),
                callerLineNumber
            );
            _answerService.LogError(userCancelledMessage);
            return (Answers.Dialogs.DialogResponse.DoNotRepeat,localAnswer); // Użytkownik wybrał "No", kończymy
        }


        async System.Threading.Tasks.Task<(Answers.Dialogs.DialogResponse Response, Answer Answer)> ProcessTimeOutDialog(
            System.Threading.Tasks.Task<bool> dialogTask,
            System.Threading.CancellationTokenSource dialogCts)
        {
            try
            {
                var dialogOutcomeTask = await System.Threading.Tasks.Task.WhenAny(methodTask, dialogTask).WaitAsync(ct);

                if (dialogOutcomeTask == methodTask)
                {
                    var localAnswer = await methodTask;
                    await dialogCts.CancelAsync();
                    return (Answers.Dialogs.DialogResponse.Answered, localAnswer);
                }

                // Sprawdzamy czy dialog został zakończony przez użytkownika
                if (await dialogTask)
                {
                    return (Answers.Dialogs.DialogResponse.Continue, null);
                }
                string userCancelledMessage = string.Format(
                    _answerService.Strings.UserCancelledMessageFormat,
                    callerName,
                    System.IO.Path.GetFileName(callerFilePath),
                    callerLineNumber
                );
                _answerService.LogError(userCancelledMessage);
                return (Answers.Dialogs.DialogResponse.DoNotWait,
                    Answer.Prepare(_answerService.Strings.CancelledText).Error(_answerService.Strings.TimeoutError).ConcludeDialog());
            }
            catch (System.OperationCanceledException)
            {
                string errorMessage = string.Format(
                    _answerService.Strings.UserCancelledMessageFormat,
                    callerName,
                    System.IO.Path.GetFileName(callerFilePath),
                    callerLineNumber
                );
                _answerService.LogError(errorMessage);
                return (Answers.Dialogs.DialogResponse.Cancel,
                    Answer.Prepare(_answerService.Strings.CancelMessage).Error(_answerService.Strings.CancelMessage).ConcludeDialog());
            }
        }
    }


    private IAnswerService _answerService;
    private BusinessLogicClass _utilityLayer;
    private QuickBooksPseudoClass _qbpc;
    public PresentationLayer(BusinessLogicClass utilityLayer,QuickBooksPseudoClass qbpc, IAnswerService answerService)
    {
        _qbpc = qbpc;
        _answerService = answerService;
    //   _answerService.AddDialog(new UserDialogStub([true,false],null));
        _utilityLayer = utilityLayer;
     //   LogDetailedInfo();
    }

    public async Task<Answer> SimulateSomething()
    {
        var resp = Answer.Prepare("symulacja dzialania");
        var response =await TryAsync(() => _qbpc.GetDataFromQuickBooks(5, new CancellationToken()), new CancellationToken());
        return resp.Attach(response);
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

public partial class QuickBooksPseudoClass() : IAnswerable
{
    public async Task<Answer> GetDataFromQuickBooks(int id, CancellationToken ct)
    {
        var answer = Answer.Prepare($"[QuickBooksPseudoClass] GetDataFromQuickBooks({id})");
        
        return answer.Error($"Error processing QB data");
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
