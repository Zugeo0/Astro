using AstroLang.Analysis.Parsing.SyntaxNodes;

namespace AstroLang.Runtime.DataTypes;

public class Function : Object, ICallable
{
	private readonly FunctionDeclarationSyntax _function;
	private readonly Environment _closure;

	public Function(FunctionDeclarationSyntax function, Environment closure)
	{
		_function = function;
		_closure = closure;
	}

	public int Arity() => _function.Arguments.Count;

	public Object Call(Interpreter interpreter, List<Object> arguments)
	{
		_closure.BeginScope();

		for (int i = 0; i < Arity(); i++)
			_closure.DeclareLocal(_function.Arguments[i].Lexeme, arguments[i]);

		Object returnValue = new Null();
		
		try
		{
			interpreter.Execute(_function.Body);
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