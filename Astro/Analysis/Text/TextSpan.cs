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
}