namespace AstroLang.Analysis.Text;

public class TextSpan
{
	public int Start { get; }
	public int Length { get; }

	public TextSpan(int start, int length)
	{
		Start = start;
		Length = length;
	}

	public TextSpan ExtendTo(TextSpan span, bool ignoreLength = false) => new TextSpan(Start, span.Start - Start + (ignoreLength ? 0 : span.Length));
}