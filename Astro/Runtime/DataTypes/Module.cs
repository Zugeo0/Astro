using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime.DataTypes;

public class Module : Object, IAccessible
{
	private record ModuleGlobal(Object Global, AccessModifier Accessability);
	
	public string Name { get; }
	public AccessModifier Accessability { get; }

	private Dictionary<string, ModuleGlobal> _globals = new();

	public Module(string name, AccessModifier accessability)
	{
		Name = name;
		Accessability = accessability;
	}

	public Object Access(Interpreter interpreter, Token name)
	{
		return FindProperty(name.Lexeme) ?? new Null();
	}

	public void AddProperty(string name, Object value, AccessModifier access) => _globals.Add(name, new(value, access));
	public void RemoveProperty(string name) => _globals.Remove(name);

	public Object? FindProperty(string name) => _globals.ContainsKey(name) ? _globals[name].Global : null;

	public Function? FindEntry()
	{
		foreach (var values in _globals.Values)
			if (values.Global is Function f && f.Flags.Exists(flag => flag == DeclarationFlag.MainFunction))
				return f;
		
		return null;
	}

	public override string TypeString() => "Module";

	public override string ToString() => $"{Name}Module";
}