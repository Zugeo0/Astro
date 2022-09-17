using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime.DataTypes;

public class Class : Object, IInstanceable, IAccessible
{
	public string Name { get; }
	public Dictionary<string, Object> Fields { get; }
	public Dictionary<string, Function> Functions { get; }
	public Function? Constructor { get; }

	public Class(string name, Dictionary<string, Object> fields, Dictionary<string, Function> functions, Function? constructor)
	{
		Name = name;
		Fields = fields;
		Functions = functions;
		Constructor = constructor;
	}

	public int Arity()
	{
		if (Constructor is not null)
			return Constructor.Arity();
		return 0;
	}

	public Object CreateInstance(Interpreter interpreter, List<Object> arguments)
	{
		var instance = new Instance(this, new(Fields));

		if (Constructor is not null)
			Constructor.Bind(instance).Call(interpreter, arguments);

		return instance;
	}

	public Function? GetFunction(string name)
	{
		return Functions
			.Where(func => func.Key == name)
			.Select(function => function.Value)
			.FirstOrDefault();
	}

	public Object Access(Interpreter interpreter, Token name)
	{
		if (Functions.ContainsKey(name.Lexeme))
		{
			var func = Functions[name.Lexeme];
			if (func.Type != FunctionType.Function)
				interpreter.Error(name.Span, $"'{name.Lexeme}' is not a static function");
			
			return func;
		}

		interpreter.Error(name.Span, $"Static function '{name.Lexeme}' for '{TypeString()}' is not defined");
		return new Null();
	}

	public override string TypeString() => Name;
	public override string ToString() => Name;
}