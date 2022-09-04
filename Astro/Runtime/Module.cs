namespace AstroLang.Runtime;

public class Module
{
	public string Name { get; }
	
	private Dictionary<string, DataTypes.Object> _globals = new();

	public Module(string name)
	{
		Name = name;
	}

	public void AddGlobal(string name, DataTypes.Object value) => _globals.Add(name, value);
	public void RemoveGlobal(string name) => _globals.Remove(name);

	public DataTypes.Object? FindGlobal(string name) => _globals.ContainsKey(name) ? _globals[name] : null;
}