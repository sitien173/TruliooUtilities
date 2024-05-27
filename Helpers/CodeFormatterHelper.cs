using CSharpier;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TruliooExtension.Helpers;

public static class CodeFormatterHelper
{
    private static readonly CodeFormatterOptions _formatOpt = new()
    {
        Width = 250,
        IndentSize = 2,
        EndOfLine = EndOfLine.LF,
        IndentStyle = IndentStyle.Tabs
    };
    
    public static string FormatCode(string code)
    {
        CodeFormatterResult format = CodeFormatter.Format(code, _formatOpt);
        
        if (!format.CompilationErrors.Any()) 
            return format.Code;
        
        string errors = string.Join(Environment.NewLine, format.CompilationErrors
            .Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
            .Select(d =>
            {
                LinePosition position = d.Location.GetLineSpan().StartLinePosition;
                return $"Line {position.Line}, Col {position.Character}: {d.Id} - {d.GetMessage()}";
            }));

        return errors;
    }
}