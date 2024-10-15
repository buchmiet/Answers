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




