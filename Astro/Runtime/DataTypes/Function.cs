using AstroLang.Analysis.Parsing.SyntaxNodes;

namespace AstroLang.Runtime.DataTypes;

public class Function : Object, ICallable
{
	private FunctionDeclarationSyntax _function { get; }

	public Function(FunctionDeclarationSyntax function)
	{
		_function = function;
	}

	public int Arity() => _function.Arguments.Count;

	public Object Call(Interpreter interpreter, List<Object> arguments)
	{
		interpreter.Environment.BeginScope();

		for (int i = 0; i < Arity(); i++)
			interpreter.Environment.DeclareLocal(_function.Arguments[i].Lexeme, arguments[i]);

		interpreter.Execute(_function.Body);
		interpreter.Environment.EndScope();

		return new Null();
	}

	public override string TypeString() => "function";
	public override string ToString() => "function";
}