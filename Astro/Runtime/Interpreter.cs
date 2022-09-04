using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Parsing.SyntaxNodes;
using AstroLang.Runtime.DataTypes;
using AstroLang.Diagnostics;

namespace AstroLang.Runtime;

public class Interpreter
{
	private class InterpretException : Exception {}
	
	private readonly DiagnosticList _diagnostics;

	public Interpreter(DiagnosticList diagnostics)
	{
		_diagnostics = diagnostics;
	}

	public static void Interpret(SyntaxTree syntaxTree, DiagnosticList diagnostics)
	{
		var interpreter = new Interpreter(diagnostics);
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
			case ExpressionStatementSyntax e:
				var result = Evaluate(e.Expression);
				Console.WriteLine($"{result}");
				
				break;
		}
	}

	private DataTypes.Object Evaluate(ExpressionSyntax expression)
	{
		switch (expression)
		{
			case LiteralExpressionSyntax e:
				return EvaluateLiteral(e.Literal);
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
}