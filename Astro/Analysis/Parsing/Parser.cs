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

	private StatementSyntax ParseVariableDeclaration()
	{
		var varKeyword = Advance();
		var name = Advance();
		var initializer = Match(TokenType.Equals) ? ParseBinaryExpression() : null;
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
		}
		
		return ParseExpressionStatement();
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

	private StatementSyntax ParseFunctionStatement()
	{
		var keyword = Advance();
		var name = Consume(TokenType.Identifier, "identifier after 'function'");

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
		
		return new FunctionDeclarationSyntax(keyword.Span.ExtendTo(body.Span), keyword, name, arguments, body);
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

		var body = ParseStatement();
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
		
		var body = ParseStatement();
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
			var leftParen = Peek();
			if (!Match(TokenType.LeftParen))
				break;

			var arguments = new List<ExpressionSyntax>();
			if (!AtEnd() && Peek().Type != TokenType.RightParen)
			{
				do
				{
					if (arguments.Count == 255)
					{
						_diagnostics.Add(new Diagnostic(TokenSpan, $"Too many arguments in call"));
						throw new ParseException();
					}
					
					arguments.Add(ParseBinaryExpression());
				}
				while (Match(TokenType.Comma));
			}

			if (AtEnd())
			{
				_diagnostics.Add(new Diagnostic(TokenSpan, $"Expected ')' after arguments"));
				throw new ParseException();
			}

			var rightParen = Consume(TokenType.RightParen, "')' after arguments");
			expr = new CallExpressionSyntax(span.ExtendTo(rightParen.Span), expr, arguments, leftParen, rightParen);
		}

		return expr;
	}

	private ExpressionSyntax ParsePrimaryExpression()
	{
		var span = TokenSpan;
		var token = Peek();

		if (Match(TokenType.LeftParen))
		{
			var expr = ParseBinaryExpression();
			Consume(TokenType.RightParen, "')' after grouping");
			return expr;
		}
		
		if (Match(TokenType.Number, TokenType.String, TokenType.True, TokenType.False, TokenType.Null))
			return new LiteralExpressionSyntax(token, span);

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