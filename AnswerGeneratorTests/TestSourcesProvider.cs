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
                                           public partial class {{TestClassName}}001 : {{generator.InterfaceName}}
                                           {
                                               // Empty class
                                           }
                                       }
                                       
              """;

        public static string Source_002(ITestableGenerator generator) =>
            $$"""
              
                      namespace {{TestNamespace}}
                      {
                          public partial class {{TestClassName}}002 : {{generator.InterfaceName}}
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
                          public partial class {{TestClassName}}003 : {{generator.InterfaceName}}
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
                          public partial class {{TestClassName}}004: {{generator.InterfaceName}}
                          {
                              private {{generator.ServiceInterface}} _customAnswerService;
                          }
                      }
              """;
    }
}
