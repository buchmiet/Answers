using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AnswerGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Answers;
using System.Runtime.InteropServices;
using FluentAssertions;

namespace AnswerGeneratorTests
{
    public class AnswerGeneratorUnitTests
    {
        // Automatically detect the path to Answers.dll
       
        private const string TestNamespace = "TestNamespace";
        private const string TestClassName = "TestClass";
        private readonly string _answersDllPath;
        public AnswerGeneratorUnitTests()
        {
            _answersDllPath = GetAnswersDllPath();
        }

        [Fact]
        public void ClassWithNoConstructorAndNoIAnswerServiceMember_ShouldAddFieldAndConstructor()
        {
            var generator = new AnswerableGenerator();
            var source = $@"

                         namespace {TestNamespace}
                         {{
                             public partial class {TestClassName} : {generator.InterfaceName}
                             {{
                                 // Empty class
                             }}
                         }}
                         ";

            // Compile and run the generator
            var (assembly, diagnostics) = CompileAndRunGenerator(source, generator);

            // Verify no errors
            Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            // Use reflection to verify the generated members
            var testClassType = assembly.GetType($"{TestNamespace}.{TestClassName}");
            Assert.NotNull(testClassType);
            var fields = testClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var answerServiceFields = fields.Where(p => p.FieldType.FullName == generator.ServiceName);
            Assert.True(answerServiceFields.Any());
            bool passed = false;
            foreach (var answerServiceField in answerServiceFields)
            {
                if (answerServiceField.IsInitOnly)
                {
                    passed = true;
                    break;
                }
            }
            Assert.True(passed);

            // Check for the constructor that accepts IAnswerService
            var ctor = testClassType.GetConstructor([typeof(Answers.IAnswerService)]);

            Assert.NotNull(ctor);
        }

     

        [Fact]
        public void ClassWithExistingConstructors_ShouldAddOverloads()
        {
            var generator = new AnswerableGenerator();
            var source = $@"
        using Answers;

        namespace {TestNamespace}
        {{
            public partial class {TestClassName} : {generator.InterfaceName}
            {{
                public {TestClassName}()
                {{
                }}

                public {TestClassName}(int value)
                {{
                }}
            }}
        }}  
        ";

            // Compile and run the generator
            var (assembly, diagnostics) = CompileAndRunGenerator(source, generator);

            // Verify no errors
            Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            var testClassType = assembly.GetType($"{TestNamespace}.{TestClassName}");
            Assert.NotNull(testClassType);

            var constructors = testClassType.GetConstructors();

            // Original constructors
            Assert.Contains(constructors, c => c.GetParameters().Length == 0);
            Assert.Contains(constructors, c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(int));

            // Generated overloads
            Assert.Contains(constructors, c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.FullName ==generator.ServiceName;
            });

            Assert.Contains(constructors, c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(int) &&
                       parameters[1].ParameterType.FullName ==generator.ServiceName;
            });
        }

        [Fact]
        public void ClassWithConstructorHavingIAnswerService_ShouldNotAddOverload()
        {
            var generator = new AnswerableGenerator();
            var source = $@"
        using Answers;


        namespace {TestNamespace}
        {{
            public partial class {TestClassName} : {generator.InterfaceName}
            {{
                public {TestClassName}({generator.ServiceName} answerService)
                {{
                }}

                public {TestClassName}(int value)
                {{
                }}
            }}
        }}  
        ";

            // Compile and run the generator
            var (assembly, diagnostics) = CompileAndRunGenerator(source, generator);

            // Verify no errors
            Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            var testClassType = assembly.GetType($"{TestNamespace}.{TestClassName}");
            Assert.NotNull(testClassType);

            var constructors = testClassType.GetConstructors();

            // Ensure no overload is created for the constructor that already has IAnswerService
            Assert.Single(constructors.Where(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.FullName ==generator.ServiceName;
            }));

            // Check for overload for constructor without IAnswerService
            Assert.Contains(constructors, c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(int) &&
                       parameters[1].ParameterType.FullName ==generator.ServiceName;
            });
        }

        //[Fact]
        //public void ClassWithSingleIAnswerServiceMember_ShouldUseExistingMember()
        //{
        //    var source = @"
        //using Answers;

        //namespace TestNamespace
        //{
        //    public partial class TestClass : IAnswerable
        //    {
        //        private Answers.IAnswerService _customAnswerService;
        //    }
        //}
       
        //";

        //    // Compile and run the generator
        //    var (assembly, diagnostics) = CompileAndRunGenerator(source, GetAnswersDllPath());

        //    // Verify no errors
        //    Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        //    var testClassType = assembly.GetType("TestNamespace.TestClass");
        //    Assert.NotNull(testClassType);

        //    // Ensure the generator did not add a new field
        //    var fields = testClassType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        //    Assert.Single(fields);
        //    Assert.Equal("_customAnswerService", fields[0].Name);

        //    // Check for constructor that assigns to the existing field
        //    var constructor = testClassType.GetConstructor(new Type[] { assembly.GetType("Answers.IAnswerService") });
        //    Assert.NotNull(constructor);
        //}

        //        [Fact]
        //        public void ClassWithMultipleIAnswerServiceMembers_ShouldEmitError()
        //        {
        //            var source = @"
        //using Answers;

        //namespace TestNamespace
        //{
        //    public partial class TestClass : IAnswerable
        //    {
        //        private Answers.IAnswerService _service1;
        //        private Answers.IAnswerService _service2;
        //    }
        //}

        //namespace Answers
        //{
        //    public interface IAnswerable { }
        //    public interface IAnswerService { }
        //}
        //";

        //            // Compile and run the generator
        //            var (_, diagnostics) = CompileAndRunGenerator(source);

        //            // Verify that an error diagnostic is produced
        //            var errorDiagnostics = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        //            Assert.NotEmpty(errorDiagnostics);

        //            // Optionally check the diagnostic message
        //            Assert.Contains(errorDiagnostics, d => d.GetMessage().Contains("multiple IAnswerService members found"));
        //        }

        // Helper method to compile source code and run the generator
        //private (Assembly assembly, ImmutableArray<Diagnostic> diagnostics) CompileAndRunGenerator(string source)
        //{
        //    var syntaxTree = CSharpSyntaxTree.ParseText(source);

        //    var references = AppDomain.CurrentDomain.GetAssemblies()
        //        .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
        //        .Select(a => MetadataReference.CreateFromFile(a.Location))
        //        .Cast<MetadataReference>()
        //        .ToList();

        //    var compilation = CSharpCompilation.Create("TestAssembly",
        //        new[] { syntaxTree },
        //        references,
        //        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        //    // Create an instance of the generator
        //    var generator = new AnswerableGenerator();

        //    // Run the generator
        //    CSharpGeneratorDriver.Create(generator)
        //        .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        //    // Emit the assembly to a stream
        //    using var ms = new System.IO.MemoryStream();
        //    var emitResult = outputCompilation.Emit(ms);

        //    if (!emitResult.Success)
        //    {
        //        var errors = string.Join(Environment.NewLine, emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        //        throw new InvalidOperationException($"Compilation failed: {errors}");
        //    }

        //    ms.Seek(0, System.IO.SeekOrigin.Begin);
        //    var assembly = Assembly.Load(ms.ToArray());

        //    return (assembly, diagnostics);
        //}

        // Helper method to compile source code and run the generator
        private (Assembly assembly, ImmutableArray<Diagnostic> diagnostics) CompileAndRunGenerator(string source, IIncrementalGenerator generator)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            // Reference to the Answers.dll
            var answersReference = MetadataReference.CreateFromFile(_answersDllPath);

            // Reference to netstandard.dll
            var netstandardPath = Path.Combine(
                RuntimeEnvironment.GetRuntimeDirectory(),
                "netstandard.dll");
            var netstandardReference = MetadataReference.CreateFromFile(netstandardPath);



            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Cast<MetadataReference>()
                .ToList();


            references.Add(answersReference);
            references.Add(netstandardReference);

            var compilation = CSharpCompilation.Create("TestAssembly",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // Create an instance of the generator
            

            // Run the generator
            CSharpGeneratorDriver.Create(generator)
                .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            // Emit the assembly to a stream
            using var ms = new System.IO.MemoryStream();
            var emitResult = outputCompilation.Emit(ms);

            if (!emitResult.Success)
            {
                var errors = string.Join(Environment.NewLine, emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
                throw new InvalidOperationException($"Compilation failed: {errors}");
            }

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            return (assembly, diagnostics);
        }

        private string GetAnswersDllPath()
        {
            // Find the solution directory by walking up the directory tree
            string solutionDirectory = Directory.GetCurrentDirectory();
            while (!Directory.GetFiles(solutionDirectory, "*.sln").Any())
            {
                solutionDirectory = Directory.GetParent(solutionDirectory)?.FullName;
                if (solutionDirectory == null)
                {
                    throw new InvalidOperationException("Cannot find the solution directory.");
                }
            }

            // Construct the path to the Answers.dll in the bin directory
            string answersDllPath = Path.Combine(solutionDirectory, "Answers", "bin", "Debug", "netstandard2.0", "Answers.dll");

            if (!File.Exists(answersDllPath))
            {
                throw new FileNotFoundException("Answers.dll not found. Make sure the Answers project is built.", answersDllPath);
            }

            return answersDllPath;
        }

    }
}
