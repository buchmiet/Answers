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
var tescik = new ServiceTierClass(randomServie, answerService);
var tescik2 = new UtilityLayerClass(tescik, answerService);
var presentationLayer = new PresentationLayer( tescik2, answerService);
using (var cts = new CancellationTokenSource())
{
    cts.CancelAfter(TimeSpan.FromSeconds(5)); // Set a global timeout
    await presentationLayer.ExecuteConcurrentOperations(cts.Token);
}

//await wyswietlacz.DisplayProductInformation(0, cancellationToken);




//public partial class TestClassName : IAnswerable
//{
//    private readonly IAnswerService _customAnswerService;
//}


//public class PresentationLayer
//{
//    IAnswerService _answerService;
//    //private UtilityLayerClass _utilityLayer;

//    //public PresentationLayer(Answers.IAnswerService answerService, UtilityLayerClass utilityLayer)
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


//    private UtilityLayerClass _utilityLayer;
//    private UtilityLayerClass _anotherUtilityLayer;

//    public PresentationLayer(Answers.IAnswerService answerService, UtilityLayerClass utilityLayer, UtilityLayerClass anotherUtilityLayer)
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
//public partial class ServiceTierClass:IAnswerable
//{
//    private RandomService _randomService;

//    public ServiceTierClass(RandomService randomService)
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

//public partial class UtilityLayerClass:IAnswerable
//{
//    private ServiceTierClass _serviceTier;

//    public UtilityLayerClass(ServiceTierClass serviceTier)
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
    private async Task<Answers.Answer> TryAsync(
    Func<System.Threading.Tasks. Task<Answers. Answer>> method,
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
                var fullOperationName = GetFullOperationName(System.Diagnostics.Activity.Current);
                var message = fullOperationName ?? "Unknown task";
                if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                        $"The operation {message} timed out. Do you want to retry?", ct))
                {
                    answer =Answers.Answer.Prepare(message);
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

    private string GetFullOperationName(System.Diagnostics. Activity activity)
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


    private UtilityLayerClass _utilityLayer;
    private IAnswerService _answerService;

    public PresentationLayer(UtilityLayerClass utilityLayer,IAnswerService answerService)
    {
        _answerService= answerService;
        _utilityLayer = utilityLayer;
    }

    public async Task ExecuteConcurrentOperations(CancellationToken ct)
    {
        var task1 = FetchDatabaseData(1, ct);
        var task2 = FetchWebApiData(2, ct);

        await Task.WhenAll(task1, task2);
    }

    private async Task FetchDatabaseData(int id, CancellationToken ct)
    {
        Answer response = await TryAsync(() => _utilityLayer.GetDatabaseData(id, ct), ct);
        if (response.IsSuccess)
        {
            Console.WriteLine($"[Database] Success: {response.GetValue<string>()}");
        }
        else
        {
            Console.WriteLine($"[Database] Error: {response.Message}");
        }
    }

    private async Task FetchWebApiData(int id, CancellationToken ct)
    {
        Answer response = await TryAsync(() => _utilityLayer.GetWebApiData(id, ct), ct);
        if (response.IsSuccess)
        {
            Console.WriteLine($"[Web API] Success: {response.GetValue<string>()}");
        }
        else
        {
            Console.WriteLine($"[Web API] Error: {response.Message}");
        }
    }

    // Assume TryAsync is implemented as shown earlier
}



public partial class UtilityLayerClass:IAnswerable
{
    private ServiceTierClass _serviceTier;

    public UtilityLayerClass(ServiceTierClass serviceTier)
    {
        this._serviceTier = serviceTier;
    }

    public async Task<Answer> GetDatabaseData(int id, CancellationToken ct)
    {
        using (var answer = Answer.Prepare($"GetDatabaseData({id})"))
        {
            Answer result = await TryAsync(() => _serviceTier.GetDataFromDatabase(id, ct), ct);
            if (!result.IsSuccess)
            {
                return answer.Error($"UtilityLayer Error: {result.Message}");
            }
            return answer.WithValue(result.GetValue<string>());
        }
    }

    public async Task<Answer> GetWebApiData(int id, CancellationToken ct)
    {
        using (var answer = Answer.Prepare($"GetWebApiData({id})"))
        {
            Answer result = await TryAsync(() => _serviceTier.GetDataFromWebApi(id, ct), ct);
            if (!result.IsSuccess)
            {
                return answer.Error($"UtilityLayer Error: {result.Message}");
            }
            return answer.WithValue(result.GetValue<string>());
        }
    }
}


public partial class ServiceTierClass:IAnswerable
{
    private RandomService _randomService;

    public ServiceTierClass(RandomService randomService)
    {
        _randomService = randomService;
    }

    public async Task<Answer> GetDataFromDatabase(int id, CancellationToken ct)
    {
        using (var answer = Answer.Prepare($"GetDataFromDatabase({id})"))
        {
            // Simulate work
            await Task.Delay(2000, ct);
            return _randomService.NextBool() ? answer.WithValue($"DatabaseData_{id}") : answer.Error($"Error fetching data from database for ID {id}");
        }
    }

    public async Task<Answer> GetDataFromWebApi(int id, CancellationToken ct)
    {
        using (var answer = Answer.Prepare($"GetDataFromWebApi({id})"))
        {
            // Simulate work
            await Task.Delay(2000, ct);
            return _randomService.NextBool() ? answer.WithValue($"WebApiData_{id}") : answer.Error($"Error fetching data from Web API for ID {id}");
        }
    }
}


//public partial class ServiceTierClass : IAnswerable
//{
//    private RandomService _randomService;
//    public ServiceTierClass(RandomService randomService)
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

//public partial class UtilityLayerClass : IAnswerable
//{
//    private ServiceTierClass _serviceTier;

//    public UtilityLayerClass(ServiceTierClass serviceTier)
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
