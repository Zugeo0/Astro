using AstroLang.Analysis;
using AstroLang.Analysis.Text;

namespace AstroLang;

public class Astro
{
	public void Run(string text)
	{
		var source = new SourceText(text);
		var tokens = Scanner.Scan(source);
		
		foreach (var token in tokens)
			Console.WriteLine($"{token}: '{source.GetLexeme(token.Span)}'");
	}
}