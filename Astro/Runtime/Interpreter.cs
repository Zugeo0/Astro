using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Parsing.SyntaxNodes;
using AstroLang.Runtime.DataTypes;
using AstroLang.Diagnostics;
using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime;

public class Interpreter
{
	private class InterpretException : Exception {}

	public class ReturnException : Exception
	{
		public DataTypes.Object Value { get; }

		public ReturnException(Object value)
		{
			Value = value;
		}
	}
	
	private readonly DiagnosticList _diagnostics;

	internal Environment Environment { get; private set; }
	
	public Interpreter(DiagnosticList diagnostics, Environment environment)
	{
		_diagnostics = diagnostics;
		Environment = environment;
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

	internal void Execute(StatementSyntax statement)
	{
		switch (statement)
		{
			case ExpressionStatementSyntax s:
				Evaluate(s.Expression);
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
			case WhileStatementSyntax s:
				ExecuteWhileStatement(s);
				break;
			case FunctionDeclarationSyntax s:
				ExecuteFunctionDeclaration(s);
				break;
			case ReturnStatementSyntax s:
				ExecuteReturnStatement(s);
				break;
		}
	}

	private void ExecuteReturnStatement(ReturnStatementSyntax statement)
	{
		var value = Evaluate(statement.Value);
		throw new ReturnException(value);
	}
	
	private void ExecuteFunctionDeclaration(FunctionDeclarationSyntax declaration)
	{
		var function = new Function(declaration, Environment.Reference());
		Environment.DeclareLocal(declaration.Name.Lexeme, function);
	}

	private void ExecuteVariableDeclaration(VariableDeclarationSyntax declaration)
	{
		if (Environment.LocalDeclaredInCurrentScope(declaration.Name.Lexeme))
		{
			_diagnostics.Add(new Diagnostic(declaration.Name.Span, $"Variable '{declaration.Name.Lexeme}' has already been declared"));
			throw new InterpretException();
		}
		
		var value = declaration.Initializer is not null ? Evaluate(declaration.Initializer) : new Null();
		Environment.DeclareLocal(declaration.Name.Lexeme, value);
	}

	private void ExecuteWhileStatement(WhileStatementSyntax whileStatement)
	{
		while (Evaluate(whileStatement.Condition).IsTruthful())
			Execute(whileStatement.Body);
	}

	private void ExecuteIfStatement(IfStatementSyntax ifStatement)
	{
		var result = Evaluate(ifStatement.Condition);
		if (result.IsTruthful())
			Execute(ifStatement.ThenBranch);
		else if (ifStatement.ElseBranch is not null)
			Execute(ifStatement.ElseBranch);
	}

	private void ExecuteBlockStatement(BlockStatementSyntax blockStatement)
	{
		Environment.BeginScope();
		foreach (var statement in blockStatement.Statements)
			Execute(statement);
		Environment.EndScope();
	}
	
	internal DataTypes.Object Evaluate(ExpressionSyntax expression)
	{
		switch (expression)
		{
			case LiteralExpressionSyntax e:
				return EvaluateLiteral(e);
			case AssignExpressionSyntax e:
				return EvaluateAssign(e);
			case VariableExpressionSyntax e:
				return EvaluateVariable(e);
			case UnaryExpressionSyntax e:
				return EvaluateUnary(e);
			case BinaryExpressionSyntax e:
				return EvaluateBinary(e);
			case CallExpressionSyntax e:
				return EvaluateCall(e);
			case AccessExpressionSyntax e:
				return EvaluateAccess(e);
		}

		throw new Exception("Invalid Expression");
	}

	private DataTypes.Object EvaluateBinary(BinaryExpressionSyntax binaryExpression)
	{
		var left = Evaluate(binaryExpression.Left);
		switch (binaryExpression.Operator.Type)
		{
			case TokenType.And:
			{
				if (!left.IsTruthful())
					return left;
				
				return Evaluate(binaryExpression.Right);
			}
			case TokenType.Or:
			{
				if (left.IsTruthful())
					return left;
				
				return Evaluate(binaryExpression.Right);
			}
		}
		
		var right = Evaluate(binaryExpression.Right);
		switch (binaryExpression.Operator.Type)
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
				_diagnostics.Add(new Diagnostic(binaryExpression.Operator.Span, $"Invalid binary operation '{binaryExpression.Operator.Lexeme}' for types {left} and {right}"));
				throw new InterpretException();
		}
	}

	private DataTypes.Object EvaluateUnary(UnaryExpressionSyntax unaryExpression)
	{
		var value = Evaluate(unaryExpression.Right);
		
		switch (unaryExpression.Operator.Type)
		{
			case TokenType.Bang:
				return !value;
			case TokenType.Minus when value is Number v:
				return -v;
			default:
				_diagnostics.Add(new Diagnostic(unaryExpression.Operator.Span, $"Invalid unary operation '{unaryExpression.Operator.Lexeme}' for type {value}"));
				throw new InterpretException();
		}
	}

	private DataTypes.Object EvaluateAccess(AccessExpressionSyntax accessExpression)
	{
		var obj = Evaluate(accessExpression.Object);
		if (obj is not IAccessible a)
		{
			_diagnostics.Add(new Diagnostic(accessExpression.Span, $"Object of type '{obj.TypeString()}' is not accessible"));
			throw new InterpretException();
		}

		return a.Access(this, accessExpression.Name.Lexeme);
	}

	private DataTypes.Object EvaluateAssign(AssignExpressionSyntax assignExpression)
	{
		var name = assignExpression.Name.Lexeme;
		var value = Evaluate(assignExpression.Value);
		
		if (Environment.AssignLocal(name, value))
			return value;
		
		_diagnostics.Add(new Diagnostic(assignExpression.Name.Span, $"Local variable '{name}' is not defined"));
		throw new InterpretException();
	}

	private DataTypes.Object EvaluateCall(CallExpressionSyntax call)
	{
		var callee = Evaluate(call.Callee);

		var arguments = call.Arguments
			.Select(Evaluate)
			.ToList();

		if (callee is not ICallable callable)
		{
			_diagnostics.Add(new Diagnostic(call.Span, $"Type '{callee.TypeString()}' is not callable"));
			throw new InterpretException();
		}

		if (arguments.Count != callable.Arity() && callable.Arity() != -1)
		{
			var span = call.LeftParen.Span.ExtendTo(call.RightParen.Span);
			_diagnostics.Add(new Diagnostic(span, $"Incorrect number of arguments. Expected {callable.Arity()}, got {arguments.Count}"));
			throw new InterpretException();
		}

		return callable.Call(this, arguments);
	}

	private static DataTypes.Object EvaluateLiteral(LiteralExpressionSyntax literal)
	{
		var lexeme = literal.Literal.Lexeme;
		return literal.Literal.Type switch
		{
			TokenType.Number => new Number(lexeme),
			TokenType.True => new Bool(true),
			TokenType.False => new Bool(false),
			TokenType.String => new DataTypes.String(lexeme.Substring(1, lexeme.Length - 2)),
			TokenType.Null => new Null(),
			
			_ => throw new Exception("Invalid literal"),
		};
	}

	private DataTypes.Object EvaluateVariable(VariableExpressionSyntax variableExpression)
	{
		var variable = Environment.FindVariable(variableExpression.Name.Lexeme);
		if (variable is not null)
			return variable;
		
		_diagnostics.Add(new Diagnostic(variableExpression.Name.Span, $"Variable '{variableExpression.Name.Lexeme}' is not defined"));
		throw new InterpretException();
	}
}