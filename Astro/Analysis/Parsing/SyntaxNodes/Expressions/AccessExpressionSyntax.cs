using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class AccessExpressionSyntax : ExpressionSyntax
{
	public override TextSpan Span { get; }
	public ExpressionSyntax Object { get; }
	public Token Name { get; }

	public AccessExpressionSyntax(TextSpan span, ExpressionSyntax obj, Token name)
	{
		Span = span;
		Object = obj;
		Name = name;
	}
}