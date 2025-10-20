using System.Text.RegularExpressions;

namespace KursProject_TyapiMT
{
    public class LexicalAnalyzator
    {
        private const int MAXLENIDEN = 12;
        private List<string> code;
        private List<string> keywords { get; set; }
        private List<string> UnarOp { get; set; }
        private List<string> BinarOp { get; set; }
        private List<string> ident { get; set; }
        private List<Token> tokens { get; set; }
        private bool hasError = false;

        public LexicalAnalyzator(List<string> _code)
        {
            code = _code;
            keywords = new List<string>()
            {
                "VAR", "INTEGER", "BEGIN", "READ", "FOR", "TO", "DO", "END_FOR", "WRITE", "END"
            };
            UnarOp = new List<string>() { "-" };
            BinarOp = new List<string>() { "+", "-", "*" };
            ident = new List<string>();
            tokens = new List<Token>();
        }

        public class Token
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public int Line { get; set; }
            public int Position { get; set; }

            public Token(string type, string value, int line, int position)
            {
                Type = type;
                Value = value;
                Line = line;
                Position = position;
            }

            public override string ToString()
            {
                return $"{Type} '{Value}' (строка {Line})";
            }
        }

        public void Analyze()
        {
            tokens.Clear();
            hasError = false;

            if (code.Count == 0)
            {
                PrintError(new Errors(0, "Пустой входной файл"));
                return;
            }

            AnalyzeFirstLine();
            if (hasError) return;

            for (int i = 1; i < code.Count; i++)
            {
                AnalyzeLine(code[i], i + 1);
                if (hasError) return;
            }

            CheckOperators();
            if (hasError) return;

            PrintResults();
        }

        private void PrintError(Errors error)
        {
            hasError = true;
            Console.WriteLine(error.ToString());
        }

        private void AnalyzeFirstLine()
        {
            string startStr = code[0].Trim();
            
            if (!startStr.ToUpper().StartsWith("VAR"))
            {
                PrintError(new Errors(1, "Программа должна начинаться с VAR"));
                return;
            }

            tokens.Add(new Token("KEYWORD", "VAR", 1, 0));
            
            int index = startStr.IndexOf(':');
            if (index == -1)
            {
                PrintError(new Errors(1, "Отсутствует символ ':' после VAR"));
                return;
            }

            string idenPart = startStr.Substring(3, index - 3).Trim();
            string[] identifiers = idenPart.Split(',');

            foreach (string id in identifiers)
            {
                string cleanId = id.Trim();
                if (!string.IsNullOrEmpty(cleanId))
                {
                    if (cleanId.Length > MAXLENIDEN)
                    {
                        PrintError(new Errors(1, $"Превышена максимальная длина идентификатора: {cleanId}"));
                        return;
                    }

                    if (!Regex.IsMatch(cleanId, @"^[a-zA-Z][a-zA-Z0-9]*$"))
                    {
                        PrintError(new Errors(1, $"Некорректные символы в имени идентификатора: {cleanId}"));
                        return;
                    }
                    else
                    {
                        ident.Add(cleanId);
                        tokens.Add(new Token("IDENTIFIER", cleanId, 1, startStr.IndexOf(cleanId)));
                    }
                }
            }
            
            tokens.Add(new Token("SEPARATOR", ":", 1, index));
            
            string typePart = startStr.Substring(index + 1).Trim();
            if (typePart.ToUpper().StartsWith("INTEGER"))
            {
                tokens.Add(new Token("KEYWORD", "INTEGER", 1, index + 1));
            }
            else
            {
                PrintError(new Errors(1, "Ожидается тип INTEGER после ':'"));
                return;
            }

            if (!startStr.EndsWith(";"))
            {
                PrintError(new Errors(1, "Отсутствует ';' в конце объявления"));
                return;
            }
            else
            {
                tokens.Add(new Token("SEPARATOR", ";", 1, startStr.Length - 1));
            }
        }

        private void AnalyzeLine(string line, int lineNumber)
        {
            string cleanLine = line.Trim();
            if (string.IsNullOrEmpty(cleanLine)) return;

            if (cleanLine.ToUpper() == "BEGIN")
            {
                tokens.Add(new Token("KEYWORD", "BEGIN", lineNumber, 0));
                return;
            }
            else if (cleanLine.ToUpper() == "END")
            {
                tokens.Add(new Token("KEYWORD", "END", lineNumber, 0));
                return;
            }

            if (cleanLine.ToUpper().StartsWith("READ(") || cleanLine.ToUpper().StartsWith("WRITE("))
            {
                AnalyzeReadWrite(cleanLine, lineNumber);
                return;
            }

            if (cleanLine.Contains('='))
            {
                AnalyzeAssignment(cleanLine, lineNumber);
                return;
            }

            PrintError(new Errors(lineNumber, $"Неизвестная конструкция: {cleanLine}"));
        }

        private void AnalyzeReadWrite(string line, int lineNumber)
        {
            string upperLine = line.ToUpper();
            bool isRead = upperLine.StartsWith("READ(");
            string keyword = isRead ? "READ" : "WRITE";

            tokens.Add(new Token("KEYWORD", keyword, lineNumber, 0));
            tokens.Add(new Token("SEPARATOR", "(", lineNumber, keyword.Length));
            
            int startParams = keyword.Length + 1;
            int endParams = line.IndexOf(')');
            if (endParams == -1)
            {
                PrintError(new Errors(lineNumber, "Отсутствует закрывающая скобка ')'"));
                return;
            }

            string paramsStr = line.Substring(startParams, endParams - startParams);
            string[] parameters = paramsStr.Split(',');
            
            foreach (string param in parameters)
            {
                string cleanParam = param.Trim();
                if (ident.Contains(cleanParam))
                {
                    tokens.Add(new Token("IDENTIFIER", cleanParam, lineNumber, line.IndexOf(cleanParam)));
                }
                else
                {
                    PrintError(new Errors(lineNumber, $"Необъявленный идентификатор: {cleanParam}"));
                    return;
                }

                if (param != parameters[parameters.Length - 1])
                {
                    tokens.Add(new Token("SEPARATOR", ",", lineNumber, line.IndexOf(',', line.IndexOf(cleanParam))));
                }
            }

            tokens.Add(new Token("SEPARATOR", ")", lineNumber, endParams));
            
            if (!line.EndsWith(";"))
            {
                PrintError(new Errors(lineNumber, "Отсутствует ';' в конце оператора"));
                return;
            }
            else
            {
                tokens.Add(new Token("SEPARATOR", ";", lineNumber, line.Length - 1));
            }
        }

        private void AnalyzeAssignment(string line, int lineNumber)
        {
            string[] parts = line.Split('=');
            if (parts.Length != 2)
            {
                PrintError(new Errors(lineNumber, "Некорректный оператор присваивания"));
                return;
            }
            
            string leftPart = parts[0].Trim();
            if (ident.Contains(leftPart))
            {
                tokens.Add(new Token("IDENTIFIER", leftPart, lineNumber, 0));
            }
            else
            {
                PrintError(new Errors(lineNumber, $"Необъявленный идентификатор в левой части: {leftPart}"));
                return;
            }

            tokens.Add(new Token("OPERATOR", "=", lineNumber, parts[0].Length));

            string rightPart = parts[1].Trim().TrimEnd(';');
            AnalyzeExpression(rightPart, lineNumber, parts[0].Length + 1);
            if (hasError) return;

            if (!line.EndsWith(";"))
            {
                PrintError(new Errors(lineNumber, "Отсутствует ';' в конце оператора"));
                return;
            }
            else
            {
                tokens.Add(new Token("SEPARATOR", ";", lineNumber, line.Length - 1));
            }
        }

        private void AnalyzeExpression(string expression, int lineNumber, int startPos)
        {
            int pos = 0;
            while (pos < expression.Length && !hasError)
            {
                char current = expression[pos];
                if (!expression.Contains('(') && expression.Contains(')'))
                {
                    PrintError(new Errors(lineNumber, "Нет открывающейся скобочки"));
                    return;
                }
                if (current == '(')
                {
                    if (!expression.Contains(')'))
                    {
                        PrintError(new Errors(lineNumber,"Нет закрывающейся скобочки"));
                        return;
                    }
                }
                if (char.IsWhiteSpace(current))
                {
                    pos++;
                    continue;
                }

                if (current == '(')
                {
                    tokens.Add(new Token("SEPARATOR", "(", lineNumber, startPos + pos));
                    pos++;
                    continue;
                }
                else if (current == ')')
                {
                    tokens.Add(new Token("SEPARATOR", ")", lineNumber, startPos + pos));
                    pos++;
                    continue;
                }
                
                if (current == '-' && (pos == 0 || expression[pos - 1] == '(' || BinarOp.Contains(expression[pos - 1].ToString())))
                {
                    tokens.Add(new Token("OPERATOR", "-", lineNumber, startPos + pos));
                    pos++;
                    continue;
                }
                
                if (BinarOp.Contains(current.ToString()))
                {
                    tokens.Add(new Token("OPERATOR", current.ToString(), lineNumber, startPos + pos));
                    pos++;
                    continue;
                }

                if (char.IsLetter(current))
                {
                    string identifier = "";
                    while (pos < expression.Length && (char.IsLetterOrDigit(expression[pos]) || expression[pos] == '_'))
                    {
                        identifier += expression[pos];
                        pos++;
                    }

                    if (ident.Contains(identifier))
                    {
                        tokens.Add(new Token("IDENTIFIER", identifier, lineNumber, startPos + pos - identifier.Length));
                    }
                    else
                    {
                        PrintError(new Errors(lineNumber, $"Необъявленный идентификатор: {identifier}"));
                        return;
                    }
                    continue;
                }
            }
            
        }

        public void CheckOperators()
        {
    for (int i = 0; i < tokens.Count - 1; i++)
    {
        var current = tokens[i];
        var next = tokens[i + 1];

        if (current.Type == "OPERATOR" && next.Type == "OPERATOR")
        {
            if (next.Value == "-")
            {
                var allowedBeforeUnary = new List<string> { "=", "+", "-", "*", "(", "," };
                
                if (i > 0)
                {
                    var prev = tokens[i - 1];
                }
            }
            else
            {
                PrintError(new Errors(current.Line, $"Некорректная последовательность операторов: {current.Value} {next.Value}"));
                return;
            }
        }
    }
}

        private void PrintResults()
        {
            if (!hasError)
            {
                Console.WriteLine("\nРАСПОЗНАННЫЕ ТОКЕНЫ:");
                foreach (var token in tokens)
                {
                    Console.WriteLine($"  {token}");
                }
                Console.WriteLine($"\nВсего токенов: {tokens.Count}");
                Console.WriteLine("Лексический анализ завершен успешно!");
            }
        }

        public void checkStr() => Analyze();
    }
}