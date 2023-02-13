using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime.DataTypes;

public class Module : Object, IAccessible
{
	public string Name { get; }

	private Dictionary<string, Restricted<Object>> _properties = new();

	public Module(string name)
	{
		Name = name;
	}

	public Object Access(Object? accessor, Interpreter interpreter, Token name)
	{
        var prop = FindProperty(name.Lexeme);
		if (prop is not null)
		{
			if (prop.Accessability == AccessModifier.Private && !(accessor?.Equals(this) ?? false))
                interpreter.Error(name.Span, $"Cannot access private property '{name.Lexeme}' outside of own module");

			return prop.Unlock(accessor);
        }

        interpreter.Error(name.Span, $"No property named '{name.Lexeme}' defined in module {ToString()}");
		return new Null();
    }

	public void AddProperty(string name, Object value, AccessModifier access) => _properties.Add(name, new(access, value));
	public void RemoveProperty(string name) => _properties.Remove(name);

	public Restricted<Object>? FindProperty(string name) => _properties.ContainsKey(name) ? _properties[name] : null;

	public Function? FindEntry()
	{
		foreach (var values in _properties.Values)
			if (values.Unlock(null) is Function f && f.Flags.Exists(flag => flag == DeclarationFlag.MainFunction))
				return f;
		
		return null;
	}

	public override string TypeString() => "Module";

	public override string ToString() => $"{Name}Module";
}