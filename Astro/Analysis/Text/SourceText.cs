using System.Text;

namespace AstroLang.Analysis.Text;

public class SourceText
{
	private readonly string _text;
	private readonly TextLine[] _lines;

	public SourceText(string text)
	{
		_text = text;
		_lines = SplitLines(text);
	}
	
	public int Length => _text.Length;
	public char this[int idx] => _text[idx];

	public int FindLineIndex(int index)
	{
		var lower = 0;
		var upper = _lines.Length - 1;

		while (lower <= upper)
		{
			var idx = (lower + upper) / 2;
			var line = _lines[idx];
			var start = line.Span.Start;
			var end = line.Span.Start + line.Span.Length;
			
			if (index >= start && index < end)
				return idx;
			
			if (index < start)
				upper = idx - 1;
			else
				lower = idx + 1;
		}
		
		return 0;
	}

	public TextLine GetLine(int idx) => _lines[idx];

	public string GetLexeme(TextSpan span) => _text.Substring(span.Start, span.Length);

	private static TextLine[] SplitLines(string text)
	{
		var lines = new List<TextLine>();
		var lineBuilder = new StringBuilder();
		var index = 0;
		var lineLength = 0;
		
		foreach (var c in text)
		{
			lineBuilder.Append(c);
			lineLength++;

			if (c != '\n')
				continue;

			var span = new TextSpan(index, lineLength);
			var line = lineBuilder.ToString();
			var textLine = new TextLine(span, line);
			
			lineBuilder.Clear();
			lines.Add(textLine);
			index += lineLength;
		}

		if (lineBuilder.Length != 0)
		{
			var span = new TextSpan(index, lineLength);
			var line = lineBuilder.ToString();
			var textLine = new TextLine(span, line);
			lines.Add(textLine);
		}

		return lines.ToArray();
	}
}