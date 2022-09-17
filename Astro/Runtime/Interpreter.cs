using AstroLang.Analysis.Parsing;
using AstroLang.Analysis.Parsing.SyntaxNodes;
using AstroLang.Analysis.Text;
using AstroLang.Runtime.DataTypes;
using AstroLang.Diagnostics;
using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime;

public class Interpreter
{
	private class InterpretException : Exception {}
	
	public class BreakException: Exception {}
	public class ReturnException : Exception
	{
		public DataTypes.Object Value { get; }

		public ReturnException(Object value)
		{
			Value = value;
		}
	}
	
	private readonly DiagnosticList _diagnostics;
	private readonly List<Module> _availableModules;

	internal Environment Environment { get; private set; }
	
	public Interpreter(DiagnosticList diagnostics, Environment environment, List<Module> availableModules)
	{
		_diagnostics = diagnostics;
		_availableModules = availableModules;
		Environment = environment;
	}

	public static void Interpret(SyntaxTree syntaxTree, DiagnosticList diagnostics, List<Module> availableModules, Environment? environment = null)
	{
		environment ??= new Environment();
		
		var interpreter = new Interpreter(diagnostics, environment, availableModules);
		try
		{
			foreach (var statement in syntaxTree.Root.Statements)
				interpreter.Execute(statement);
		}
		catch (InterpretException)
		{
			return;
		}
		
		try
		{
			var entry = environment.FindEntry();
			if (entry is not null)
				entry.Call(interpreter, new());
			else
				diagnostics.Add(new Diagnostic(new TextSpan(syntaxTree.Root.Span.Start + syntaxTree.Root.Span.Length, 1), "No entry point defined"));
		}
		catch (InterpretException) { }
	}

	internal void Execute(StatementSyntax statement, Environment? environment = null)
	{
		var prevEnvironment = Environment;
		if (environment is not null)
			Environment = environment;
		
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
			case ClassDeclarationSyntax s:
				ExecuteClassDeclaration(s);
				break;
			case BreakStatementSyntax s:
				ExecuteBreakStatement(s);
				break;
			case RequireStatementSyntax s:
				ExecuteRequireStatement(s);
				break;
			case ModDeclarationSyntax s:
				ExecuteModDeclaration(s);
				break;
		}
		
		if (environment is not null)
			Environment = prevEnvironment;
	}

	private void ExecuteModDeclaration(ModDeclarationSyntax mod)
	{
		Environment.BeginModule(this, mod.Name, mod.AccessModifier);

		foreach (var declaration in mod.Declarations)
			Execute(declaration);
		
		Environment.EndModule();
	}

	private void ExecuteRequireStatement(RequireStatementSyntax require)
	{
		Module? module = null;
		foreach (var availableModule in _availableModules)
			if (availableModule.Name == require.ModName.Lexeme)
				module = availableModule;

		if (module is null)
		{
			_diagnostics.Add(new Diagnostic(require.ModName.Span, $"Module '{require.ModName.Lexeme}' does not exist"));
			throw new InterpretException();
		}
		
		Environment.AddModule(this, module, require.Alias ?? require.ModName);
	}
	
	private void ExecuteClassDeclaration(ClassDeclarationSyntax declaration)
	{
		var fields = new Dictionary<string, Object>();
		var functions = new Dictionary<string, Function>();
		
		Environment.BeginScope();

		foreach (var property in declaration.Properties)
		{
			switch (property.Declaration)
			{
				case FunctionDeclarationSyntax function:
					var func = new Function(function, Environment, function.Type, function.Accessability, function.Flags);
					functions.Add(function.Name.Lexeme, func);
					
					break;
				case VariableDeclarationSyntax member:
					fields.Add(member.Name.Lexeme, new Null());
					break;

				default:
					throw new Exception("Invalid property type");
			}
		}

		Function? constructor = null;
		if (declaration.Constructor?.Declaration is FunctionDeclarationSyntax fn)
			constructor = new(fn, Environment, fn.Type, fn.Accessability, fn.Flags);
		
		var @class = new Class(declaration.Name.Lexeme, fields, functions, constructor);
		Environment.EndScope();
		Environment.DefineProperty(declaration.Name.Lexeme, @class, declaration.Accessability);
	}

	private void ExecuteReturnStatement(ReturnStatementSyntax statement)
	{
		var value = Evaluate(statement.Value);
		throw new ReturnException(value);
	}
	
	private void ExecuteFunctionDeclaration(FunctionDeclarationSyntax declaration)
	{
		var function = new Function(declaration, Environment.ReferenceSnapshot(), declaration.Type, declaration.Accessability, declaration.Flags);
		Environment.DefineProperty(declaration.Name.Lexeme, function, declaration.Accessability);
	}

	private void ExecuteVariableDeclaration(VariableDeclarationSyntax declaration)
	{
		if (Environment.LocalDeclaredInCurrentScope(declaration.Name.Lexeme))
		{
			_diagnostics.Add(new Diagnostic(declaration.Name.Span, $"Variable '{declaration.Name.Lexeme}' has already been declared"));
			throw new InterpretException();
		}
		
		var value = declaration.Initializer is not null ? Evaluate(declaration.Initializer) : new Null();
		Environment.DefineLocal(declaration.Name.Lexeme, value);
	}

	private void ExecuteBreakStatement(BreakStatementSyntax breakStatement)
	{
		throw new BreakException();
	}
	
	private void ExecuteWhileStatement(WhileStatementSyntax whileStatement)
	{
		while (Evaluate(whileStatement.Condition).IsTruthful())
		{
			try
			{
				Execute(whileStatement.Body);
			}
			catch (BreakException)
			{
				break;
			}
		}
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
			case SetExpressionSyntax e:
				return EvaluateSet(e);
			case NewExpressionSyntax e:
				return EvaluateNew(e);
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

	private DataTypes.Object EvaluateNew(NewExpressionSyntax newExpression)
	{
		var obj = Evaluate(newExpression.Object);
		if (obj is IInstanceable instanceable)
		{
			if (newExpression.Arguments.Count != instanceable.Arity())
			{
				_diagnostics.Add(new Diagnostic(newExpression.Span, $"Incorrect number of arguments. Expected {instanceable.Arity()}, got {newExpression.Arguments.Count}"));
				throw new InterpretException();
			}

			var arguments = new List<DataTypes.Object>();
			foreach (var arg in newExpression.Arguments)
				arguments.Add(Evaluate(arg));

			return instanceable.CreateInstance(this, arguments);
		}
		
		_diagnostics.Add(new Diagnostic(newExpression.Object.Span, $"Object of type '{obj.TypeString()}' is not instanceable"));
		throw new InterpretException();
	}

	private DataTypes.Object EvaluateAccess(AccessExpressionSyntax accessExpression)
	{
		var obj = Evaluate(accessExpression.Object);
		if (obj is not IAccessible a)
		{
			_diagnostics.Add(new Diagnostic(accessExpression.Span, $"Object of type '{obj.TypeString()}' is not accessible"));
			throw new InterpretException();
		}

		return a.Access(this, accessExpression.Name);
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
	
	private DataTypes.Object EvaluateSet(SetExpressionSyntax setExpression)
	{
		var target = Evaluate(setExpression.Target);
		var value = Evaluate(setExpression.Value);
		
		if (target is ISettable settable)
			return settable.Set(this, setExpression.Name, value);
		
		_diagnostics.Add(new Diagnostic(setExpression.Target.Span, $"Invalid assignment target"));
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

	internal void Error(TextSpan span, string message)
	{
		_diagnostics.Add(new(span, message));
		throw new InterpretException();
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