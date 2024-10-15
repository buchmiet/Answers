#region AUTOEXEC.BAT

using AnswerConsole;
using Answers;
using Microsoft.Extensions.Logging;

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
var wyswietlacz = new PresentationLayer(answerService, tescik2);
await wyswietlacz.DisplayProductInformation(0, cancellationToken);

public class PresentationLayer
{
    IAnswerService _answerService;
    private UtilityLayerClass _utilityLayer;

    public PresentationLayer(Answers.IAnswerService answerService, UtilityLayerClass utilityLayer)
    {
        _answerService = answerService;
        _answerService.AddYesNoDialog(new ConsoleUserDialog());
        this._utilityLayer = utilityLayer;
    }

    public async Task DisplayProductInformation(int id, CancellationToken ct)
    {
        var response = await TryAsync(() => _utilityLayer.GetOrderAndProductsData(0, ct), ct);
        if (response.IsSuccess)
        {
            Console.WriteLine(response.GetValue<string>());
        }
        else
        {
            DisplayError(response);
        }
    }

    public void DisplayError(IAnswer answer)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Error:");
        Console.ResetColor();
        Console.WriteLine(answer.Message);
    }





    private async Task<IAnswer> TryAsync(
    Func<Task<IAnswer>> method,
    CancellationToken ct,
    TimeSpan? timeout = null)
    {
        _answerService.LogInfo($"TryAsync started, method: {method.Method.Name}, timeout: {(timeout.HasValue ? timeout.ToString() : "none")}");
        while (true)
        {
            Task<IAnswer> methodTask = method();
            Task timeoutTask = null;
            IAnswer answer;

            if (timeout.HasValue)
            {
                _answerService.LogInfo($"Timeout set to {timeout.Value}");
                timeoutTask = Task.Delay(timeout.Value, ct);
            }

            if (timeoutTask is not null)
            {
                Task completedTask = await Task.WhenAny(methodTask, timeoutTask);

                if (completedTask == methodTask)
                {
                    _answerService.LogInfo($"{method.Method.Name} finished before timeout");
                    try
                    {
                        answer = await methodTask;
                    }
                    catch (Exception ex)
                    {
                        _answerService.LogError($"Exception in method {method.Method.Name}: {ex.Message}");
                        throw;
                    }

                    if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
                    {
                        _answerService.LogInfo($"Method {method.Method.Name} succeeded or dialog concluded");
                        return answer;
                    }

                    _answerService.LogWarning($"Method {method.Method.Name} failed: {answer.Message}");
                    if (await _answerService.AskYesNoAsync(answer.Message, ct))
                    {
                        _answerService.LogInfo("User chose to retry");
                        continue;
                    }

                    _answerService.LogInfo("User declined to retry, concluding dialog");
                    answer.ConcludeDialog();
                    return answer;
                }

                _answerService.LogWarning($"Timeout occurred during {method.Method.Name}");
                if (!_answerService.HasTimeOutDialog || !await _answerService.AskYesNoToWaitAsync(
                        "The operation timed out. Do you want to retry?", ct))
                {
                    _answerService.LogWarning($"User declined to wait, returning timed-out answer");
                    return Answer.TimedOut();
                }

                _answerService.LogInfo("User chose to wait and retry after timeout");
                continue;
            }

            try
            {
                answer = await methodTask;
            }
            catch (Exception ex)
            {
                _answerService.LogError($"Exception in method {method.Method.Name}: {ex.Message}");
                throw;
            }

            if (answer.IsSuccess || answer.DialogConcluded || !_answerService.HasDialog)
            {
                _answerService.LogInfo($"Method {method.Method.Name} succeeded or dialog concluded");
                return answer;
            }

            _answerService.LogWarning($"Method {method.Method.Name} failed: {answer.Message}");
            if (await _answerService.AskYesNoAsync(answer.Message, ct))
            {
                _answerService.LogInfo("User chose to retry");
                continue;
            }

            _answerService.LogInfo("User declined to retry, concluding dialog");
            answer.ConcludeDialog();
            return answer;
        }
    }

}


public partial class ServiceTierClass : IAnswerable
{
    private RandomService _randomService;
    public ServiceTierClass(RandomService randomService)
    {
        _randomService = randomService;
    }
    public async Task<Answer> GetOrderData(int orderId, CancellationToken ct)
    {
        var response = Answer.Prepare($"GetOrderData({orderId}, {ct.ToString()})");
        return _randomService.NextBool() ?
            response.WithValue($"Order {orderId}") :
            response.Error($"There has been an error fetching order {orderId}");
    }

    public async Task<Answer> GetProductData(int productId, CancellationToken ct)
    {
        var response = Answer.Prepare($"GetProductData({productId}, {ct.ToString()})");
        return _randomService.NextBool() ?
            response.WithValue($"Product {productId}") :
            response.Error($"There has been an error fetching product {productId}");
    }

    public async Task<Answer> ExtractPlentyOfData(CancellationToken ct)
    {
        var waitTime = _randomService.NextInt(10);
        var response = Answer.Prepare($"ExtractPlentyOfData({waitTime}, {ct.ToString()})");
        await Task.Delay(waitTime, ct);
        return response.WithValue("Data extracted");
    }

}

public partial class UtilityLayerClass : IAnswerable
{
    private ServiceTierClass _serviceTier;

    public UtilityLayerClass(ServiceTierClass serviceTier)
    {
        this._serviceTier = serviceTier;
    }

    public async Task<IAnswer> GetOrderAndProductsData(int orderId, CancellationToken ct)
    {

        var response = Answer.Prepare($"GetOrderAndProductsData({orderId}, {ct.ToString()})");

        Answer resp = await TryAsync(() => _serviceTier.GetOrderData(orderId, ct), ct);
        if (!resp.IsSuccess)
        { return resp.Error($"Could not fetch order {orderId}"); }
        Answer resp2 = await TryAsync(() => _serviceTier.GetProductData(orderId, ct), ct);
        return !resp2.IsSuccess ? resp.Error($"Could not fetch product {orderId}") : response.WithValue(resp.GetValue<string>() + " " + resp2.GetValue<string>());
    }

    public async Task<Answer> SimulateLongWaitingTask(CancellationToken ct)
    {
        var response = Answer.Prepare("Simulating long waiting task");
        Answer resp = await TryAsync(() => _serviceTier.ExtractPlentyOfData(ct), ct);
        return resp.IsSuccess
            ? resp : response.Attach(resp);
    }
}
