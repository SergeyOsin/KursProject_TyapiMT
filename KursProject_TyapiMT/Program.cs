using System;
using KursProject_TyapiMT;

class Turn
{

    static void Main(string[] args)
    {
        string filePath = "/Users/sergeyosin/RiderProjects/KursProject_TyapiMT/KursProject_TyapiMT/code.txt";
        List<string> code = File.ReadAllLines(filePath).ToList();
        LexicalAnalyzator lx = new LexicalAnalyzator(code);
        lx.checkStr();

    }
}