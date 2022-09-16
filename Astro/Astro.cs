using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;
using AstroLang.Runtime;
using AstroLang.Runtime.NativeModules;
using AstroLang.Runtime.NativeModules.Console;
using AstroLang.Runtime.NativeModules.Time;

namespace AstroLang;

public class Astro
{
	private Runtime.Environment _environment;
	private List<INativeModule> _nativeModules;

	public Astro()
	{
		_environment = new Runtime.Environment();
		_nativeModules = new List<INativeModule>
		{
			new ModTime(),
			new ModConsole()
		};
	}

	public bool ExposeModule(string name)
	{
		var module = _nativeModules.FirstOrDefault(module => module.Name() == name);

		if (module is null)
			return false;
		
		_environment.ExposeModule(module.Define());
		return true;
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