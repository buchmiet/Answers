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
                                           public partial class {{TestClassName}} : {{generator.InterfaceName}}
                                           {
                                               // Empty class
                                           }
                                       }
                                       
              """;

        public static string Source_002(ITestableGenerator generator) =>
            $$"""
              
                      using Answers;
              
                      namespace {{TestNamespace}}
                      {
                          public partial class {{TestClassName}} : {{generator.InterfaceName}}
                          {
                              public {{TestClassName}}()
                              {
                              }
              
                              public {{TestClassName}}(int value)
                              {
                              }
                          }
                      }  
                      
              """;

        public static string Source_003(ITestableGenerator generator) =>
            $$"""
              
                      using Answers;
              
              
                      namespace {{TestNamespace}}
                      {
                          public partial class {{TestClassName}} : {{generator.InterfaceName}}
                          {
                              public {{TestClassName}}({{generator.ServiceName}} answerService)
                              {
                              }
              
                              public {{TestClassName}}(int value)
                              {
                              }
                          }
                      }  
              """;
    }
}
