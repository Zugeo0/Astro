using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime.DataTypes;

public class Instance : Object, IAccessible, ISettable
{
	public Class Class { get; }
	public Dictionary<string, Object> Fields { get; }

	public Instance(Class @class, Dictionary<string, Object> fields)
	{
		Class = @class;
		Fields = fields;
	}

	public Object Access(Interpreter interpreter, Token name)
	{
		if (Fields.ContainsKey(name.Lexeme))
			return Fields[name.Lexeme];

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
		{
			interpreter.Error(name.Span, $"Member '{name.Lexeme}' is not defined");
			return value;
		}
		
		Fields[name.Lexeme] = value;
		return value;
	}

	public override string ToString() => $"{Class.Name}Instance";
	public override string TypeString() => Class.Name;
}