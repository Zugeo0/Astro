using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class LiteralExpressionSyntax : ExpressionSyntax
{
	public Token Literal { get; }
	
	public override TextSpan Span { get; }

	public LiteralExpressionSyntax(Token literal)
	{
		Literal = literal;
		Span = literal.Span;
	}
}