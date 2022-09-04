using AstroLang.Analysis.Text;
using AstroLang.Runtime;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class LiteralExpressionSyntax : ExpressionSyntax
{
	public Token Literal { get; }
	public override TextSpan Span { get; }

	public LiteralExpressionSyntax(Token literal, TextSpan span)
	{
		Literal = literal;
		Span = span;
	}
}