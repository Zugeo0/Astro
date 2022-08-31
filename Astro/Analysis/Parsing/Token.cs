using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing;

public class Token
{
	public TokenType Type { get; }
	public TextSpan Span { get; }

	public Token(TokenType type, TextSpan span)
	{
		Type = type;
		Span = span;
	}
	
	public override string ToString() => $"Token [{Type}]";
}