using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class BinaryExpressionSyntax : ExpressionSyntax
{
	public ExpressionSyntax Left { get; }
	public Token Operator { get; }
	public ExpressionSyntax Right { get; }
	
	public override TextSpan Span { get; }
	
	public BinaryExpressionSyntax(ExpressionSyntax left, Token @operator, ExpressionSyntax right)
	{
		Left = left;
		Operator = @operator;
		Right = right;
		Span = left.Span.SpanTo(right.Span);
	}
}