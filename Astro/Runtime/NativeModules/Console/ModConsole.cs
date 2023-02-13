using AstroLang.Analysis.Parsing;
using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime.NativeModules.Console;

public class ModConsole : INativeModule
{
	public Module Define()
	{
		var module = new Module(Name());

		module.AddProperty("Out", NativeFunction.From<FnOut>(), AccessModifier.Public);
		
		return module;
	}

	public string Name() => "Console";
}