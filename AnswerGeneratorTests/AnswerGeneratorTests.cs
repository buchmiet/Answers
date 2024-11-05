﻿using System;
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
using static AnswerGeneratorTests.TestSourcesProvider;
using FluentAssertions;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using System.Reflection.Emit;

namespace AnswerGeneratorTests
{
    public class AnswerGeneratorUnitTests
    {
        // Automatically detect the path to Answers.dll
       
        public static string TestNamespace = "TestNamespace";
        public static string TestClassName = "TestClass";
      
        private readonly PortableExecutableReference _answersReference;
        private readonly PortableExecutableReference _netstandardReference;
        public AnswerGeneratorUnitTests()
        {
            _answersReference = MetadataReference.CreateFromFile(GetAnswersDllPath());
            var netstandardPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll");
            _netstandardReference = MetadataReference.CreateFromFile(netstandardPath);
        }


        [Fact]
        public void ClassWithNoConstructorAndNoIAnswerServiceMember_ShouldAddFieldAndConstructor_001()
        {
            var generator = new AnswerableGenerator();


            var (assembly, diagnostics) = CompileAndRunGenerator(Source_001(generator), generator);

            // Verify no errors
            Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            // Use reflection to verify the generated members
            var testClassType = assembly.GetType($"{TestNamespace}.{TestClassName}001");
            Assert.NotNull(testClassType);
            var fields = testClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var answerServiceFields = fields.Where(p => p.FieldType.FullName == generator.ServiceInterface);
            Assert.True(answerServiceFields.Any());
            bool passed = false;
            foreach (var answerServiceField in answerServiceFields)
            {
                if (!answerServiceField.IsInitOnly) continue;
                passed = true;
                break;
            }
            Assert.True(passed);

            // Check for the constructor that accepts IAnswerService
            var ctor = testClassType.GetConstructor([typeof(IAnswerService)]);

            Assert.NotNull(ctor);
        }


        [Fact]
        public void ClassWithExistingConstructors_ShouldAddOverloads_002()
        {
            var generator = new AnswerableGenerator();


            // Compile and run the generator
            var (assembly, diagnostics) = CompileAndRunGenerator(Source_002(generator), generator);
            var ax = Source_001(generator);
            var bx = $"{TestNamespace}.{TestClassName}002";
            // Verify no errors
            Assert.Empty(diagnostics.Where(d => d.Severity == Error));

            var testClassType = assembly.GetType($"{TestNamespace}.{TestClassName}002");
            Assert.NotNull(testClassType);

            var constructors = testClassType.GetConstructors();

            // Original constructors
            Assert.Contains(constructors, c => c.GetParameters().Length == 0);
            Assert.Contains(constructors, c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(int));

            // Generated overloads
            Assert.Contains(constructors, c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.FullName == generator.ServiceInterface;
            });

            Assert.Contains(constructors, c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(int) &&
                       parameters[1].ParameterType.FullName == generator.ServiceInterface;
            });
        }

        [Fact]
        public void ClassWithConstructorHavingIAnswerService_ShouldNotAddOverload_003()
        {
            var generator = new AnswerableGenerator();
            // Compile and run the generator
            var (assembly, diagnostics) = CompileAndRunGenerator(Source_003(generator), generator);

            // Verify no errors
            Assert.Empty(diagnostics.Where(d => d.Severity == Error));

            var testClassType = assembly.GetType($"{TestNamespace}.{TestClassName}003");
            Assert.NotNull(testClassType);

            var constructors = testClassType.GetConstructors();

            // Ensure no overload is created for the constructor that already has IAnswerService
            Assert.Single(constructors.Where(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.FullName == generator.ServiceInterface;
            }));

            // Check for overload for constructor without IAnswerService
            Assert.Contains(constructors, c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(int) &&
                       parameters[1].ParameterType.FullName == generator.ServiceInterface;
            });
        }

        [Fact]
        public void ClassWithSingleIAnswerServiceMember_ShouldUseExistingMember_004()
        {
            var generator = new AnswerableGenerator();
            var (assembly, diagnostics) = CompileAndRunGenerator(Source_004(generator), generator);

            // Verify no errors
            Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            var testClassType = assembly.GetType($"{TestNamespace}.{TestClassName}004");
            Assert.NotNull(testClassType);

            // Ensure the generator did not add a new property
            var fields = testClassType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.Single(fields);
            Assert.Equal("_customAnswerService", fields[0].Name);
        
            // Check for constructor that assigns to the existing field
            var constructor = testClassType.GetConstructor([typeof(IAnswerService)]);
            Assert.NotNull(constructor);
        }

        [Fact]
        public void ClassWithMultipleIAnswerServiceMembers_ShouldEmitError005()
        {

            var generator = new AnswerableGenerator();
            var (assembly, diagnostics) = CompileAndRunGenerator(Source_005(generator), generator);

            // Verify that an error diagnostic is produced
            var errorDiagnostics = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
            Assert.NotEmpty(errorDiagnostics);

            // Optionally check the diagnostic message
            Assert.Contains(errorDiagnostics, d => d.GetMessage().Contains($"multiple {generator.ServiceInterface} members"));
        }

        
        // Helper method to compile source code and run the generator
        private (Assembly assembly, ImmutableArray<Diagnostic> diagnostics) CompileAndRunGenerator(string source, IIncrementalGenerator generator)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            List<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .Cast<MetadataReference>()
                .ToList();

            references.Add(_answersReference);
            references.Add(_netstandardReference);

            var compilation = CSharpCompilation.Create("TestAssembly",
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            

            // Run the generator
            CSharpGeneratorDriver.Create(generator)
                .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            // Emit the assembly to a stream
            using var ms = new MemoryStream();
            var emitResult = outputCompilation.Emit(ms);

            if (!emitResult.Success)
            {
                var errors = string.Join(Environment.NewLine, emitResult.Diagnostics.Where(d => d.Severity == Error));
                throw new InvalidOperationException($"Compilation failed: {errors}");
            }

            ms.Seek(0, SeekOrigin.Begin);
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
