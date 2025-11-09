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
        if (!ExtractDeclarations()) return;
        CheckUsages();
        CheckUnusedVariables();
    }

    private void CheckAssignmentType(int assignPos, string varName)
    {
        for (int i = assignPos + 2; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Type == "SEPARATOR" && (token.Value == ";" || token.Value == ")") ||
                token.Type == "KEYWORD" && (token.Value == "TO" || token.Value == "DO")) break;
            if (token.Value.StartsWith("'") || token.Value.StartsWith("\""))
            {
                Console.WriteLine(Errors.Semantic($"Нельзя присвоить строку '{token.Value}' переменной '{varName}' типа INTEGER"));
                HasError = true;
                break;
            }
            if (token.Value.ToUpper() == "TRUE" || token.Value.ToUpper() == "FALSE")
            {
                Console.WriteLine(Errors.Semantic($"Нельзя присвоить булево значение '{token.Value}' переменной '{varName}' типа INTEGER"));
                HasError = true;
                break;
            }
        }
    }

    private bool ExtractDeclarations()
    {
        int pos = 0;
        while (pos < tokens.Count && !(tokens[pos].Type == "KEYWORD" && tokens[pos].Value == "VAR")) pos++;
        pos++;
        while (pos < tokens.Count)
        {
            var token = tokens[pos];
            if (token.Type == "SEPARATOR" && token.Value == ":") break;
            if (token.Type == "IDENTIFIER" && !variables.ContainsKey(token.Value))
                variables[token.Value] = new VarState { IsDeclared = true, IsUsed = false };
            if (token.Type != "SEPARATOR" || token.Value != ",") pos++;
            else pos++;
        }
        return true;
    }

    private void CheckUsages()
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Type == "KEYWORD")
            {
                switch (token.Value)
                {
                    case "READ":
                        if (i + 3 < tokens.Count && tokens[i + 1].Value == "(" && tokens[i + 2].Type == "IDENTIFIER" && tokens[i + 3].Value == ")")
                            MarkUsage(tokens[i + 2].Value);
                        break;
                    case "WRITE":
                        if (i + 2 < tokens.Count && tokens[i + 1].Value == "(")
                        {
                            int j = i + 2;
                            while (j < tokens.Count && tokens[j].Value != ")")
                            {
                                if (tokens[j].Type == "IDENTIFIER") MarkUsage(tokens[j].Value);
                                j++;
                            }
                        }
                        break;
                    case "FOR":
                        if (i + 1 < tokens.Count && tokens[i + 1].Type == "IDENTIFIER")
                        {
                            MarkUsage(tokens[i + 1].Value);
                            if (i + 3 < tokens.Count && tokens[i + 2].Value == "=") CheckExpressionForVariables(i + 3);
                            int toPos = i + 3;
                            while (toPos < tokens.Count && !(tokens[toPos].Type == "KEYWORD" && tokens[toPos].Value == "TO")) toPos++;
                            if (toPos + 1 < tokens.Count) CheckExpressionForVariables(toPos + 1);
                        }
                        break;
                }
            }
            else if (token.Type == "IDENTIFIER" && i + 1 < tokens.Count && tokens[i + 1].Value == "=")
            {
                MarkUsage(token.Value);
                CheckExpressionForVariables(i + 2);
            }
        }
    }

    private void CheckExpressionForVariables(int startPos)
    {
        for (int i = startPos; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Type == "SEPARATOR" && (token.Value == ";" || token.Value == ")") ||
                token.Type == "KEYWORD" && (token.Value == "TO" || token.Value == "DO")) break;
            if (token.Type == "IDENTIFIER") MarkUsage(token.Value);
        }
    }

    private void CheckUnusedVariables()
    {
        foreach (var variable in variables)
            if (!variable.Value.IsUsed)
            {
                Console.WriteLine(Errors.Semantic($"Объявленная переменная '{variable.Key}' не используется"));
                HasError = true;
                return;
            }
    }

    private void MarkUsage(string varName)
    {
        if (!variables.TryGetValue(varName, out var state))
        {
            Console.WriteLine(Errors.Semantic($"Использование необъявленной переменной '{varName}'"));
            HasError = true;
            return;
        }
        state.IsUsed = true;
    }
}
