using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class ExpressionStatementSyntax : StatementSyntax
{
	public ExpressionSyntax Expression { get; }
	public override TextSpan Span { get; }

	public ExpressionStatementSyntax(ExpressionSyntax expression)
	{
		Expression = expression;
		Span = expression.Span;
	}
}