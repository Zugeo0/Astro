using AstroLang.Analysis.Text;

namespace AstroLang.Diagnostics;

public class Diagnostic
{
	public TextSpan Span { get; }
	public string Message { get; }

	public Diagnostic(TextSpan span, string message)
	{
		Span = span;
		Message = message;
	}

	public void WriteMessage(TextWriter writer, SourceText source)
	{
		var lineIdx = source.FindLineIndex(Span.Start);
		var line = source.GetLine(lineIdx);
		var column = Span.Start - line.Span.Start;
		writer.WriteLine($"Error: {Message} at [{lineIdx + 1}:{column}]");

		var lineNumberText = $"{lineIdx + 1} | ";
		writer.WriteLine($"{lineNumberText}{line.Line}");

		var arrowOffset = new string(' ', lineNumberText.Length + column);
		var arrows = new string('^', Span.Length);
		writer.WriteLine($"{arrowOffset}{arrows}-- here");
	}
}