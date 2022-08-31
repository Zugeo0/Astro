using AstroLang.Analysis.Parsing.SyntaxNodes;
using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;

namespace AstroLang.Analysis.Parsing;

public class Parser
{
	private readonly Token[] _tokens;
	private readonly DiagnosticList _diagnostics;
	private int _index;

	private TextSpan Span => Peek().Span;
	
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
			statements.Add(ParseStatement());

		return new ProgramSyntax(statements.ToArray());
	}

	private StatementSyntax ParseStatement()
	{
		return ParseExpressionStatement();
	}

	private StatementSyntax ParseExpressionStatement()
	{
		var expression = ParseBinaryExpression();
		Consume(TokenType.Semicolon, "';'");
		return new ExpressionStatementSyntax(expression);
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
			
		return ParsePrimaryExpression();
	}

	private ExpressionSyntax ParsePrimaryExpression()
	{
		var token = Peek();

		if (token.Type == TokenType.Number)
		{
			Advance();
			return new LiteralExpressionSyntax(token);
		}
		
		_diagnostics.Add(new Diagnostic(Span, $"Expected a literal"));
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
			
			default: return 0;
		}
	}

	private Token Consume(TokenType type, string expect)
	{
		var token = Peek();
		if (token.Type == type)
		{
			Advance();
			return token;
		}
		
		_diagnostics.Add(new Diagnostic(Span, $"Expected {expect}"));
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
	private bool AtEnd() => _tokens[_index].Type == TokenType.EndOfFile;
}