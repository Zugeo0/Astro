using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class WhileStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }

	public ExpressionSyntax Condition { get; }
	public StatementSyntax Body { get; }

	public WhileStatementSyntax(ExpressionSyntax condition, StatementSyntax body)
	{
		Condition = condition;
		Body = body;
		Span = condition.Span;
	}
}