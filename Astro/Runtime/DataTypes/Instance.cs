using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime.DataTypes;

public class Instance : Object, IAccessible, ISettable
{
	public Class Class { get; }
	public Dictionary<string, Restricted<Object>> Fields { get; }

	public Instance(Class @class, Dictionary<string, Restricted<Object>> fields)
	{
		Class = @class;
		Fields = fields;

		foreach (var field in fields)
			if (field.Value.Unlock(null) is Function f)
				f.Owner = this;
	}

	public Object Access(Object? accessor, Interpreter interpreter, Token name)
	{
		if (Fields.ContainsKey(name.Lexeme))
		{
			if (!(accessor?.Equals(this) ?? false))
                interpreter.Error(name.Span, $"Property '{name.Lexeme}' is inaccessible");

            return Fields[name.Lexeme].Unlock(accessor);
		}

		var function = Class.GetFunction(name.Lexeme);
		if (function is not null)
		{
			if (function.Type == FunctionType.Method)
				return function.Bind(this);
			
			interpreter.Error(name.Span, $"'{name.Lexeme}' is a static function, not a method");
		}

		return new Null();
	}

	public Object Set(Interpreter interpreter, Token name, Object value)
	{
		if (!Fields.ContainsKey(name.Lexeme))
			interpreter.Error(name.Span, $"Member '{name.Lexeme}' is not defined");
		
		Fields[name.Lexeme].Value = value;
		return value;
	}

	public override string ToString() => $"{Class.Name}Instance";
	public override string TypeString() => Class.Name;
}