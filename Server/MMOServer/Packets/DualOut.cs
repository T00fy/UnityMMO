using System;
using System.IO;
using System.Text;

public static class DualOut
{
    private static TextWriter _current;

    private class OutputWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return _current.Encoding;
            }
        }

        public override void WriteLine(string value)
        {
            _current.WriteLine(value);
            File.AppendAllText("Output.txt", value + Environment.NewLine);
        }
    }

    public static void Init()
    {
        _current = Console.Out;
        Console.SetOut(new OutputWriter());
    }
}