using System.Text.RegularExpressions;

namespace KursProject_TyapiMT
{
    public class LexicalAnalyzator
    {
        private Errors er;
        private const int MAXLENIDEN = 12;
        private List<string> code;
        private List<string> keywords { get; set; }
        private List<string> UnarOp { get; set; }
        private List<string> BinarOp { get; set; }
        private List<string> ident { get; set; }

        public LexicalAnalyzator(List<string> _code)
        {
            code = _code;
            keywords = new List<string>()
            {
                "VAR", "INTEGER", "BEGIN", "READ", "FOR", "TO", "DO", "END_FOR", "WRITE", "END"
            };
            UnarOp = new List<string>() { "-" };
            BinarOp = new List<string>() { "+", "-", "*" };
            ident = new List<string>(); // Инициализация списка идентификаторов
        }

        public void checkStr()
        {
            if (code.Count == 0)
            {
                er = new Errors(0, "Пустой входной файл");
                Console.Write(er.ToString());
                return;
            }

            string startStr = code[0].Trim(); // Убираем пробелы в начале и конце

            // Проверка на VAR в начале строки
            if (!Regex.IsMatch(startStr, @"^\s*VAR\b", RegexOptions.IgnoreCase))
            {
                er = new Errors(0, "Программа должна начинаться с VAR");
                Console.Write(er.ToString());
                return;
            }

            // Находим позицию двоеточия
            int index = startStr.IndexOf(':');
            if (index == -1)
            {
                er = new Errors(0, "Отсутствует символ ':' после VAR");
                Console.Write(er.ToString());
                return;
            }

            // Извлекаем идентификаторы до двоеточия
            string iden = "";
            for (int i = index - 1; i >= 0; i--)
            {
                if (startStr[i] == ' ')
                {
                    if (!string.IsNullOrEmpty(iden))
                    {
                        ident.Add(new string(iden.Reverse().ToArray()));
                        iden = "";
                    }
                }
                else if (startStr[i] == ',')
                {
                    if (!string.IsNullOrEmpty(iden))
                    {
                    ident.Add(new string(iden.Reverse().ToArray()));
                    iden = "";
                    }
                }
                else
                {
                    iden += startStr[i];
                }
            }

            // Добавляем последний идентификатор
            if (!string.IsNullOrEmpty(iden))
            {
                ident.Add(new string(iden.Reverse().ToArray()));
            }

            // Проверка длины идентификаторов
            for (int i = 0; i < ident.Count; i++)
            {
                if (ident[i].Length > MAXLENIDEN)
                {
                    er = new Errors(0, $"Превышена максимальная длина идентификатора");
                    Console.Write(er.ToString());
                    return;
                }
            }

            // Дополнительная проверка на корректность идентификаторов
            foreach (var id in ident)
            {
                if (!Regex.IsMatch(id, @"^[a-zA-Z][a-zA-Z0-9]*$"))
                {
                    er = new Errors(0, $"Некорректный идентификатор: {id}");
                    Console.Write(er.ToString());
                    return;
                }
            }
        }
    }
}
