using AstroLang.Analysis.Text;
using AstroLang.Runtime;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class BinaryExpressionSyntax : ExpressionSyntax
{
	public ExpressionSyntax Left { get; }
	public Token Operator { get; }
	public ExpressionSyntax Right { get; }
	
	public override TextSpan Span { get; }
	
	public BinaryExpressionSyntax(ExpressionSyntax left, Token op, ExpressionSyntax right)
	{
		Left = left;
		Operator = op;
		Right = right;
		Span = left.Span.SpanTo(right.Span);
	}
}