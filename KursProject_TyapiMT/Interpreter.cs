namespace KursProject_TyapiMT;

public class Interpreter
{
    public static void Start()
    {
        List<string> code = File.ReadAllLines("code.txt").ToList();
        LexicalAnalyzator lx = new LexicalAnalyzator(code);
        if (lx.Analyze() && SyntaxAnalyzer.Analyze(lx.Tokens) && SemanticAnalyzer.Analyze(lx.Tokens))
        {
            Console.Write("Анализаторы выполнены без ошибок");
        }
        
    }
    
}