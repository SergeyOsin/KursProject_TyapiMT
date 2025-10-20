namespace KursProject_TyapiMT;

public class Errors
{
    private int numbstr;
    private string error;

    public Errors(int _numbstr,string _error)
    {
        numbstr = _numbstr;
        error = _error;
    }
    public override string ToString()
    {
        return ($"Ошибка в {numbstr} строке:\n{error}");
    }
    
}