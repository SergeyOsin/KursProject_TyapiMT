using System;
using KursProject_TyapiMT;

class Program
{
    static void Main(string[] args)
    {
        List<string> code = File.ReadAllLines("code.txt").ToList();
        LexicalAnalyzator lx = new LexicalAnalyzator(code);
        lx.checkStr();
    }
}