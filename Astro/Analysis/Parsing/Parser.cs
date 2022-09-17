using AstroLang.Analysis.Parsing.SyntaxNodes;
using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;

namespace AstroLang.Analysis.Parsing;

public class Parser
{
	private readonly Token[] _tokens;
	private readonly DiagnosticList _diagnostics;
	private int _index;
	private bool _inFunction;
	private bool _inLoop;

	private TextSpan TokenSpan => Peek().Span;
	
	private class ParseException : Exception {}
	
	private Parser(Token[] tokens, DiagnosticList diagnostics)
	{
		_tokens = tokens;
		_diagnostics = diagnostics;
	}

	public static SyntaxTree? Parse(SourceText source, DiagnosticList diagnostics)
	{
		var tokens = Scanner.Scan(source, diagnostics);
		if (diagnostics.AnyErrors())
			return null;
		
		var parser = new Parser(tokens, diagnostics);
		
		try
		{
			var program = parser.ParseProgram();
			return new SyntaxTree(program);
		}
		catch (ParseException)
		{
			return null;
		}
	}

	private ProgramSyntax ParseProgram()
	{
		var statements = new List<StatementSyntax>();
		while (!AtEnd())
			statements.Add(ParseDeclaration());

		return new ProgramSyntax(statements.ToArray());
	}

	private StatementSyntax ParseDeclaration()
	{
		if (Peek().Type == TokenType.Var)
			return ParseVariableDeclaration();

		return ParseStatement();
	}

	private StatementSyntax ParseVariableDeclaration(bool allowInitializer = true)
	{
		var varKeyword = Advance();
		var name = Advance();
		var initializer = allowInitializer && Match(TokenType.Equals) ? ParseBinaryExpression() : null;
		var span = initializer is not null
			? varKeyword.Span.ExtendTo(initializer.Span)
			: varKeyword.Span.ExtendTo(name.Span);

		Consume(TokenType.Semicolon, "';' after variable declaration");
		return new VariableDeclarationSyntax(span, name, initializer);
	}
	
	private StatementSyntax ParseStatement()
	{
		switch (Peek().Type)
		{
			case TokenType.LeftBrace:
				return ParseBlockStatement();
			case TokenType.If:
				return ParseIfStatement();
			case TokenType.While:
				return ParseWhileStatement();
			case TokenType.For:
				return ParseForStatement();
			case TokenType.Function:
				return ParseFunctionStatement();
			case TokenType.Return:
				return ParseReturnStatement();
			case TokenType.Break:
				return ParseBreakStatement();
			
			case TokenType.Public:
			case TokenType.Private:
				var accessModifier = Advance();
				
				switch (Peek().Type)
				{
					case TokenType.Class:
						return ParseClassStatement(accessModifier);
				}
				
				_diagnostics.Add(new Diagnostic(TokenSpan, "'class' or 'mod' after access modifier"));
				throw new ParseException();
		}
		
		return ParseExpressionStatement();
	}

	private StatementSyntax ParseBreakStatement()
	{
		var breakKeyword = Advance();
		
		if (!_inLoop)
		{
			_diagnostics.Add(new Diagnostic(breakKeyword.Span, "Break statement not allowed outside of a loop"));
			throw new ParseException();
		}

		Consume(TokenType.Semicolon, "';' after break statement");
		return new BreakStatementSyntax(breakKeyword);
	}
	
	private StatementSyntax ParseReturnStatement()
	{
		var keyword = Advance();
		
		if (!_inFunction)
		{
			_diagnostics.Add(new Diagnostic(keyword.Span, "Return statement not allowed outside of a function or method"));
			throw new ParseException();
		}
		
		var value = ParseBinaryExpression();
		Consume(TokenType.Semicolon, "';' after return statement");

		return new ReturnStatementSyntax(keyword.Span.ExtendTo(value.Span), keyword, value);
	}

	private StatementSyntax ParseClassStatement(Token accessModifier)
	{
		var keyword = Advance();
		var name = Consume(TokenType.Identifier, "identifier after 'class'");
		var leftBrace = Consume(TokenType.LeftBrace, "'{' after class name");
		
		var access = accessModifier.Type switch
		{
			TokenType.Public => AccessModifier.Public,
			TokenType.Private => AccessModifier.Private,
			
			_ => throw new ArgumentException("Invalid access modifier")
		};

		var properties = new List<PropertyDeclarationSyntax>();
		PropertyDeclarationSyntax? constructor = null;
		
		while (!AtEnd() && Peek().Type != TokenType.RightBrace)
		{
			var property = ParseClassProperty();
			if (property.Declaration is FunctionDeclarationSyntax fn && fn.Type == FunctionType.Constructor)
			{
				if (constructor is not null)
				{
					_diagnostics.Add(new Diagnostic(fn.Keyword.Span, "Constructor already defined"));
					throw new ParseException();
				}

				constructor = property;
				continue;
			}

			properties.Add(property);
		}

		Consume(TokenType.RightBrace, "'}' after class body");

		return new ClassDeclarationSyntax(name.Span, access, keyword, name, properties, constructor);
	}

	private PropertyDeclarationSyntax ParseClassProperty()
	{
		var accessModifierToken = Peek();
		var accessModifier = AccessModifier.Private;
		switch (accessModifierToken.Type)
		{
			case TokenType.Public:
				accessModifier = AccessModifier.Public;
				break;
			case TokenType.Private:
				accessModifier = AccessModifier.Private;
				break;
			
			default:
				_diagnostics.Add(new Diagnostic(accessModifierToken.Span, "Expected 'public' or 'private'"));
				throw new ParseException();
		}

		Advance();

		if (Peek().Type == TokenType.Var)
		{
			var declaration = ParseVariableDeclaration(false);
			var span = accessModifierToken.Span.ExtendTo(declaration.Span);
			return new PropertyDeclarationSyntax(span, accessModifier, declaration);
		}

		if (Peek().Type == TokenType.Method)
		{
			var func = ParseFunctionStatement(FunctionType.Method);
			var span = accessModifierToken.Span.ExtendTo(func.Span);
			return new PropertyDeclarationSyntax(span, accessModifier, func);
		}
		
		if (Peek().Type == TokenType.Function)
		{
			var func = ParseFunctionStatement(FunctionType.Function);
			var span = accessModifierToken.Span.ExtendTo(func.Span);
			return new PropertyDeclarationSyntax(span, accessModifier, func);
		}

		if (Peek().Type == TokenType.Constructor)
		{
			var func = ParseFunctionStatement(FunctionType.Constructor);
			var span = accessModifierToken.Span.ExtendTo(func.Span);
			return new PropertyDeclarationSyntax(span, accessModifier, func);
		}

		var errorSpan = new TextSpan(accessModifierToken.Span.Start + accessModifierToken.Span.Length, 1);
		_diagnostics.Add(new Diagnostic(errorSpan, "Expected 'function', 'method' or 'var'"));
		throw new ParseException();
	}

	private FunctionDeclarationSyntax ParseFunctionStatement(FunctionType type = FunctionType.Function)
	{
		var keyword = Advance();
		var name = type != FunctionType.Constructor
			? Consume(TokenType.Identifier, "identifier after 'function'")
			: new(TokenType.Inserted, TokenSpan, "");
		
		var leftParent = Consume(TokenType.LeftParen, "'(' after function name");

		var arguments = new List<Token>();
		if (Peek().Type != TokenType.RightParen)
		{
			do
			{
				var varName = Consume(TokenType.Identifier, "variable name in parameter list");
				if (arguments.Count > 255)
				{
					_diagnostics.Add(new Diagnostic(leftParent.Span.ExtendTo(varName.Span), "Too many parameters in parameter list"));
					throw new ParseException();
				}

				arguments.Add(varName);
			} while (!AtEnd() && Match(TokenType.Comma));
		}
		
		Consume(TokenType.RightParen, "')' after argument list");
		Consume(TokenType.LeftBrace, "'{' after function declaration", false);
		var wasInFunction = _inFunction;
		_inFunction = true;
		var body = ParseBlockStatement();
		_inFunction = wasInFunction;
		
		return new(keyword.Span.ExtendTo(body.Span), type, keyword, name, arguments, body);
	}

	private StatementSyntax ParseForStatement()
	{
		var forToken = Advance();
		
		Consume(TokenType.LeftParen, "'(' after 'for' keyword");
		var initializer = Match(TokenType.Semicolon) ? null : ParseDeclaration();
		var condition = Match(TokenType.Semicolon)
			? new LiteralExpressionSyntax(new Token(TokenType.True, TokenSpan, "true"), TokenSpan)
			: ParseBinaryExpression();
		Consume(TokenType.Semicolon, "';' after condition");
		var finalizer = Peek().Type == TokenType.RightParen ? null : ParseAssignmentExpression();
		var rightParen = Consume(TokenType.RightParen, "')' after for loop declaration");

		var block = new List<StatementSyntax>();
		if (initializer is not null)
			block.Add(initializer);

		_inLoop = true;
		var body = ParseStatement();
		_inLoop = false;
		var bodyBlock = new BlockStatementSyntax(body.Span, new List<StatementSyntax> { body });
		
		if (finalizer is not null)
		{
			var finalizerStatement = new ExpressionStatementSyntax(finalizer);
			bodyBlock.Statements.Add(finalizerStatement);
		}

		block.Add(new WhileStatementSyntax(condition, bodyBlock));
		return new BlockStatementSyntax(forToken.Span.ExtendTo(rightParen.Span), block);
	}

	private StatementSyntax ParseWhileStatement()
	{
		var whileToken = Advance();
		Consume(TokenType.LeftParen, "'(' after 'while' keyword");
		var condition = ParseBinaryExpression();
		Consume(TokenType.RightParen, "')' after condition");

		_inLoop = true;
		var body = ParseStatement();
		_inLoop = false;
		return new WhileStatementSyntax(condition, body);
	}
	
	private StatementSyntax ParseIfStatement()
	{
		var ifToken = Advance();
		Consume(TokenType.LeftParen, "'(' after 'if' keyword");
		var condition = ParseBinaryExpression();
		Consume(TokenType.RightParen, "')' after condition");

		var thenBranch = ParseStatement();
		var elseBranch = Match(TokenType.Else)
			? ParseDeclaration()
			: null;
		var span = ifToken.Span.ExtendTo(elseBranch?.Span ?? thenBranch.Span);
		return new IfStatementSyntax(span, condition, thenBranch, elseBranch);
	}

	private StatementSyntax ParseBlockStatement()
	{
		var leftBrace = Advance();
		var statements = new List<StatementSyntax>();
		while (!AtEnd() && Peek().Type != TokenType.RightBrace)
			statements.Add(ParseDeclaration());
		
		var rightBrace = Consume(TokenType.RightBrace, "'}' after block");
		var span = leftBrace.Span.ExtendTo(rightBrace.Span);
		return new BlockStatementSyntax(span, statements);
	}

	private StatementSyntax ParseExpressionStatement()
	{
		var expression = ParseAssignmentExpression();
		Consume(TokenType.Semicolon, "';' after expression");
		return new ExpressionStatementSyntax(expression);
	}

	private ExpressionSyntax ParseAssignmentExpression()
	{
		var expr = ParseBinaryExpression();

		if (Match(TokenType.Equals))
		{
			var value = ParseAssignmentExpression();
			if (expr is VariableExpressionSyntax v)
				return new AssignExpressionSyntax(v.Name, value);

			if (expr is AccessExpressionSyntax a)
				return new SetExpressionSyntax(a.Object, a.Name, value);
			
			_diagnostics.Add(new Diagnostic(expr.Span, $"Invalid assignment target"));
			throw new ParseException();
		}

		return expr;
	}

	private ExpressionSyntax ParseBinaryExpression(int precedence = 0)
	{
		var expr = ParseUnaryExpression();

		while (true)
		{
			var op = Peek();
			var operatorPrecedence = GetPrecedence(op);

			if (operatorPrecedence == 0 || operatorPrecedence <= precedence)
				break;

			Advance();
			var right = ParseBinaryExpression(operatorPrecedence);
			expr = new BinaryExpressionSyntax(expr, op, right);
		}

		return expr;
	}

	private ExpressionSyntax ParseUnaryExpression()
	{
		var op = Peek();
		if (Match(TokenType.Bang, TokenType.Minus))
			return new UnaryExpressionSyntax(op, ParseUnaryExpression());
			
		return ParseCallExpression();
	}

	private ExpressionSyntax ParseCallExpression()
	{
		var expr = ParsePrimaryExpression();
		var span = expr.Span;

		while (true)
		{
			var firstToken = Peek();
			
			if (Match(TokenType.LeftParen))
			{
				var arguments = new List<ExpressionSyntax>();
				if (Peek().Type != TokenType.RightParen)
				{
					do
					{
						if (arguments.Count == 255)
						{
							_diagnostics.Add(new Diagnostic(TokenSpan, $"Too many arguments in call"));
							throw new ParseException();
						}

						arguments.Add(ParseBinaryExpression());
					} while (Match(TokenType.Comma));
				}

				var rightParen = Consume(TokenType.RightParen, "')' after arguments");
				expr = new CallExpressionSyntax(span.ExtendTo(rightParen.Span), expr, arguments, firstToken, rightParen);
			}
			else if (Match(TokenType.Dot))
			{
				var name = Consume(TokenType.Identifier, "identifier after '.'");
				expr = new AccessExpressionSyntax(expr.Span.ExtendTo(name.Span), expr, name);
			}
			else
				break;
		}

		return expr;
	}

	private ExpressionSyntax ParsePrimaryExpression()
	{
		var span = TokenSpan;
		var token = Peek();

		if (token.Type == TokenType.New)
			return ParseNewExpression();

		if (Match(TokenType.LeftParen))
		{
			var expr = ParseBinaryExpression();
			Consume(TokenType.RightParen, "')' after grouping");
			return expr;
		}

		return ParseLiteral();
	}

	private ExpressionSyntax ParseNewExpression()
	{
		var newKeyword = Advance();
		var expr = ParseLiteral();
		var leftParen = Consume(TokenType.LeftParen, "'(' after class name");
		
		var arguments = new List<ExpressionSyntax>();
		if (Peek().Type != TokenType.RightParen)
		{
			do
			{
				if (arguments.Count == 255)
				{
					_diagnostics.Add(new Diagnostic(TokenSpan, $"Too many arguments in constructor"));
					throw new ParseException();
				}

				arguments.Add(ParseBinaryExpression());
			} while (Match(TokenType.Comma));
		}

		var rightParen = Consume(TokenType.RightParen, "')' after arguments");
		return new NewExpressionSyntax(expr.Span.ExtendTo(rightParen.Span), newKeyword, expr, arguments, leftParen, rightParen);
	}

	private ExpressionSyntax ParseLiteral()
	{
		var token = Peek();
		if (Match(TokenType.Number, TokenType.String, TokenType.True, TokenType.False, TokenType.Null))
			return new LiteralExpressionSyntax(token, token.Span);

		if (Match(TokenType.Identifier))
			return new VariableExpressionSyntax(token);

		_diagnostics.Add(new Diagnostic(TokenSpan, $"Expected a literal"));
		throw new ParseException();
	}

	private int GetPrecedence(Token token)
	{
		switch (token.Type)
		{
			case TokenType.Plus:
			case TokenType.Minus:
				return 1;
			
			case TokenType.Star:
			case TokenType.Slash:
			case TokenType.Percent:
				return 2;
			
			case TokenType.Greater:
			case TokenType.GreaterEquals:
			case TokenType.Lesser:
			case TokenType.LesserEquals:
				return 3;
			
			case TokenType.DoubleEquals:
			case TokenType.BangEquals:
				return 4;
			
			case TokenType.And:
				return 5;
			
			case TokenType.Or:
				return 6;
			
			default: return 0;
		}
	}

	private Token Consume(TokenType type, string expect, bool advance = true)
	{
		var token = Peek();
		if (token.Type == type)
		{
			if (advance)
				Advance();
			return token;
		}

		var prev = PreviousOrFirst();
		var span = new TextSpan(prev.Span.Start + prev.Span.Length, 1);
		_diagnostics.Add(new Diagnostic(span, $"Expected {expect}"));
		throw new ParseException();
	}

	private bool Match(params TokenType[] types) => types.Any(Match);
	private bool Match(TokenType type)
	{
		if (Peek().Type != type)
			return false;
		
		Advance();
		return true;
	}

	private Token Advance() => _tokens[_index++];
	private Token Peek() => _tokens[_index];
	private Token PreviousOrFirst() => _index > 0 ? _tokens[_index - 1] : _tokens[_index];
	private bool AtEnd() => _tokens[_index].Type == TokenType.EndOfFile;
}