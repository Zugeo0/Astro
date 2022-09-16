using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime.NativeModules.Time;

public class ModTime : INativeModule
{
	public Module Define()
	{
		var module = new Module(Name());

		module.AddGlobal("Current", NativeFunction.From<FnCurrent>());
		
		return module;
	}

	public string Name() => "Time";
}