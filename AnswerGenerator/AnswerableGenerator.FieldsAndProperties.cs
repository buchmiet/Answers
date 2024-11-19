using System.Collections.Generic;
using System.Linq;

namespace AnswerGenerator
{
    partial class AnswerableGenerator
    {

        public  string ServiceInterface => "Answers.IAnswerService";
        public string ClassInterfaceName => "Answers.IAnswerable";
        public string ConstructorServiceField => "answerService";

        public string DefaultAnswerServiceMemberName => "_answerService";

        private readonly HashSet<string> _processedClasses = [];
        private readonly List<string> _helperMethods = new();
        private static readonly List<string> ResourceNames =
            ["AnswerGenerator.TryAsyncClass.cs", "AnswerGenerator.TryClass.cs"];

        private readonly List<string> _classesInResources = ResourceNames
            .Select(name => name.Split('.')[1]) 
            .ToList();

    }
}
