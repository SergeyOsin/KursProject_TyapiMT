namespace KursProject_TyapiMT;

public static class Interpreter
{
    private static List<string> interPre;
    private static Dictionary<string, int> idents = new();

    public static void Start()
    {
        List<string> code = File.ReadAllLines("code.txt").ToList();
        interPre = code.ToList();
        LexicalAnalyzator lx = new LexicalAnalyzator(code);
        if (lx.Analyze() && SyntaxAnalyzer.Analyze(lx.Tokens) && SemanticAnalyzer.Analyze(lx.Tokens))
        {
            Console.Write("Анализаторы не обнаружили ошибок\n");
            ReadIdents(interPre);
            ExecuteProgram();
        }
    }

    private static void ReadIdents(List<string> interPre)
    {
        for (int i = 2; !interPre[i].StartsWith("for") && !interPre[i].Contains('*'); i++)
        {
            string str = interPre[i].Trim();
            if (str.StartsWith("read"))
            {
                string iden = str.Substring(5, str.IndexOf(')') - 5);
                Console.Write($"Значение {iden} = ");
                idents[iden] = int.Parse(Console.ReadLine());
                Console.WriteLine($"Переменной {iden} присвоено значение {idents[iden]}\n");
            }
            else if (str.Contains("="))
            {
                string iden = str.Substring(0, str.IndexOf("=")).Trim();
                string elem = str.Substring(str.IndexOf("=") + 1).Trim(';', ' ');
                idents[iden] = idents.ContainsKey(elem) ? idents[elem] : Convert.ToInt32(elem);
                Console.WriteLine($"Переменной {iden} присвоено значение {idents[iden]}");
            }
        }
    }

    private static void ExecuteProgram()
    {
        for (int i = 3; i < interPre.Count - 1; i++)
        {
            string line = interPre[i].Trim();
            
            if (line.StartsWith("for")) 
            {
                ProcessFor(ref i);
            }
            else if (line.StartsWith("write")) 
            {
                ProcessWrite(line);
            }
            else if (line.Contains("=")) 
            {
                ProcessAssignment(line);
            }
        }
    }

    private static void ProcessFor(ref int currentLine)
    {
        string forLine = interPre[currentLine].Trim();
        string[] parts = forLine.Split(new[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
        
        string varName = parts[1];
        int start = int.Parse(parts[2]);
        int end = int.Parse(parts[4]); 
        
        idents[varName] = start;
        int loopStartLine = currentLine;
        
        currentLine++;

        int bodyStartLine = currentLine;

        while (idents[varName] <= end)
        {
            int tempLine = bodyStartLine;
            
            while (tempLine < interPre.Count - 1 && 
                   !interPre[tempLine].Trim().Equals("end;") && 
                   !interPre[tempLine].Trim().Equals("endfor;") &&
                   !interPre[tempLine].Trim().Equals("end_for;"))
            {
                string line = interPre[tempLine].Trim();
                
                if (line.StartsWith("write")) 
                {
                    ProcessWrite(line);
                }
                else if (line.Contains("=")) 
                {
                    ProcessAssignment(line);
                }
                else if (line.StartsWith("for")) 
                {
                    ProcessFor(ref tempLine);
                }
                
                tempLine++;
                
                if (tempLine >= interPre.Count - 1) break;
            }
            
            idents[varName]++;
        }
        
        while (currentLine < interPre.Count - 1 && 
               !interPre[currentLine].Trim().Equals("end;") && 
               !interPre[currentLine].Trim().Equals("endfor;") &&
               !interPre[currentLine].Trim().Equals("end_for;"))
        {
            currentLine++;
        }
    }

    private static void ProcessAssignment(string assignmentStr)
    {
        string[] parts = assignmentStr.Split('=');
        string iden = parts[0].Trim();
        string expression = parts[1].Trim().Trim(';');
        
        idents[iden] = EvaluateExpression(expression);
        Console.WriteLine($"Переменной {iden} присвоено значение {idents[iden]}");
    }

    private static int EvaluateExpression(string expression)
    {
        expression = expression.Trim();
        
        if (expression.Contains("+"))
        {
            string[] parts = expression.Split('+');
            return GetValue(parts[0].Trim()) + GetValue(parts[1].Trim());
        }
        else if (expression.Contains("-"))
        {
            string[] parts = expression.Split('-');
            return GetValue(parts[0].Trim()) - GetValue(parts[1].Trim());
        }
        else if (expression.Contains("*"))
        {
            string[] parts = expression.Split('*');
            return GetValue(parts[0].Trim()) * GetValue(parts[1].Trim());
        }
        else
        {
            return GetValue(expression);
        }
    }

    private static int GetValue(string token)
    {
        token = token.Trim();
        if (idents.ContainsKey(token))
            return idents[token];
        else
            return Convert.ToInt32(token);
    }

    private static void ProcessWrite(string writeStr)
    {
        int start = writeStr.IndexOf('(') + 1;
        int end = writeStr.IndexOf(')');
        string paramsStr = writeStr.Substring(start, end - start);
        string[] vars = paramsStr.Split(',');
        
        Console.Write("Вывод: ");
        foreach (string varName in vars)
        {
            string trimmedName = varName.Trim();
            if (idents.ContainsKey(trimmedName))
            {
                Console.Write($"{trimmedName} = {idents[trimmedName]} ");
            }
        }
        Console.WriteLine();
    }
}