namespace KursProject_TyapiMT;

public enum ErrorType
{
    Lexical,
    Syntax,
    Semantic
}
public class Errors
{
    private string Message { get; }
    private ErrorType Type { get; }

    public Errors(string message, ErrorType type)
    {
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
        return $"{prefix}:\n {Message}";
    }
    public static Errors Lexical(string message) =>
        new Errors(message, ErrorType.Lexical);
    public static Errors Syntax(string message) =>
        new Errors(message, ErrorType.Syntax);
    public static Errors Semantic(string message) =>
        new Errors(message, ErrorType.Semantic);
}

