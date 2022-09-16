using AstroLang.Analysis.Text;
using AstroLang.Runtime;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class UnaryExpressionSyntax : ExpressionSyntax
{
	public Token Operator { get; }
	public ExpressionSyntax Right { get; }
	
	public override TextSpan Span { get; }

	public UnaryExpressionSyntax(Token op, ExpressionSyntax right)
	{
		Operator = op;
		Right = right;
		Span = op.Span.ExtendTo(right.Span);
	}
}