namespace AnswerGenerator
{
    partial class AnswerableGenerator
    {
        private string GenerateAnswerServiceMemberSource(string namespaceName, string className, string propertyName)=>
             namespaceName is null
                ? $$"""
                    
                                        public partial class {{className}}
                                        {
                                            private readonly {{ServiceInterface}} {{propertyName}};
                                        }
                                        
                    """
                : $$"""
                    
                                        namespace {{namespaceName}}
                                        {
                                            public partial class {{className}}
                                            {
                                               private readonly {{ServiceInterface}} {{propertyName}};
                                            }
                                        }
                                        
                    """;
        

        private string GenerateConstructorOverload_001(string className, string answerServiceMemberName) =>
            $$"""

              public partial class {{className}}
              {
                  public {{className}}({{ServiceInterface}}  {{ConstructorServiceField}})
                  {
                      {{answerServiceMemberName}} = {{ConstructorServiceField}};
                  }
              }
              """;

        private string GenerateConstructorOverload_002(string className,string parameterList,string argumentList,  string answerServiceMemberName) =>
            $$"""

              public partial class {{className}}
              {
                  public {{className}}({{parameterList}})
                      : this({{argumentList}})
                  {
                      {{answerServiceMemberName}} = {{ConstructorServiceField}};
                  }
              }
              """;
        private string GenerateConstructorOverload_003(string classBody, string namespaceName) =>
            namespaceName is null
                ? classBody
                : $$"""

                    namespace {{namespaceName}}
                    {
                        {{classBody}}
                    }
                    """;

        private string GenerateHelperMethods_001(string className, string methodsCode)=>
            $$"""
              public partial class {{className}} 
              {
                {{methodsCode}}
               }
              """;

        private string GenerateHelperMethods_002(string namespaceName, string classBody) =>
            namespaceName == null
                ? classBody
                : $$"""
                    namespace {{namespaceName}} 
                    {
                        {{classBody}}
                    }
                    """;
    }
}
