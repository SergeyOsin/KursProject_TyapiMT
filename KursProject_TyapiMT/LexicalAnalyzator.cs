using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KursProject_TyapiMT
{
    public class LexicalAnalyzator
    {
        private const int MAXLENIDEN = 12;
        private List<string> code;
        private List<string> keywords { get; set; }
        private List<Token> tokens { get; set; }
        private bool hasError = false;

        public LexicalAnalyzator(List<string> _code)
        {
            code = _code;
            keywords = new List<string>()
            {
                "VAR", "INTEGER", "BEGIN", "READ", "FOR", "TO", "DO", "END_FOR", "WRITE", "END"
            };
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
                return $"{Type} '{Value}' (строка {Line}, позиция {Position})";
            }
        }

        public void Analyze()
        {
            tokens.Clear();
            hasError = false;

            if (code.Count == 0)
            {
                PrintError("Пустой входной файл");
                return;
            }

            string fullCode = string.Join(" ", code); // Объединяем все строки в одну для общего токенизирования
            Tokenize(fullCode);

            if (!hasError)
            {
                PrintResults();
            }
        }

        private void Tokenize(string input)
        {
            int pos = 0;
            int line = 1;
            int linePos = 0;

            while (pos < input.Length)
            {
                char current = input[pos];

                if (char.IsWhiteSpace(current))
                {
                    if (current == '\n')
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

                // Проверяем на separators: :, ;, ,, (, )
                if (current == ':' || current == ';' || current == ',' || current == '(' || current == ')')
                {
                    tokens.Add(new Token("SEPARATOR", current.ToString(), line, linePos));
                    pos++;
                    linePos++;
                    continue;
                }

                // Проверяем на operators: +, -, *, =
                if (current == '+' || current == '-' || current == '*' || current == '=')
                {
                    tokens.Add(new Token("OPERATOR", current.ToString(), line, linePos));
                    pos++;
                    linePos++;
                    continue;
                }

                // Проверяем на числа
                if (char.IsDigit(current))
                {
                    string number = "";
                    int startPos = linePos;
                    while (pos < input.Length && char.IsDigit(input[pos]))
                    {
                        number += input[pos];
                        pos++;
                        linePos++;
                    }
                    tokens.Add(new Token("NUMBER", number, line, startPos));
                    continue;
                }

                // Проверяем на идентификаторы или ключевые слова
                if (char.IsLetter(current))
                {
                    string word = "";
                    int startPos = linePos;
                    while (pos < input.Length && (char.IsLetterOrDigit(input[pos]) || input[pos] == '_'))
                    {
                        word += input[pos];
                        pos++;
                        linePos++;
                    }

                    if (keywords.Contains(word.ToUpper()))
                    {
                        tokens.Add(new Token("KEYWORD", word.ToUpper(), line, startPos));
                    }
                    else
                    {
                        if (word.Length > MAXLENIDEN)
                        {
                            PrintError($"Превышена максимальная длина идентификатора: {word} (строка {line})");
                            return;
                        }
                        if (!Regex.IsMatch(word, @"^[a-zA-Z][a-zA-Z0-9]*$"))
                        {
                            PrintError($"Некорректные символы в имени идентификатора: {word} (строка {line})");
                            return;
                        }
                        tokens.Add(new Token("IDENTIFIER", word, line, startPos));
                    }
                    continue;
                }

                // Неизвестный символ
                PrintError($"Неизвестный символ: {current} (строка {line}, позиция {linePos})");
                pos++;
                linePos++;
            }
        }

        private void PrintError(string message)
        {
            hasError = true;
            Console.WriteLine($"Лексическая ошибка: {message}");
        }

        private void PrintResults()
        {
            Console.WriteLine("\nРАСПОЗНАННЫЕ ТОКЕНЫ:");
            foreach (var token in tokens)
            {
                Console.WriteLine($"  {token}");
            }
            Console.WriteLine($"\nВсего токенов: {tokens.Count}");
        }

        public List<Token> GetTokens() => tokens;

        public void checkStr() => Analyze();
    }
}

