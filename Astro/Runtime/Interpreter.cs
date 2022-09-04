using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Parsing.SyntaxNodes;
using AstroLang.Runtime.DataTypes;
using AstroLang.Diagnostics;

namespace AstroLang.Runtime;

public class Interpreter
{
	private class InterpretException : Exception {}
	
	private readonly DiagnosticList _diagnostics;
	private readonly Environment _environment;
	
	public Interpreter(DiagnosticList diagnostics, Environment environment)
	{
		_diagnostics = diagnostics;
		_environment = environment;
	}

	public static void Interpret(SyntaxTree syntaxTree, DiagnosticList diagnostics, Environment? environment = null)
	{
		environment ??= new Environment();
		
		var interpreter = new Interpreter(diagnostics, environment);
		try
		{
			foreach (var statement in syntaxTree.Root.Statements)
				interpreter.Execute(statement);
		}
		catch (InterpretException) { }
	}

	private void Execute(StatementSyntax statement)
	{
		switch (statement)
		{
			case ExpressionStatementSyntax s:
				var result = Evaluate(s.Expression);
				Console.WriteLine($"{result}");
				break;
			case VariableDeclarationSyntax s:
				ExecuteVariableDeclaration(s);
				break;
			case BlockStatementSyntax s:
				ExecuteBlockStatement(s);
				break;
			case IfStatementSyntax s:
				ExecuteIfStatement(s);
				break;
		}
	}

	private void ExecuteVariableDeclaration(VariableDeclarationSyntax declaration)
	{
		var value = declaration.Initializer is not null ? Evaluate(declaration.Initializer) : new Null();
		_environment.DeclareVariable(declaration.Name.Lexeme, value);
	}

	private void ExecuteIfStatement(IfStatementSyntax ifStatementSyntax)
	{
		var result = Evaluate(ifStatementSyntax.Condition);
		if (result.IsTruthful())
			Execute(ifStatementSyntax.ThenBranch);
		else if (ifStatementSyntax.ElseBranch is not null)
			Execute(ifStatementSyntax.ElseBranch);
	}

	private void ExecuteBlockStatement(BlockStatementSyntax blockStatement)
	{
		_environment.BeginScope();
		foreach (var statement in blockStatement.Statements)
			Execute(statement);
		_environment.EndScope();
	}
	
	private DataTypes.Object Evaluate(ExpressionSyntax expression)
	{
		switch (expression)
		{
			case LiteralExpressionSyntax e:
				return EvaluateLiteral(e.Literal);
			case AssignExpressionSyntax e:
				return EvaluateAssignExpression(e.Name, Evaluate(e.Value));
			case VariableExpressionSyntax e:
				return EvaluateVariable(e.Name);
			case UnaryExpressionSyntax e:
				return EvaluateUnary(e.Operator, Evaluate(e.Right));
			case BinaryExpressionSyntax e:
				return EvaluateBinary(e.Operator, Evaluate(e.Left), Evaluate(e.Right));
		}

		throw new Exception("Invalid Expression");
	}

	private DataTypes.Object EvaluateBinary(Token op, DataTypes.Object left, DataTypes.Object right)
	{
		switch (op.Type)
		{
			case TokenType.Plus when left is Number l && right is Number r:
				return l + r;
			case TokenType.Minus when left is Number l && right is Number r:
				return l - r;
			case TokenType.Star when left is Number l && right is Number r:
				return l * r;
			case TokenType.Slash when left is Number l && right is Number r:
				return l / r;
			
			case TokenType.Lesser when left is Number l && right is Number r:
				return l < r;
			case TokenType.LesserEquals when left is Number l && right is Number r:
				return l <= r;
			case TokenType.Greater when left is Number l && right is Number r:
				return l > r;
			case TokenType.GreaterEquals when left is Number l && right is Number r:
				return l >= r;

			case TokenType.DoubleEquals when left is Number l && right is Number r:
				return l == r;
			case TokenType.BangEquals when left is Number l && right is Number r:
				return l != r;
			
			case TokenType.DoubleEquals when left is Bool l && right is Bool r:
				return l == r;
			case TokenType.BangEquals when left is Bool l && right is Bool r:
				return l != r;
			
			case TokenType.DoubleEquals when left is DataTypes.String l && right is DataTypes.String r:
				return l == r;
			case TokenType.BangEquals when left is DataTypes.String l && right is DataTypes.String r:
				return l != r;
			
			case TokenType.DoubleEquals:
				return left == right;
			case TokenType.BangEquals:
				return left != right;
			
			default:
				_diagnostics.Add(new Diagnostic(op.Span, $"Invalid binary operation '{op.Lexeme}' for types {left} and {right}"));
				throw new InterpretException();
		}
	}

	private DataTypes.Object EvaluateUnary(Token op, DataTypes.Object value)
	{
		switch (op.Type)
		{
			case TokenType.Bang:
				return !value;
			case TokenType.Minus when value is Number v:
				return -v;
			default:
				_diagnostics.Add(new Diagnostic(op.Span, $"Invalid unary operation '{op.Lexeme}' for type {value}"));
				throw new InterpretException();
		}
	}

	private DataTypes.Object EvaluateAssignExpression(Token name, DataTypes.Object value)
	{
		if (_environment.AssignVariable(name.Lexeme, value))
			return value;
		
		_diagnostics.Add(new Diagnostic(name.Span, $"Local variable '{name.Lexeme}' is not defined"));
		throw new InterpretException();
	}

	private static DataTypes.Object EvaluateLiteral(Token literal)
	{
		return literal.Type switch
		{
			TokenType.Number => new Number(literal.Lexeme),
			TokenType.True => new Bool(true),
			TokenType.False => new Bool(false),
			TokenType.String => new DataTypes.String(literal.Lexeme.Substring(1, literal.Lexeme.Length - 2)),
			TokenType.Null => new Null(),
			
			_ => throw new Exception("Invalid literal"),
		};
	}

	private DataTypes.Object EvaluateVariable(Token name)
	{
		var variable = _environment.FindVariable(name.Lexeme);
		if (variable is not null)
			return variable;
		
		_diagnostics.Add(new Diagnostic(name.Span, $"Variable '{name.Lexeme}' is not defined"));
		throw new InterpretException();
	}
}