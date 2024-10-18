using System;
using System.Collections.Generic;
using System.Text;

namespace AnswerGenerator
{
    //interface for tests
    public interface ITestableGenerator
    {
        string ServiceInterface { get; }
        string ClassInterfaceName { get; }
        string ConstructorServiceField { get; }
        string DefaultAnswerServiceMemberName { get; }
    }
}
