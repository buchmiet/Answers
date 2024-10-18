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
