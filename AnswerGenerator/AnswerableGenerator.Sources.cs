namespace AnswerGenerator
{
    partial class AnswerableGenerator
    {
        public static class SourceInjector
        {
            public static string GenerateAnswerServiceMemberSource(string opening,string closing, string serviceInterface,string namespaceName, string className, string propertyName) =>
             namespaceName is null
                ? $$"""
                    
                    {{opening}}
                                        public partial class {{className}}
                                        {
                                            private readonly {{serviceInterface}} {{propertyName}};
                                        }
                                        {{closing}}
                                        
                    """
                : $$"""
                    
                                        namespace {{namespaceName}}
                                        {
                                        {{opening}}
                                            public partial class {{className}}
                                            {
                                               private readonly {{serviceInterface}} {{propertyName}};
                                            }
                                            {{closing}}
                                        }
                                        
                    """;


            public static string GenerateConstructorOverload_001(string ServiceInterface,string ConstructorServiceField, string className, string answerServiceMemberName) =>
                $$"""

              public partial class {{className}}
              {
                  public {{className}}({{ServiceInterface}}  {{ConstructorServiceField}})
                  {
                      {{answerServiceMemberName}} = {{ConstructorServiceField}};
                  }
              }
              """;

            public static string GenerateConstructorOverload_002(string constructorServiceField, string className, string parameterList, string argumentList, string answerServiceMemberName) =>
                $$"""

              public partial class {{className}}
              {
                  public {{className}}({{parameterList}})
                      : this({{argumentList}})
                  {
                      {{answerServiceMemberName}} = {{constructorServiceField}};
                  }
              }
              """;
            public static string GenerateConstructorOverload_003(string opening, string closing, string classBody, string namespaceName) =>
                namespaceName is null
                    ? classBody
                    : $$"""

                    namespace {{namespaceName}}
                    {
                    {{opening}}
                        {{classBody}}
                        {{closing}}
                    }
                    """;

            public static string GenerateHelperMethods_001(string className, string methodsCode) =>
                $$"""
              public partial class {{className}} 
              {
                {{methodsCode}}
               }
              """;

            public static string GenerateHelperMethods_002(string opening, string closing, string namespaceName, string classBody) =>
                namespaceName == null
                    ? classBody
                    : $$"""
                    namespace {{namespaceName}} 
                    {
                    {{opening}}
                        {{classBody}}
                        {{closing}}
                    }
                    """;
        }
    }
        
}
