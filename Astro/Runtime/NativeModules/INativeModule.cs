using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime.NativeModules;

public interface INativeModule
{
	public Module Define();

	public string Name();
}