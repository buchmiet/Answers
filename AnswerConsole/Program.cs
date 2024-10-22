#region AUTOEXEC.BAT

using AnswerConsole;
using Answers;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new SimpleLoggerProvider());
});

var logger = loggerFactory.CreateLogger<Program>();
var answerService = new AnswerService(null, logger);
var randomServie = new RandomService(logger);
var cancellationToken = new CancellationToken();
#endregion AUTOEXEC.BAT
var databaseClass = new DatabaseTierClass(randomServie, answerService);
var httpClass = new HttpTierClass(randomServie, answerService);
var businessLogic = new BusinessLogicClass(databaseClass, httpClass, answerService);
var presentationLayer = new PresentationLayer(businessLogic, answerService);


    await presentationLayer.ExecuteConcurrentOperations(new CancellationToken());


string GetFullOperationName(System.Diagnostics.Activity activity)
{
    if (activity == null) return null;

    var operationNames = new Stack<string>();
    var currentActivity = activity;

    while (currentActivity != null)
    {
        operationNames.Push(currentActivity.OperationName);
        currentActivity = currentActivity.Parent;
    }

    return string.Join(" -> ", operationNames);
}

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
//    //    _answerService.AddYesNoDialog(new ConsoleUserDialog());
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
//        _answerService.AddYesNoDialog(new ConsoleUserDialog());
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

public class PresentationLayer
{
    private async System.Threading.Tasks.Task<Answers.Answer> TryAsync(
    System.Func<System.Threading.Tasks. Task<Answers. Answer>> method,
    System.Threading. CancellationToken ct,
    System.TimeSpan? timeout = null)
    {
        while (true)
        {
            System.Threading.Tasks. Task<Answers. Answer> methodTask = method();
            System.Threading.Tasks.Task timeoutTask = null;
            Answers.Answer answer;

            if (timeout.HasValue)
            {
                timeoutTask = System.Threading.Tasks.Task.Delay(timeout.Value, ct);
            }

            if (timeoutTask != null)
            {
                System.Threading.Tasks.Task completedTask = await System.Threading.Tasks.Task.WhenAny(methodTask, timeoutTask);

                if (completedTask == methodTask)
                {
                    answer = await methodTask;

                    if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                    {
                        return answer;
                    }

                    if (await _answerService.AskYesNoAsync(answer.Message, ct))
                    {
                        continue;
                    }

                    answer.ConcludeDialog();
                    return answer;
                }

                // Timeout occurred
                if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                        $"The operation timed out. Do you want to retry?", ct))
                {
                    answer =Answers.Answer.Prepare("Time out");
                    return answer.Error($"{timeout.Value.TotalSeconds} seconds elapsed");
                }

                continue;
            }

            // No timeout specified
            answer = await methodTask;

            if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
            {
                return answer;
            }

            if (await _answerService.AskYesNoAsync(answer.Message, ct))
            {
                continue;
            }

            answer.ConcludeDialog();
            return answer;
        }
    }

    private IAnswerService _answerService;
    private BusinessLogicClass _utilityLayer;
    public PresentationLayer(BusinessLogicClass utilityLayer, Answers.IAnswerService answerService)
    {
        _answerService = answerService;
        _answerService.AddYesNoDialog(new ConsoleUserDialog());
        _utilityLayer = utilityLayer;
    }

    public async Task ExecuteConcurrentOperations(CancellationToken ct)
    {
        Task<Answer> task1 = FetchDatabaseData(1, ct);
        Task<Answer> task2 = FetchWebApiData(2, ct);

        // Poczekaj na wszystkie taski
        await Task.WhenAll(task1, task2);

        // Pobierz wyniki z tasków
        Answer result1 = await task1;
        Answer result2 = await task2;

        // Teraz możesz operować na odpowiedziach
        if (result1.IsSuccess)
        {
            Console.WriteLine($"Result from FetchDatabaseData: {result1.GetValue<string>()}");
        }
        else
        {
            Console.WriteLine($"Error in FetchDatabaseData: {result1.Message}");
        }

        if (result2.IsSuccess)
        {
            Console.WriteLine($"Result from FetchWebApiData: {result2.GetValue<string>()}");
        }
        else
        {
            Console.WriteLine($"Error in FetchWebApiData: {result2.Message}");
        }
    }


    private async Task<Answer> FetchDatabaseData(int id, CancellationToken ct)
    {
        Answer answer = Answer.Prepare("[PresentationLayer] Fetching data from database");
        var response= await TryAsync(() => _utilityLayer.GetDatabaseData(id, ct), ct);
        return answer.Attach(response);
    }

    private async Task<Answer> FetchWebApiData(int id, CancellationToken ct)
    {
        Answer answer = Answer.Prepare("[PresentationLayer] Fetching data from web api");
        return answer.Attach(await TryAsync(() => _utilityLayer.GetWebApiData(id, ct), ct));
    }

}



public partial class BusinessLogicClass(DatabaseTierClass databaseTier, HttpTierClass httpTier) : IAnswerable
{
    public async Task<Answer> GetDatabaseData(int id, CancellationToken ct)
    {
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
        var answer = Answer.Prepare($"[DatabaseTierClass] GetDataFromDatabase({id})");
        // Simulate work
        await Task.Delay(2000, ct);
        return randomService.NextBool() ? answer.WithValue($"DatabaseData_{id}") : answer.Error($"Error fetching data from database for ID {id}");
    }

}

public partial class HttpTierClass(RandomService randomService) : IAnswerable
{
    public async Task<Answer> GetDataFromWebApi(int id, CancellationToken ct)
    {
        var answer = Answer.Prepare($"[HttpTierClass] GetDataFromWebApi({id})");
        // Simulate work
        await Task.Delay(2000, ct);
        return randomService.NextBool() ? answer.WithValue($"WebApiData_{id}") : answer.Error($"Error fetching data from Web API for ID {id}");
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
