using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing;

public class Token
{
	public TokenType Type { get; }
	public TextSpan Span { get; }
	public string Lexeme { get; }

	public Token(TokenType type, TextSpan span, string lexeme)
	{
		Type = type;
		Span = span;
		Lexeme = lexeme;
	}
	
	public override string ToString() => $"Token [{Type}]";
}