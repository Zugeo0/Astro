using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class NewExpressionSyntax : ExpressionSyntax
{
	public override TextSpan Span { get; }
	public Token NewKeyword { get; }
	public ExpressionSyntax Object { get; }
	public List<ExpressionSyntax> Arguments { get; }
	public Token LeftParen { get; }
	public Token RightParen { get; }
	
	public NewExpressionSyntax(TextSpan span, Token newKeyword, ExpressionSyntax @object, List<ExpressionSyntax> arguments, Token leftParen, Token rightParen)
	{
		Span = span;
		NewKeyword = newKeyword;
		Arguments = arguments;
		LeftParen = leftParen;
		RightParen = rightParen;
		Object = @object;
	}
}