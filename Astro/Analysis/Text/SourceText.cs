namespace AstroLang.Analysis.Text;

public class SourceText
{
	private readonly string _text;

	public SourceText(string text)
	{
		_text = text;
	}

	public int Length => _text.Length;
	public char this[int idx] => _text[idx];

	public string GetLexeme(TextSpan span) => _text.Substring(span.Index, span.Length);
}