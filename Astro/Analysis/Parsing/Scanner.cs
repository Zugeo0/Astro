using AstroLang.Analysis.Text;
using AstroLang.Diagnostics;

namespace AstroLang.Analysis.Parsing;

public class Scanner
{
	private int _startIndex;
	private int _currentIndex;

	private DiagnosticList _diagnostics;
	private TextSpan Span => new TextSpan(_startIndex, _currentIndex - _startIndex);
	
	private readonly SourceText _sourceText;

	private Scanner(SourceText sourceText, DiagnosticList diagnostics)
	{
		_sourceText = sourceText;
		_diagnostics = diagnostics;
	}

	public static Token[] Scan(SourceText sourceText, DiagnosticList diagnostics)
	{
		var scanner = new Scanner(sourceText, diagnostics);
		return scanner.CreateTokens();
	}

	private Token[] CreateTokens()
	{
		var tokens = new List<Token>();
		
		while (!AtEnd())
		{
			var token = CreateToken();
			if (token is not null)
				tokens.Add(token);
		}
		
		tokens.Add(new Token(TokenType.EndOfFile, Span, "\0"));

		return tokens.ToArray();
	}

	private Token? CreateToken()
	{
		_startIndex = _currentIndex;
		var c = Advance();
		switch (c)
		{
			case '(': return NewToken(TokenType.LeftParen);
			case ')': return NewToken(TokenType.RightParen);
			case '[': return NewToken(TokenType.LeftBracket);
			case ']': return NewToken(TokenType.RightBracket);
			case '{': return NewToken(TokenType.LeftBrace);
			case '}': return NewToken(TokenType.RightBrace);
			
			case '.': return NewToken(TokenType.Dot);
			case ';': return NewToken(TokenType.Semicolon);
			
			case '+': return NewToken(TokenType.Plus);
			case '-': return NewToken(TokenType.Minus);
			case '*': return NewToken(TokenType.Star);
			case '/': return NewToken(TokenType.Slash);
			case '%': return NewToken(TokenType.Percent);
			
			case '=': return NewToken(Match('=') ? TokenType.DoubleEquals : TokenType.Equals);
			case '!': return NewToken(Match('=') ? TokenType.BangEquals : TokenType.Bang);
			case '>': return NewToken(Match('=') ? TokenType.GreaterEquals : TokenType.Greater);
			case '<': return NewToken(Match('=') ? TokenType.LesserEquals : TokenType.Lesser);
			
			case '"':
			case '\'':
				return NewString(c);

			case ' ':
			case '\t':
			case '\r':
			case '\n':
				return null;
		}

		if (IsAlpha(c))
			return NewIdentifier();
		if (IsDigit(c))
			return NewNumber();
		
		_diagnostics.Add(new Diagnostic(Span, $"Unrecognized character '{c}'"));
		return null;
	}

	private Token NewIdentifier()
	{
		while (!AtEnd() && IsAlphaOrDigit(Peek()))
			Advance();

		var lexeme = _sourceText.GetLexeme(Span);
		var type = MatchKeyword(lexeme);
		return NewToken(type);
	}

	private Token NewNumber()
	{
		while (!AtEnd() && IsDigit(Peek()))
			Advance();

		if (AtEnd() || Peek() != '.' || !IsDigit(Peek(1)))
			return NewToken(TokenType.Number);

		do Advance();
		while (IsDigit(Peek()));

		return NewToken(TokenType.Number);
	}

	private Token? NewString(char startingChar)
	{
		while (!AtEnd() && Peek() != startingChar)
			Advance();

		if (AtEnd())
		{
			var span = new TextSpan(Span.Start + Span.Length, 1);
			_diagnostics.Add(new Diagnostic(span, "Unterminated string"));
			return null;
		}

		Advance();
		return NewToken(TokenType.String);
	}

	private static TokenType MatchKeyword(string identifier)
	{
		return identifier switch
		{
			"true"  => TokenType.True,
			"false" => TokenType.False,
			"null"  => TokenType.Null,
			"var"   => TokenType.Var,
			"if"    => TokenType.If,
			"else"  => TokenType.Else,
			
			_       => TokenType.Identifier
		};
	}
	
	private bool Match(char c)
	{
		if (AtEnd() || Peek() != c)
			return false;
		
		Advance();
		return true;
	}
	
	private Token NewToken(TokenType type) => new Token(type, Span, _sourceText.GetLexeme(Span));

	private static bool IsDigit(char c) => c is >= '0' and <= '9';
	private static bool IsAlpha(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
	private static bool IsAlphaOrDigit(char c) => IsDigit(c) || IsAlpha(c);
	
	private char Advance() => _sourceText[_currentIndex++];
	private char Peek(int offset = 0) => _currentIndex + offset >= _sourceText.Length ? '\0' : _sourceText[_currentIndex + offset];
	private bool AtEnd() => _currentIndex >= _sourceText.Length;
}