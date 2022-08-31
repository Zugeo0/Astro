namespace AstroLang.Analysis.Text;

public class TextLine
{
	public TextSpan Span { get; }
	public string Line { get; }

	public TextLine(TextSpan span, string line)
	{
		Span = span;
		Line = line;
	}
}