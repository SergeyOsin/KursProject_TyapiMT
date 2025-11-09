        using System;
        using System.Collections.Generic;
        using System.Text.RegularExpressions;

        namespace KursProject_TyapiMT
        {
            public class LexicalAnalyzator
            {
                private const int MAXLENIDEN = 12;
                private readonly List<string> code;
                private readonly List<string> keywords = new()
                {
                    "VAR", "INTEGER", "BEGIN", "READ", "FOR", "TO", "DO", "END_FOR", "WRITE", "END"
                };
                public List<Token> Tokens { get; } = new();
                public bool HasError { get; private set; }

                public LexicalAnalyzator(List<string> _code) => code = _code;

                public class Token
                {
                    public string Type { get; }
                    public string Value { get; }
                    public int Line { get; }
                    public int Position { get; }

                    public Token(string type, string value, int line, int position)
                    {
                        Type = type;
                        Value = value;
                        Line = line;
                        Position = position;
                    }

                    public override string ToString() =>
                        $"{Type} '{Value}' (строка {Line}, позиция {Position})";
                }

                public void Analyze()
                {
                    Tokens.Clear();
                    HasError = false;
                    string fullCode = string.Join("\n", code); 
                    Tokenize(fullCode);
                    if (!HasError)
                    {
                        CheckExpressions();
                        if (!HasError)
                            PrintResults();
                    }
                }

                private void Tokenize(string input)
                {
                    int pos = 0, line = 1, linePos = 0;
                    while (pos < input.Length)
                    {
                        char c = input[pos];
                        if (char.IsWhiteSpace(c))
                        {
                            if (c == '\n')
                            {
                                line++;
                                linePos = 0;
                            }
                            else
                            {
                                linePos++;
                            }
                            pos++;
                            continue;
                        }

                        // Разделители
                        if (c is ':' or ';' or ',' or '(' or ')')
                        {
                            Tokens.Add(new Token("SEPARATOR", c.ToString(), line, linePos));
                            pos++; linePos++;
                            continue;
                        }

                        // Операторы
                        if (c is '+' or '-' or '*' or '=')
                        {
                            Tokens.Add(new Token("OPERATOR", c.ToString(), line, linePos));
                            pos++; linePos++;
                            continue;
                        }

                        // Числа
                        if (char.IsDigit(c))
                        {
                            int start = linePos;
                            string num = ReadWhile(pos, char.IsDigit, input, out pos);
                            Tokens.Add(new Token("NUMBER", num, line, start));
                            linePos += num.Length;
                            continue;
                        }

                        // Идентификаторы и ключевые слова
                        if (char.IsLetter(c) || c == '_')
                        {
                            int start = linePos;
                            string word = ReadWhile(pos, IsIdentifierChar, input, out pos);

                            if (word.Length > MAXLENIDEN)
                            {
                                Console.Write(Errors.Lexical(1,"Превышена максимальная длина идентификатора"));
                                return;
                            }

                            if (!Regex.IsMatch(word, @"^[a-zA-Z_][a-zA-Z_]*$"))
                            {
                                Console.Write((Errors.Lexical(1, $"Недопустимые символы в идентификаторе: {word}")));
                                return;
                            }

                            string upperWord = word.ToUpper();
                            if (keywords.Contains(upperWord))
                                Tokens.Add(new Token("KEYWORD", upperWord, line, start));
                            else
                                Tokens.Add(new Token("IDENTIFIER", word, line, start));

                            linePos += word.Length;
                            continue;
                        }
                        pos++; linePos++;
                    }
                }

                private string ReadWhile(int startPos, Func<char, bool> predicate, string input, out int endPos)
                {
                    endPos = startPos;
                    while (endPos < input.Length && predicate(input[endPos]))
                        endPos++;
                    return input.Substring(startPos, endPos - startPos);
                }

                private bool IsIdentifierChar(char c) => char.IsLetterOrDigit(c) || c == '_';
                
                private void PrintResults()
                {
                    Console.WriteLine("\nРАСПОЗНАННЫЕ ТОКЕНЫ:");
                    foreach (var token in Tokens)
                        Console.WriteLine($"  {token}");
                    Console.WriteLine($"\nВсего токенов: {Tokens.Count}");
                }

                private void CheckExpressions()
                {
                    int index = 0;
                    while (index < Tokens.Count)
                    {
                        var token = Tokens[index];

                        if (token.Type == "IDENTIFIER")
                        {
                            index++;
                            // Проверим, есть ли оператор присваивания
                            if (index < Tokens.Count && Tokens[index].Type == "OPERATOR" && Tokens[index].Value == "=")
                            {
                                index++;
                                bool validExpr = CheckExpressionTokens(index, out int nextIndex);
                                if (!validExpr)
                                {
                                    Console.Write(Errors.Lexical(index, "Некорректное выражение после '='"));
                                    return;
                                }
                                index = nextIndex;
                            }
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                
                private bool CheckExpressionTokens(int startIndex, out int endIndex)
        {
                int index = startIndex;
                bool expectOperand = true;

                while (index < Tokens.Count)
                {
                    var token = Tokens[index];

                    if (expectOperand)
                    {
                        if (token.Type == "NUMBER" || token.Type == "IDENTIFIER")
                        {
                            expectOperand = false;
                            index++;
                        }
                        else if (token.Type == "SEPARATOR" && token.Value == "(")
                        {
                            index++;
                            if (!CheckExpressionTokens(index, out int nextInside))
                            {
                                endIndex = index;
                                return false;
                            }
                            index = nextInside;
                            expectOperand = false;
                        }
                        else
                        {
                            // Ожидался операнд, но получен недопустимый токен
                            endIndex = index;
                            return false;
                        }
                    }
                    else
                    {
                        // Ожидаем оператор или закрывающую скобку
                        if (token.Type == "OPERATOR")
                        {
                            if (token.Value == "*" || token.Value == "+" || token.Value == "-")
                            {
                                expectOperand = true;
                                index++;
                            }
                            else if (token.Value == "=")
                            {
                                // Для '=' в выражениях (если допустимо) можно добавить логику
                                // Пока пропускаем, но можно расширить
                                index++;
                            }
                            else
                            {
                                // Неизвестный оператор
                                endIndex = index;
                                return false;
                            }
                        }
                        else if (token.Type == "SEPARATOR" && token.Value == ")")
                        {
                            index++;
                            endIndex = index;
                            return true; 
                        }
                    }
                }
                endIndex = index;
            return !expectOperand; 
        }
                public void CheckStr() => Analyze();
            }
        }