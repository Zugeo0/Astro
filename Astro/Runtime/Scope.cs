using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime;

public class Scope
{
	private Dictionary<string, DataTypes.Object> _locals = new();

	public DataTypes.Object? GetLocal(string name) => _locals.ContainsKey(name) ? _locals[name] : null;
	public void DeclareLocal(string name, DataTypes.Object value) => _locals.Add(name, value);
	public bool HasLocal(string name) => _locals.ContainsKey(name);
	public void SetLocal(string name, DataTypes.Object value) => _locals[name] = value;
}