using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime.DataTypes;

public class Module : Object, IAccessible
{
	public string Name { get; }
	
	private Dictionary<string, Object> _globals = new();

	public Module(string name)
	{
		Name = name;
	}

	public Object Access(Interpreter interpreter, Token name)
	{
		return FindGlobal(name.Lexeme);
	}

	public void AddGlobal(string name, DataTypes.Object value) => _globals.Add(name, value);
	public void RemoveGlobal(string name) => _globals.Remove(name);

	public Object FindGlobal(string name) => _globals.ContainsKey(name) ? _globals[name] : new Null();
}