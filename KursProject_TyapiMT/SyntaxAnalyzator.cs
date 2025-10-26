using System;
using System.Collections.Generic;

namespace KursProject_TyapiMT
{
    public abstract class SyntaxNode
    {
        public abstract void Print(int indent = 0);
    }
    public class ProgramNode : SyntaxNode
    {
        public DeclListNode DeclList { get; }
        public StmtListNode StmtList { get; }

        public ProgramNode(DeclListNode declList, StmtListNode stmtList)
        {
            DeclList = declList;
            StmtList = stmtList;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}Program");
            DeclList.Print(indent + 2);
            StmtList.Print(indent + 2);
        }
    }
    
    public class DeclListNode : SyntaxNode
    {
        public List<string> Identifiers { get; }

        public DeclListNode(List<string> identifiers)
        {
            Identifiers = identifiers;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}DeclList: {string.Join(", ", Identifiers)}");
        }
    }

    // Узел списка операторов
    public class StmtListNode : SyntaxNode
    {
        public List<StmtNode> Statements { get; }

        public StmtListNode(List<StmtNode> statements)
        {
            Statements = statements;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}StmtList");
            foreach (var stmt in Statements)
            {
                stmt.Print(indent + 2);
            }
        }
    }
    
    public abstract class StmtNode : SyntaxNode { }
    
    public class ReadStmtNode : StmtNode
    {
        public List<string> Identifiers { get; }

        public ReadStmtNode(List<string> identifiers)
        {
            Identifiers = identifiers;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}ReadStmt: {string.Join(", ", Identifiers)}");
        }
    }

    // Узел WRITE
    public class WriteStmtNode : StmtNode
    {
        public List<string> Identifiers { get; }

        public WriteStmtNode(List<string> identifiers)
        {
            Identifiers = identifiers;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}WriteStmt: {string.Join(", ", Identifiers)}");
        }
    }
    
    public class AssignStmtNode : StmtNode
    {
        public string Identifier { get; }
        public ExprNode Expression { get; }

        public AssignStmtNode(string identifier, ExprNode expression)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}AssignStmt: {Identifier} =");
            Expression.Print(indent + 2);
        }
    }

    // Узел FOR
    public class ForStmtNode : StmtNode
    {
        public string Identifier { get; }
        public ExprNode ToExpr { get; }
        public StmtListNode Body { get; }

        public ForStmtNode(string identifier, ExprNode toExpr, StmtListNode body)
        {
            Identifier = identifier;
            ToExpr = toExpr;
            Body = body;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}ForStmt: FOR {Identifier} TO ... DO");
            ToExpr.Print(indent + 2);
            Body.Print(indent + 2);
        }
    }
    
    public abstract class ExprNode : SyntaxNode { }

    // Узел бинарной операции
    public class BinaryOpNode : ExprNode
    {
        public ExprNode Left { get; }
        public string Operator { get; }
        public ExprNode Right { get; }

        public BinaryOpNode(ExprNode left, string op, ExprNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}BinaryOp: {Operator}");
            Left.Print(indent + 2);
            Right.Print(indent + 2);
        }
    }
    
    public class UnaryOpNode : ExprNode
    {
        public string Operator { get; }
        public ExprNode Operand { get; }

        public UnaryOpNode(string op, ExprNode operand)
        {
            Operator = op;
            Operand = operand;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}UnaryOp: {Operator}");
            Operand.Print(indent + 2);
        }
    }

    // Узел идентификатора
    public class IdentifierNode : ExprNode
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}Identifier: {Name}");
        }
    }
    
    public class NumberNode : ExprNode
    {
        public string Value { get; }

        public NumberNode(string value)
        {
            Value = value;
        }

        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}Number: {Value}");
        }
    }

    public class SyntaxAnalyzator
    {
        private List<LexicalAnalyzator.Token> tokens;
        private int currentIndex;
        private bool hasError;
        private ProgramNode syntaxTree;

        public SyntaxAnalyzator(List<LexicalAnalyzator.Token> _tokens)
        {
            tokens = _tokens;
            currentIndex = 0;
            hasError = false;
        }

        public void Analyze()
        {
            hasError = false;
            currentIndex = 0;
            syntaxTree = ParseProgram();

            if (!hasError && currentIndex < tokens.Count)
            {
                PrintError($"Неожиданный токен: {tokens[currentIndex]}");
            }
            else if (!hasError)
            {
                Console.WriteLine("Синтаксический анализ завершен успешно! Построено дерево:");
                syntaxTree.Print();
            }
        }

        private ProgramNode ParseProgram()
        {
            DeclListNode declList = null;
            StmtListNode stmtList = null;

            if (Match("KEYWORD", "VAR"))
            {
                declList = ParseDeclList();
                if (Match("SEPARATOR", ":"))
                {
                    if (Match("KEYWORD", "INTEGER"))
                    {
                        if (Match("SEPARATOR", ";"))
                        {
                            if (Match("KEYWORD", "BEGIN"))
                            {
                                stmtList = ParseStmtList();
                                if (!Match("KEYWORD", "END"))
                                {
                                    PrintError("Ожидается END в конце программы");
                                }
                            }
                            else
                            {
                                PrintError("Ожидается BEGIN после объявлений");
                            }
                        }
                        else
                        {
                            PrintError("Ожидается ; после INTEGER");
                        }
                    }
                    else
                    {
                        PrintError("Ожидается INTEGER после :");
                    }
                }
                else
                {
                    PrintError("Ожидается : после объявлений");
                }
            }
            else
            {
                PrintError("Программа должна начинаться с VAR");
            }

            return hasError ? null : new ProgramNode(declList, stmtList);
        }

        private DeclListNode ParseDeclList()
        {
            var identifiers = new List<string>();
            if (Peek("IDENTIFIER"))
            {
                identifiers.Add(tokens[currentIndex].Value);
                Match("IDENTIFIER");
                while (Match("SEPARATOR", ","))
                {
                    if (Peek("IDENTIFIER"))
                    {
                        identifiers.Add(tokens[currentIndex].Value);
                        Match("IDENTIFIER");
                    }
                    else
                    {
                        PrintError("Ожидается IDENTIFIER после ,");
                        break;
                    }
                }
            }
            else
            {
                PrintError("Ожидается хотя бы один IDENTIFIER в объявлениях");
            }
            return new DeclListNode(identifiers);
        }

        private StmtListNode ParseStmtList()
        {
            var statements = new List<StmtNode>();
            while (!Peek("KEYWORD", "END") && !Peek("KEYWORD", "END_FOR") && !hasError)
            {
                var stmt = ParseStmt();
                if (stmt != null)
                    statements.Add(stmt);
            }
            return new StmtListNode(statements);
        }

        private StmtNode ParseStmt()
        {
            if (Match("KEYWORD", "READ"))
            {
                if (Match("SEPARATOR", "("))
                {
                    var idList = ParseIDList();
                    if (Match("SEPARATOR", ")"))
                    {
                        if (Match("SEPARATOR", ";"))
                        {
                            return new ReadStmtNode(idList);
                        }
                        else
                        {
                            PrintError("Ожидается ; после READ");
                        }
                    }
                    else
                    {
                        PrintError("Ожидается ) после параметров READ");
                    }
                }
                else
                {
                    PrintError("Ожидается ( после READ");
                }
            }
            else if (Match("KEYWORD", "WRITE"))
            {
                if (Match("SEPARATOR", "("))
                {
                    var idList = ParseIDList();
                    if (Match("SEPARATOR", ")"))
                    {
                        if (Match("SEPARATOR", ";"))
                        {
                            return new WriteStmtNode(idList);
                        }
                        else
                        {
                            PrintError("Ожидается ; после WRITE");
                        }
                    }
                    else
                    {
                        PrintError("Ожидается ) после параметров WRITE");
                    }
                }
                else
                {
                    PrintError("Ожидается ( после WRITE");
                }
            }
            else if (Peek("IDENTIFIER") && PeekNext("OPERATOR", "="))
            {
                var ident = tokens[currentIndex].Value;
                Match("IDENTIFIER");
                Match("OPERATOR", "=");
                var expr = ParseExpr();
                if (Match("SEPARATOR", ";"))
                {
                    return new AssignStmtNode(ident, expr);
                }
                else
                {
                    PrintError("Ожидается ; после присваивания");
                }
            }
            else if (Match("KEYWORD", "FOR"))
            {
                if (Peek("IDENTIFIER"))
                {
                    var ident = tokens[currentIndex].Value;
                    Match("IDENTIFIER");
                    if (Match("KEYWORD", "TO"))
                    {
                        var toExpr = ParseExpr();
                        if (Match("KEYWORD", "DO"))
                        {
                            var body = ParseStmtList();
                            if (Match("KEYWORD", "END_FOR"))
                            {
                                return new ForStmtNode(ident, toExpr, body);
                            }
                            else
                            {
                                PrintError("Ожидается END_FOR после цикла FOR");
                            }
                        }
                        else
                        {
                            PrintError("Ожидается DO после TO в FOR");
                        }
                    }
                    else
                    {
                        PrintError("Ожидается TO после IDENTIFIER в FOR");
                    }
                }
                else
                {
                    PrintError("Ожидается IDENTIFIER после FOR");
                }
            }
            else
            {
                PrintError($"Неожиданный токен в операторе: {tokens[currentIndex]}");
                currentIndex++;
            }
            return null;
        }

        private List<string> ParseIDList()
        {
            var identifiers = new List<string>();
            if (Peek("IDENTIFIER"))
            {
                identifiers.Add(tokens[currentIndex].Value);
                Match("IDENTIFIER");
                while (Match("SEPARATOR", ","))
                {
                    if (Peek("IDENTIFIER"))
                    {
                        identifiers.Add(tokens[currentIndex].Value);
                        Match("IDENTIFIER");
                    }
                    else
                    {
                        PrintError("Ожидается IDENTIFIER после ,");
                        break;
                    }
                }
            }
            else
            {
                PrintError("Ожидается хотя бы один IDENTIFIER в списке");
            }
            return identifiers;
        }

        private ExprNode ParseExpr()
        {
            var left = ParseTerm();
            while (Match("OPERATOR", "+") || Match("OPERATOR", "-"))
            {
                var op = tokens[currentIndex - 1].Value;
                var right = ParseTerm();
                left = new BinaryOpNode(left, op, right);
            }
            return left;
        }

        private ExprNode ParseTerm()
        {
            var left = ParseFactor();
            while (Match("OPERATOR", "*"))
            {
                var op = tokens[currentIndex - 1].Value;
                var right = ParseFactor();
                left = new BinaryOpNode(left, op, right);
            }
            return left;
        }

        private ExprNode ParseFactor()
        {
            if (Peek("IDENTIFIER"))
            {
                var ident = tokens[currentIndex].Value;
                Match("IDENTIFIER");
                return new IdentifierNode(ident);
            }
            else if (Peek("NUMBER"))
            {
                var num = tokens[currentIndex].Value;
                Match("NUMBER");
                return new NumberNode(num);
            }
            else if (Match("SEPARATOR", "("))
            {
                var expr = ParseExpr();
                if (Match("SEPARATOR", ")"))
                {
                    return expr;
                }
                else
                {
                    PrintError("Ожидается ) после выражения");
                }
            }
            else if (Match("OPERATOR", "-"))
            {
                var factor = ParseFactor();
                return new UnaryOpNode("-", factor);
            }
            else
            {
                PrintError($"Неожиданный токен в выражении: {tokens[currentIndex]}");
                currentIndex++;
            }
            return null;
        }

        private bool Match(string type, string value = null)
        {
            if (currentIndex < tokens.Count && tokens[currentIndex].Type == type && (value == null || tokens[currentIndex].Value == value))
            {
                currentIndex++;
                return true;
            }
            return false;
        }

        private bool Peek(string type, string value = null)
        {
            return currentIndex < tokens.Count && tokens[currentIndex].Type == type && (value == null || tokens[currentIndex].Value == value);
        }
        private bool PeekNext(string type, string value = null)
        {
            return currentIndex + 1 < tokens.Count && tokens[currentIndex + 1].Type == type && (value == null || tokens[currentIndex + 1].Value == value);
        }
        private void PrintError(string message)
        {
            hasError = true;
            Console.WriteLine($"Синтаксическая ошибка: {message}");
        }
        public ProgramNode GetSyntaxTree() => syntaxTree;
    }
}
