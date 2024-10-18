using AnswerGenerator;
using static AnswerGeneratorTests.AnswerGeneratorUnitTests;

namespace AnswerGeneratorTests
{
    public static class TestSourcesProvider
    {
        public static string Source_001(ITestableGenerator generator) =>
            $$"""
              
              
                                       namespace {{TestNamespace}}
                                       {
                                           public partial class {{TestClassName}}001 : {{generator.ClassInterfaceName}}
                                           {
                                               // Empty class
                                           }
                                       }
                                       
              """;

        public static string Source_002(ITestableGenerator generator) =>
            $$"""
              
                      namespace {{TestNamespace}}
                      {
                          public partial class {{TestClassName}}002 : {{generator.ClassInterfaceName}}
                          {
                              public {{TestClassName}}002()
                              {
                              }
              
                              public {{TestClassName}}002(int value)
                              {
                              }
                          }
                      }  
                      
              """;

        public static string Source_003(ITestableGenerator generator) =>
            $$"""
                      namespace {{TestNamespace}}
                      {
                          public partial class {{TestClassName}}003 : {{generator.ClassInterfaceName}}
                          {
                              public {{TestClassName}}003({{generator.ServiceInterface}} {{generator.ConstructorServiceField}})
                              {
                              }
              
                              public {{TestClassName}}003(int value)
                              {
                              }
                          }
                      }  
              """;
        public static string Source_004(ITestableGenerator generator) =>
            $$"""
                      namespace {{TestNamespace}}
                      {
                          public partial class {{TestClassName}}004: {{generator.ClassInterfaceName}}
                          {
                              private {{generator.ServiceInterface}} _customAnswerService;
                          }
                      }
              """;

        public static string Source_005(ITestableGenerator generator) =>
            $$"""
              
                      using Answers;
              
                      namespace TestNamespace
                      {
                          public partial class {{TestClassName}}005 : {{generator.ClassInterfaceName}}
                          {
                              private {{generator.ServiceInterface}} _service1;
                              private {{generator.ServiceInterface}} _service2;
                          }
                      }
              
                      
              """;
    }
}
