using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime.NativeModules.Console;

public class ModConsole : INativeModule
{
	public Module Define()
	{
		var module = new Module(Name());

		module.AddGlobal("Out", NativeFunction.From<FnOut>());
		
		return module;
	}

	public string Name() => "Console";
}