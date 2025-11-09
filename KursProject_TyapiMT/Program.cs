using System;
using KursProject_TyapiMT;
using Microsoft.Win32.SafeHandles;

class Program
{
    static void Main(string[] args)
    {
        List<string> code = File.ReadAllLines("code.txt").ToList();
        LexicalAnalyzator lx = new LexicalAnalyzator(code);
        lx.Analyze();
    }
}