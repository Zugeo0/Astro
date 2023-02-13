using AstroLang.Analysis.Parsing;
using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime.NativeModules.Time;

public class ModTime : INativeModule
{
	public Module Define()
	{
		var module = new Module(Name());

		module.AddProperty("Current", NativeFunction.From<FnCurrent>(), AccessModifier.Public);
		
		return module;
	}

	public string Name() => "Time";
}