using System.Text.RegularExpressions;

namespace KursProject_TyapiMT;

public class SemanticAlanyz
{
    private const int MAXLENIDEN = 12;
    private Errors errors;
    private List<string> namesIden { get; set; }
    private List<string> code;
    public SemanticAlanyz(List<string> _code)
    {
        code = _code;
        namesIden = new List<string>();
    }

    private (bool,string) CheckSemantic()
    {
        string firststr = code[0].ToUpper();
        int index = firststr.IndexOf(':');
        string idenPart = firststr.Substring(3, index - 3).Trim();
        string[] identifiers = idenPart.Split(',');
        foreach (string identifier in identifiers)
        {
            string cleanId = identifier.Trim();
            if (!string.IsNullOrEmpty(cleanId))
            {
                if (cleanId.Length > MAXLENIDEN)
                    return (false, "Превышена максимальная длина");

                if (!Regex.IsMatch(cleanId, @"^[a-zA-Z][a-zA-Z0-9]*$"))
                    return (false, "Идентификатор содержит лишние символы");
                namesIden.Add(cleanId);
            }
        }
        return (true, " ");
    }
    public void TurnSemantic()
    {
        if (!CheckSemantic().Item1)
        {
            errors = new Errors(0, CheckSemantic().Item2);
            errors.ToString();
            return;
        }
    }
}