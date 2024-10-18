using Microsoft.CodeAnalysis;
using System;

namespace AnswerGenerator
{
    public partial class AnswerableGenerator
    {
        public enum Warnings
        {
            MultipleAnswerServiceMembers,
            ResourceFileNotFound,
            RequiredClassNotFoundInResource
        }

        private DiagnosticDescriptor WarningGenerator(Warnings warning)
        {
            return warning switch
            {
                Warnings.MultipleAnswerServiceMembers => new DiagnosticDescriptor(
                    id: "ANSWR001",
                    title: $"Multiple {ServiceInterface} members found",
                    messageFormat: $"The class {{0}} contains multiple {ServiceInterface} members, which might lead to unexpected behavior.",
                    category: "AnswerServiceGeneration",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true
                ),
                Warnings.ResourceFileNotFound => new DiagnosticDescriptor(
                    id: "ANSWR002",
                    title: "Resource file not found",
                    messageFormat: "The resource file '{0}' was not found in the assembly resources.",
                    category: "ResourceProcessing",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true
                ),
                Warnings.RequiredClassNotFoundInResource => new DiagnosticDescriptor(
                    id: "ANSWR003",
                    title: "Required class not found in resource",
                    messageFormat: "The required class '{0}' was not found in the resource file.",
                    category: "ResourceProcessing",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true
                ),
                _ => throw new NotImplementedException()
            };
        }
    }
}
