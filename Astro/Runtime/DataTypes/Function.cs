using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Parsing.SyntaxNodes;

namespace AstroLang.Runtime.DataTypes;

public class Function : Object, ICallable
{
	public FunctionType Type { get; }
	public List<DeclarationFlag> Flags { get; }
	public readonly FunctionDeclarationSyntax Declaration;
	public readonly Environment Closure;

	public Object? Owner { get; set; }

	public Function(FunctionDeclarationSyntax function, Environment closure, FunctionType type, List<DeclarationFlag> flags)
	{
		Type = type;
		Flags = flags;
		Declaration = function;
		Closure = closure;
	}

	public int Arity() => Declaration.Arguments.Count;

	public Method Bind(Instance instance) => new(Declaration, Closure, instance);
	
	public Object Call(Interpreter interpreter, List<Object> arguments)
	{
		Closure.BeginScope();

		for (int i = 0; i < Arity(); i++)
			Closure.DefineLocal(Declaration.Arguments[i].Lexeme, arguments[i]);

		Object returnValue = new Null();
		
		try
		{
			interpreter.ExecuteFunction(this);
		}
		catch (Interpreter.ReturnException e)
		{
			returnValue = e.Value;
		}
		
		Closure.EndScope();

		return returnValue;
	}

	public override string TypeString() => "function";
	public override string ToString() => "function";
}