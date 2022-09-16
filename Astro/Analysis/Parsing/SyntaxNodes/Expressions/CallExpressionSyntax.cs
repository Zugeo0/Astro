using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class CallExpressionSyntax : ExpressionSyntax
{
	public override TextSpan Span { get; }
	
	public ExpressionSyntax Callee { get; }
	public List<ExpressionSyntax> Arguments { get; }
	public Token LeftParen { get; }
	public Token RightParen { get; }
	
	public CallExpressionSyntax(TextSpan span, ExpressionSyntax callee, List<ExpressionSyntax> arguments, Token leftParen, Token rightParen)
	{
		Span = span;
		Callee = callee;
		Arguments = arguments;
		LeftParen = leftParen;
		RightParen = rightParen;
	}
}