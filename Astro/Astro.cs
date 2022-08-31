using AstroLang.Analysis;
using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;

namespace AstroLang;

public class Astro
{
	public void Run(string text)
	{
		var diagnosticList = new DiagnosticList();
		
		var source = new SourceText(text);
		var tokens = Scanner.Scan(source, diagnosticList);

		if (diagnosticList.AnyErrors())
		{
			foreach (var diagnostic in diagnosticList.Diagnostics)
				diagnostic.WriteMessage(Console.Out, source);
			
			return;
		}
		
		foreach (var token in tokens)
			Console.WriteLine($"{token}: '{source.GetLexeme(token.Span)}'");
	}
}