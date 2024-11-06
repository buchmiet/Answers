using System;
using System.Collections.Generic;

namespace Answers;

public class MessageAggregator
{
    private string _connector = " > ";
    public List<string> Actions { get; } = new();

    public string Message => string.Join(_connector, Actions);

    public void SetConnector(string connector) => _connector = connector;

    public void AddAction(string action)
    {
        switch (action)
        {
            case null:
                throw new ArgumentNullException(nameof(action));
            case "":
                throw new ArgumentException("Action cannot be empty", nameof(action));
            default:
                Actions.Add(action);
                break;
        }
    } 

    public void AddActions(IEnumerable<string> actions) => Actions.AddRange(actions);
}