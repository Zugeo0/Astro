using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Parsing.SyntaxNodes;

namespace AstroLang.Runtime.DataTypes;

public class Function : Object, ICallable
{
	public FunctionType Type { get; }
	public AccessModifier AccessModifier { get; }
	public List<DeclarationFlag> Flags { get; }
	private readonly FunctionDeclarationSyntax _function;
	private readonly Environment _closure;

	public Function(FunctionDeclarationSyntax function, Environment closure, FunctionType type, AccessModifier accessModifier, List<DeclarationFlag> flags)
	{
		Type = type;
		AccessModifier = accessModifier;
		Flags = flags;
		_function = function;
		_closure = closure;
	}

	public int Arity() => _function.Arguments.Count;

	public Method Bind(Instance instance) => new(_function, _closure, instance);
	
	public Object Call(Interpreter interpreter, List<Object> arguments)
	{
		_closure.BeginScope();

		for (int i = 0; i < Arity(); i++)
			_closure.DefineLocal(_function.Arguments[i].Lexeme, arguments[i]);

		Object returnValue = new Null();
		
		try
		{
			interpreter.Execute(_function.Body, _closure);
		}
		catch (Interpreter.ReturnException e)
		{
			returnValue = e.Value;
		}
		
		_closure.EndScope();

		return returnValue;
	}

	public override string TypeString() => "function";
	public override string ToString() => "function";
}