using System;
using System.Collections.Generic;
using KursProject_TyapiMT;

public class SemanticAnalyzer
{
    private readonly List<LexicalAnalyzator.Token> tokens;
    private readonly Dictionary<string, VarState> variables = new();
    public bool HasError { get; private set; }

    private class VarState
    {
        public bool IsDeclared { get; set; }
        public bool IsUsed { get; set; }
        public bool IsInitialized { get; set; }
    }

    public SemanticAnalyzer(List<LexicalAnalyzator.Token> tokens)
    {
        this.tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
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
        ExtractDeclarations();
        if (HasError) return;
        CheckUsages();
        if (HasError) return;
    }

    private void ExtractDeclarations()
    {
        int pos = 0;
        while (pos < tokens.Count && !(tokens[pos].Type == "KEYWORD" && tokens[pos].Value == "VAR")) pos++;
        pos++;
        while (pos < tokens.Count && !(tokens[pos].Type == "SEPARATOR" && tokens[pos].Value == ":"))
        {
            if (tokens[pos].Type == "IDENTIFIER" && !variables.ContainsKey(tokens[pos].Value))
                variables[tokens[pos].Value] = new VarState { IsDeclared = true, IsUsed = false, IsInitialized = false };
            pos++;
        }
    }
    private void CheckUsages()
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Type == "IDENTIFIER")
            {
                if (i + 1 < tokens.Count && tokens[i + 1].Value == "=")
                {
                    CheckDeclared(token.Value);
                    if (HasError) return;
                    if (variables.TryGetValue(token.Value, out var state))
                    {
                        state.IsInitialized = true;
                        state.IsUsed = true;
                    }
                    CheckExpressionForVariables(i + 2, allowStrings: false);
                    if (HasError) return;
                }
                else if (i > 0 && tokens[i - 1].Value == "(" &&
                         (tokens[i - 2].Value == "READ" || tokens[i - 2].Value == "WRITE"))
                {
                    if (tokens[i - 2].Value == "READ")
                    {
                        CheckDeclared(token.Value);
                        if (HasError) return;
                        if (variables.TryGetValue(token.Value, out var state))
                            state.IsInitialized = true;
                    }
                    else 
                    {
                        CheckInitialized(token.Value);
                        if (HasError) return;
                    }
                }
            }
            else if (token.Type == "KEYWORD")
            {
                if (token.Value == "FOR" && i + 1 < tokens.Count && 
                    tokens[i + 1].Type == "IDENTIFIER")
                {
                    CheckDeclared(tokens[i + 1].Value);
                    if (HasError) return;
                    if (variables.TryGetValue(tokens[i + 1].Value, out var state))
                        state.IsUsed = true;
                    CheckExpressionForVariables(i + 3, allowStrings: false);
                    if (HasError) return;
                    int toPos = i + 3;
                    while (toPos < tokens.Count && !(tokens[toPos].Type == "KEYWORD" && tokens[toPos].Value == "TO")) toPos++;
                    if (toPos + 1 < tokens.Count) CheckExpressionForVariables(toPos + 1, allowStrings: false);
                    if (HasError) return;
                }
            }
        }
    }

    private void CheckExpressionForVariables(int startPos, bool allowStrings = false)
    {
        for (int i = startPos; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Type == "SEPARATOR" && (token.Value == ";" || token.Value == ")") ||
                token.Type == "KEYWORD" && (token.Value == "TO" || token.Value == "DO")) break;
            if (token.Type == "IDENTIFIER")
            {
                CheckInitialized(token.Value);
                if (HasError) return;
            }
           
        }
    }
    
    private void CheckDeclared(string varName)
    {
        if (!variables.ContainsKey(varName))
        {
            Console.WriteLine(Errors.Semantic($"Использование необъявленной переменной '{varName}'"));
            HasError = true;
        }
    }

    private void CheckInitialized(string varName)
    {
        if (!variables.TryGetValue(varName, out var state) || !state.IsInitialized)
        {
            Console.WriteLine(Errors.Semantic($"Использование неинициализированной переменной '{varName}'"));
            HasError = true;
        }
        else
        {
            state.IsUsed = true;
        }
    }
}
