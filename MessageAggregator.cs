using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Answers
{
    public class MessageAggregator
    {
        private static string _connector = " > ";
        public List<string> Actions { get; } = new List<string>();

        public string Message => string.Join(_connector, Actions);

        public static void SetConnector(string connector) => _connector = connector;

        public void AddAction(string action) => Actions.Add(action);
        public void AddActions(IEnumerable<string> actions) => Actions.AddRange(actions);
    }
}
