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
        }

        public bool Analyze()
        {
            Tokens.Clear();
            HasError = false;
            string fullCode = string.Join("\n", code);
            Tokenize(fullCode);
            return !HasError;
        }

        private void Tokenize(string input)
        {
            int pos = 0, line = 1, linePos = 0;
            while (pos < input.Length && !HasError)
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

                if (c is ':' or ';' or ',' or '(' or ')')
                {
                    Tokens.Add(new Token("SEPARATOR", c.ToString(), line, linePos));
                    pos++;
                    linePos++;
                    continue;
                }

                // Операторы
                if (c is '+' or '-' or '*' or '=' or '/')
                {
                    Tokens.Add(new Token("OPERATOR", c.ToString(), line, linePos));
                    pos++;
                    linePos++;
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
                        HasError = true;
                        Console.WriteLine(Errors.Lexical("Превышена максимальная длина идентификатора"));
                        return;
                    }

                    // Разрешаем буквы, цифры и подчеркивания в идентификаторах
                    if (!Regex.IsMatch(word, @"^[a-zA-Z_]*$"))
                    {
                        HasError = true;
                        Console.Write(Errors.Lexical("Недопустимые символы в идентификаторе"));
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
                HasError = true;
                return;
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
    }
}