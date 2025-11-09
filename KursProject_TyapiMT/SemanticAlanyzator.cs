using System.Text.RegularExpressions;

namespace KursProject_TyapiMT;

public class SemanticAlanyzator
{
    private Errors errors;
    private Dictionary<string, bool> idents;
    private List<string> code;
    public SemanticAlanyzator(List<string> _code)
    {
        code = _code;
        idents=new Dictionary<string, bool>();
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
                if (!Regex.IsMatch(cleanId, @"^[a-zA-Z][a-zA-Z0-9]*$"))
                    return (false, "Идентификатор содержит лишние символы");
                idents[cleanId] = true;
            }
        }
        return (true, " ");
    }
    public void TurnSemantic()
    {
        if (!CheckSemantic().Item1)
        {
            Console.Write(Errors.Semantic(1, CheckSemantic().Item2));
            return;
        }
    }
}