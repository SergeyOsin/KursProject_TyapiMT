using System;
using System.Collections.Generic;
using System.Linq;
using KursProject_TyapiMT;

public class SyntaxAnalyzer
{
    private List<LexicalAnalyzator.Token> tokens;
    private int position = 0;
    public bool HasError { get; private set; } = false;
    private LexicalAnalyzator.Token Current => position < tokens.Count ? tokens[position] : null;
    private bool IsEnd => position >= tokens.Count;
    private readonly Dictionary<string, Action> keywordHandlers;

    public SyntaxAnalyzer(List<LexicalAnalyzator.Token> tokens)
    {
        this.tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        keywordHandlers = new Dictionary<string, Action>
        {
            ["VAR"] = ParseVarDeclaration,
            ["INTEGER"] = ParseType,
            ["BEGIN"] = ParseBegin,
            ["READ"] = ParseScan,
            ["FOR"] = ParseForLoop,
            ["WRITE"] = ParsePrint,
            ["END"] = ParseEnd
        };
    }

    public static bool Analyze(List<LexicalAnalyzator.Token> tokens)
    {
        var analyzer = new SyntaxAnalyzer(tokens);
        analyzer.ParseProgram();
        return !analyzer.HasError;
    }

    public void ParseProgram()
    {
        if (!tokens.Any(t => t.Type == "KEYWORD" && t.Value == "BEGIN"))
        {
            Console.WriteLine(Errors.Syntax("Программа должна начинаться с BEGIN"));
            HasError = true;
            return;
        }

        if (!tokens.Any(t => t.Type == "KEYWORD" && t.Value == "END"))
        {
            Console.WriteLine(Errors.Syntax("Программа должна завершаться END"));
            HasError = true;
            return;
        }

        if (!tokens.Any(t => t.Type == "KEYWORD" && t.Value == "VAR"))
        {
            Console.WriteLine(Errors.Syntax("Программа должна содержать объявление переменных с помощью VAR"));
            HasError = true;
            return;
        }

        if (!tokens.Any(t => t.Type == "KEYWORD" && t.Value == "INTEGER"))
        {
            Console.WriteLine(Errors.Syntax("Программа должна содержать объявление типа INTEGER"));
            HasError = true;
            return;
        }

        bool hasVarDeclaration = false;
        while (Current != null && Current.Type == "KEYWORD" && Current.Value == "VAR")
        {
            hasVarDeclaration = true;
            ParseVarDeclaration();
            if (HasError) return;
        }

        if (!hasVarDeclaration)
        {
            Console.WriteLine(Errors.Syntax("Отсутствует объявление переменных VAR"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("KEYWORD", "BEGIN"))
        {
            HasError = true;
            return;
        }

        while (Current != null && !(Current.Type == "KEYWORD" && Current.Value == "END"))
        {
            ParseStatement();
            if (HasError) return;
        }

        if (!CheckAndMove("KEYWORD", "END"))
        {
            HasError = true;
            return;
        }
        if (!IsEnd)
        {
            Console.WriteLine(Errors.Syntax("После END не должно быть токенов"));
            HasError = true;
        }
    }

    private void ParseStatement()
    {
        if (IsEnd)
        {
            Console.WriteLine(Errors.Syntax("Незавершенное выражение"));
            HasError = true;
            return;
        }

        if (Current.Type == "KEYWORD")
        {
            switch (Current.Value)
            {
                case "READ":
                    ParseScan();
                    break;
                case "FOR":
                    ParseForLoop();
                    break;
                case "WRITE":
                    ParsePrint();
                    break;
                default:
                    Console.WriteLine(Errors.Syntax($"Недопустимое ключевое слово в операторе: {Current.Value}"));
                    HasError = true;
                    break;
            }
        }
        else if (Current.Type == "IDENTIFIER")
        {
            ParseAssignment();
        }
    }

    private void ParseVarDeclaration()
    {
        if (!CheckAndMove("KEYWORD", "VAR"))
        {
            Console.WriteLine(Errors.Syntax("Ожидалось ключевое слово VAR"));
            HasError = true;
            return;
        }

        // Проверяем, что есть хотя бы один идентификатор
        if (Current == null || Current.Type != "IDENTIFIER")
        {
            Console.WriteLine(Errors.Syntax("После VAR должен быть хотя бы один идентификатор"));
            HasError = true;
            return;
        }

        // Парсим список идентификаторов
        bool hasIdentifiers = false;
        do
        {
            if (!CheckAndMove("IDENTIFIER"))
            {
                Console.WriteLine(Errors.Syntax("Ожидался идентификатор в списке переменных"));
                HasError = true;
                return;
            }

            hasIdentifiers = true;
        } while (CheckAndMove("SEPARATOR", ","));

        // Проверяем, что после списка идентификаторов идет двоеточие
        if (!CheckAndMove("SEPARATOR", ":"))
        {
            Console.WriteLine(Errors.Syntax("После списка переменных должно быть двоеточие"));
            HasError = true;
            return;
        }

        // Проверяем, что указан тип INTEGER
        if (!CheckAndMove("KEYWORD", "INTEGER"))
        {
            Console.WriteLine(Errors.Syntax("После двоеточия должен быть указан тип INTEGER"));
            HasError = true;
            return;
        }

        // Проверяем точку с запятой в конце
        if (!CheckAndMove("SEPARATOR", ";"))
        {
            Console.WriteLine(Errors.Syntax("Отсутствует ; после объявления переменных"));
            HasError = true;
        }
    }

    private void ParseType()
    {
        if (!CheckAndMove("KEYWORD", "INTEGER"))
        {
            Console.WriteLine(Errors.Syntax("Ожидался тип INTEGER"));
            HasError = true;
        }
    }

    private void ParseBegin()
    {
        if (!CheckAndMove("KEYWORD", "BEGIN"))
        {
            Console.WriteLine(Errors.Syntax("Ожидалось ключевое слово BEGIN"));
            HasError = true;
        }
    }

    private void ParseEnd()
    {
        if (!CheckAndMove("KEYWORD", "END"))
        {
            Console.WriteLine(Errors.Syntax("Ожидалось ключевое слово END"));
            HasError = true;
        }
    }

    private void ParseScan()
    {
        if (!CheckAndMove("KEYWORD", "READ"))
        {
            Console.WriteLine(Errors.Syntax("Ожидалось ключевое слово READ"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("SEPARATOR", "("))
        {
            Console.WriteLine(Errors.Syntax("После READ должна быть ("));
            HasError = true;
            return;
        }

        if (!CheckAndMove("IDENTIFIER"))
        {
            Console.WriteLine(Errors.Syntax("После ( должен быть идентификатор"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("SEPARATOR", ")"))
        {
            Console.WriteLine(Errors.Syntax("После идентификатора должна быть )"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("SEPARATOR", ";"))
        {
            Console.WriteLine(Errors.Syntax("После ) должна быть ;"));
            HasError = true;
        }
    }

    private void ParseForLoop()
    {
        if (!CheckAndMove("KEYWORD", "FOR"))
        {
            Console.WriteLine(Errors.Syntax("Ожидалось ключевое слово FOR"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("IDENTIFIER"))
        {
            Console.WriteLine(Errors.Syntax("После FOR должен быть идентификатор"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("OPERATOR", "="))
        {
            Console.WriteLine(Errors.Syntax("После идентификатора должен быть ="));
            HasError = true;
            return;
        }
        ParseExpression();
        if (!CheckAndMove("KEYWORD", "TO"))
        {
            Console.WriteLine(Errors.Syntax("После начального значения должно быть TO"));
            HasError = true;
            return;
        }

        ParseExpression();

        if (!CheckAndMove("KEYWORD", "DO"))
        {
            Console.WriteLine(Errors.Syntax("После конечного значения должно быть DO"));
            HasError = true;
            return;
        }
        bool hasBody = false;
        while (Current != null && !(Current.Type == "KEYWORD" && Current.Value == "END_FOR"))
        {
            hasBody = true;
            ParseStatement();
            if (HasError) return;
        }

        if (!hasBody)
        {
            Console.WriteLine(Errors.Syntax("Тело цикла FOR не может быть пустым"));
            HasError = true;
            return;
        }

        if (Current == null || Current.Type != "KEYWORD" || Current.Value != "END_FOR")
        {
            Console.WriteLine(Errors.Syntax("Цикл FOR должен завершаться END_FOR"));
            HasError = true;
            return;
        }
        position++;  

        if (!CheckAndMove("SEPARATOR", ";"))
        {
            Console.WriteLine(Errors.Syntax("После END_FOR должна быть ;"));
            HasError = true;
        }
    }

    private void ParsePrint()
    {
        if (!CheckAndMove("KEYWORD", "WRITE"))
        {
            Console.WriteLine(Errors.Syntax("Ожидалось ключевое слово WRITE"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("SEPARATOR", "("))
        {
            Console.WriteLine(Errors.Syntax("После WRITE должна быть ("));
            HasError = true;
            return;
        }

        ParseExpression();

        while (CheckAndMove("SEPARATOR", ","))
        {
            ParseExpression();
        }

        if (!CheckAndMove("SEPARATOR", ")"))
        {
            Console.WriteLine(Errors.Syntax("После аргументов должна быть )"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("SEPARATOR", ";"))
        {
            Console.WriteLine(Errors.Syntax("После ) должна быть ;"));
            HasError = true;
        }
    }

    private void ParseAssignment()
    {
        if (!CheckAndMove("IDENTIFIER"))
        {
            Console.WriteLine(Errors.Syntax("Ожидался идентификатор"));
            HasError = true;
            return;
        }

        if (!CheckAndMove("OPERATOR", "="))
        {
            Console.WriteLine(Errors.Syntax("После идентификатора должен быть ="));
            HasError = true;
            return;
        }

        ParseExpression();

        if (!CheckAndMove("SEPARATOR", ";"))
        {
            Console.WriteLine(Errors.Syntax("После выражения должна быть ;"));
            HasError = true;
        }
    }

    private void ParseExpression()
    {
        if (IsEnd)
        {
            Console.WriteLine(Errors.Syntax("Незавершенное выражение"));
            HasError = true;
            return;
        }

        ParseTerm();

        while (Current != null && Current.Type == "OPERATOR" &&
               (Current.Value == "+" || Current.Value == "-" || Current.Value == "*" || Current.Value == "/"))
        {
            CheckAndMove("OPERATOR");
            ParseTerm();
        }
    }

    private void ParseTerm()
    {
        if (IsEnd)
        {
            Console.WriteLine(Errors.Syntax("Незавершенный терм в выражении"));
            HasError = true;
            return;
        }

        if (Current.Type == "IDENTIFIER" || Current.Type == "NUMBER")
        {
            CheckAndMove(Current.Type);
        }
        else if (Current.Type == "SEPARATOR" && Current.Value == "(")
        {
            CheckAndMove("SEPARATOR", "(");
            ParseExpression();
            if (!CheckAndMove("SEPARATOR", ")"))
            {
                Console.WriteLine(Errors.Syntax("Незакрытая скобка"));
                HasError = true;
                return;
            }
        }
        else
        {
            HasError = true;
        }
    }

    private bool CheckAndMove(string type, string value = null)
    {
        if (IsEnd)
        {
            HasError = true;
            return false;
        }

        if (Current.Type == type && (value == null || Current.Value == value))
        {
            position++;
            return true;
        }
        return false;
    }
}
