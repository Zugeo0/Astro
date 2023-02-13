using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime.DataTypes;

public class Class : Object, IInstanceable, IAccessible
{
	public string Name { get; }
	public Dictionary<string, Restricted<Object>> Fields { get; }
	public Dictionary<string, Restricted<Function>> Functions { get; }
	public Restricted<Function>? Constructor { get; }

	public Module Owner;

	public Class(string name, Module owner, Dictionary<string, Restricted<Object>> fields, Dictionary<string, Restricted<Function>> functions, Restricted<Function>? constructor)
	{
		Name = name;
		Owner = owner;
		Fields = fields;
		Functions = functions;
		Constructor = constructor;
	}

	public int Arity()
	{
		if (Constructor is not null)
			return Constructor.Value.Arity();
		return 0;
	}

	public Object CreateInstance(Interpreter interpreter, List<Object> arguments)
	{
		var instance = new Instance(this, new(Fields));

		if (Constructor is not null)
			Constructor.Value.Bind(instance).Call(interpreter, arguments);

		return instance;
	}

	public Function? GetFunction(string name)
	{
		return Functions
			.Where(func => func.Key == name)
			.Select(function => function.Value)
			.FirstOrDefault()?
			.Value;
	}

	public Object Access(Object accessor, Interpreter interpreter, Token name)
	{
		if (Functions.ContainsKey(name.Lexeme))
		{
			var func = Functions[name.Lexeme];
			if (func.Value.Type != FunctionType.Function)
				interpreter.Error(name.Span, $"'{name.Lexeme}' is not a static function");
			
			return func.Value;
		}

		interpreter.Error(name.Span, $"Static function '{name.Lexeme}' for '{TypeString()}' is not defined");
		return new Null();
	}

	public override string TypeString() => Name;
	public override string ToString() => Name;
}