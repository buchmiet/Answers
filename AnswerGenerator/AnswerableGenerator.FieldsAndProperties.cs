using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnswerGenerator
{
    partial class AnswerableGenerator
    {

        public string ServiceInterface => "Answers.IAnswerService";
        public string InterfaceName => "Answers.IAnswerable";
        public string ConstructorServiceField => "answerService";

        public string DefaultAnswerServiceMemberName => "_answerService";

        private readonly HashSet<string> _processedClasses = [];
        private List<string> _helperMethods = new();
        private static readonly List<string> ResourceNames =
            ["AnswerGenerator.TryAsyncClass.cs", "AnswerGenerator.TryClass.cs"];

        private readonly List<string> _classesInResources = ResourceNames
            .Select(name => name.Split('.')[1]) 
            .ToList();

        public enum SourceMethod
        {
            Constructor,
            HelperMethod,
            AnswerServiceMember
        }


    }
}
