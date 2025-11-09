namespace KursProject_TyapiMT;

public enum ErrorType
{
    Lexical,
    Syntax,
    Semantic
}
public class Errors
{
    private int LineNumber { get; }
    private string Message { get; }
    private ErrorType Type { get; }

    public Errors(int lineNumber, string message, ErrorType type)
    {
        LineNumber = lineNumber;
        Message = message;
        Type = type;
    }

    public override string ToString()
    {
        string prefix = Type switch
        {
            ErrorType.Lexical => "Лексический анализатор",
            ErrorType.Syntax => "Синтаксический анализатор",
            ErrorType.Semantic => "Семантический анализатор"
        };
        return $"{prefix}:\nОшибка в строке {LineNumber}: {Message}";
    }
    public static Errors Lexical(int line, string message) =>
        new Errors(line, message, ErrorType.Lexical);
    public static Errors Syntax(int line, string message) =>
        new Errors(line, message, ErrorType.Syntax);
    public static Errors Semantic(int line, string message) =>
        new Errors(line, message, ErrorType.Semantic);
}

