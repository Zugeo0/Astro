using AstroLang.Analysis.Parsing.SyntaxNodes;

namespace AstroLang.Runtime.DataTypes;

public class Method : Object, ICallable
{
	private readonly FunctionDeclarationSyntax _function;
	private readonly Environment _closure;
	private readonly Instance _instance;

	public Object? Owner { get; }

	public Method(FunctionDeclarationSyntax function, Environment closure, Instance instance)
	{
		_function = function;
		_closure = closure;
		_instance = instance;
	}

	public int Arity() => _function.Arguments.Count;

	public Object Call(Interpreter interpreter, List<Object> arguments)
	{
		_closure.BeginScope();
		_closure.DefineLocal("this", _instance);

		for (int i = 0; i < Arity(); i++)
			_closure.DefineLocal(_function.Arguments[i].Lexeme, arguments[i]);

		Object returnValue = new Null();
		
		try
		{
			interpreter.ExecuteFunction(_function.Body, _closure);
		}
		catch (Interpreter.ReturnException e)
		{
			returnValue = e.Value;
		}
		
		_closure.EndScope();

		return returnValue;
	}

	public override string TypeString() => "method";
	public override string ToString() => "method";
}