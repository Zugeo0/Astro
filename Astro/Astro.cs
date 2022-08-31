using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;

namespace AstroLang;

public class Astro
{
	public void Run(string text)
	{
		var source = new SourceText(text);
		var diagnosticList = new DiagnosticList();

		var syntaxTree = Parser.Parse(source, diagnosticList);
		syntaxTree?.Print(Console.Out, source);
		
		if (diagnosticList.AnyErrors())
			foreach (var diagnostic in diagnosticList.Diagnostics)
				diagnostic.WriteMessage(Console.Out, source);
	}
}