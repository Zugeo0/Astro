namespace AstroLang.Analysis.Text;

public class TextSpan
{
	public int Index { get; }
	public int Length { get; }

	public TextSpan(int index, int length)
	{
		Index = index;
		Length = length;
	}
}