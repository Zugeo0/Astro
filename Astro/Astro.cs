using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;
using AstroLang.Runtime;
using AstroLang.Runtime.DataTypes;
using AstroLang.Runtime.NativeModules;
using AstroLang.Runtime.NativeModules.Console;
using AstroLang.Runtime.NativeModules.Time;

namespace AstroLang;

public class Astro
{
	private readonly Runtime.Environment _environment;
	private readonly List<INativeModule> _nativeModules;
	private readonly List<Module> _exposedModules;

	public Astro()
	{
		_environment = new ();
		_exposedModules = new();
		_nativeModules = new()
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
		
		_exposedModules.Add(module.Define());
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
		
		Interpreter.Interpret(syntaxTree, diagnosticList, _exposedModules, _environment);
		
		if (diagnosticList.AnyErrors())
			foreach (var diagnostic in diagnosticList.Diagnostics)
				diagnostic.WriteMessage(Console.Out, source);
	}
}