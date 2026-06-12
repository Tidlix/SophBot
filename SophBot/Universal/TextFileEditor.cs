using System;
using System.IO;
using System.Linq;

namespace SophBot.Universal
{
    public class TextFileEditor
    {
        private readonly string _filePath;

        public TextFileEditor(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
                File.Create(_filePath).Close();
        }

        public string[] AppendNewLine(string text)
        {
            File.AppendAllLines(_filePath, [text]);
            return ReadAllLines();
        }
        public string[] AppendNewLines(string[] texts)
        {
            File.AppendAllLines(_filePath, texts);
            return ReadAllLines();
        }

        public string[] SetLineText(int lineIndex, string text)
        {
             var lines = File.ReadAllLines(_filePath).ToList();
            if (lineIndex < 0 || lineIndex >= lines.Count)
                throw new ArgumentOutOfRangeException(nameof(lineIndex));

            lines[lineIndex] = text;
            File.WriteAllLines(_filePath, lines);
            return ReadAllLines();
        }
        public string[] DeleteLine(int lineIndex)
        {
             var lines = File.ReadAllLines(_filePath).ToList();
            if (lineIndex < 0 || lineIndex >= lines.Count)
                throw new ArgumentOutOfRangeException(nameof(lineIndex));

            lines.RemoveAt(lineIndex);
            File.WriteAllLines(_filePath, lines);
            return ReadAllLines();
        }

        public string[] ReadAllLines()
        {
            return File.ReadAllLines(_filePath);
        }
    }
}