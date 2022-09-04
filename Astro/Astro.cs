using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;
using AstroLang.Runtime;

namespace AstroLang;

public class Astro
{
	private Runtime.Environment _environment;

	public Astro()
	{
		_environment = new Runtime.Environment();
	}
	
	public void Run(string text)
	{
		var source = new SourceText(text);
		var diagnosticList = new DiagnosticList();

		var syntaxTree = Parser.Parse(source, diagnosticList);
		//syntaxTree?.Print(Console.Out);
		
		if (diagnosticList.AnyErrors())
			foreach (var diagnostic in diagnosticList.Diagnostics)
				diagnostic.WriteMessage(Console.Out, source);

		if (syntaxTree is null)
			return;
		
		Interpreter.Interpret(syntaxTree, diagnosticList, _environment);
		
		if (diagnosticList.AnyErrors())
			foreach (var diagnostic in diagnosticList.Diagnostics)
				diagnostic.WriteMessage(Console.Out, source);
	}
}