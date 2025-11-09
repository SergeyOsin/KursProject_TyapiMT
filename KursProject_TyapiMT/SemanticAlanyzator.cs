using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KursProject_TyapiMT;

public class SemanticAnalyzer
{
    private readonly List<LexicalAnalyzator.Token> tokens;
    private readonly Dictionary<string, bool> idents = new();
    public bool HasError { get; private set; }

    public SemanticAnalyzer(List<LexicalAnalyzator.Token> tokens)
    {
        this.tokens = tokens;
        HasError = false;
    }

    public static bool Analyze(List<LexicalAnalyzator.Token> tokens)
    {
        var analyzer = new SemanticAnalyzer(tokens);
        analyzer.Run();
        return !analyzer.HasError;
    }

    private void Run()
    {
        foreach (var token in tokens)
        {
            if (token.Type == "IDENTIFIER")
                idents[token.Value] = false;
        }

        CheckSemantic();
    }

    private void CheckSemantic()
    {
    }

}
