using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class IfStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }

	public ExpressionSyntax Condition { get; }
	public StatementSyntax ThenBranch { get; }
	public StatementSyntax? ElseBranch { get; }

	public IfStatementSyntax(TextSpan span, ExpressionSyntax condition, StatementSyntax thenBranch, StatementSyntax? elseBranch)
	{
		Condition = condition;
		ThenBranch = thenBranch;
		ElseBranch = elseBranch;
		Span = span;
	}
}