using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class UnaryExpressionSyntax : ExpressionSyntax
{
	public Token Operator { get; }
	public ExpressionSyntax Right { get; }
	
	public override TextSpan Span { get; }

	public UnaryExpressionSyntax(Token @operator, ExpressionSyntax right)
	{
		Operator = @operator;
		Right = right;
		Span = @operator.Span.SpanTo(right.Span);
	}
}