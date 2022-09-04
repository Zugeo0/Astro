using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class BlockStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	
	public List<StatementSyntax> Statements { get; }

	public BlockStatementSyntax(Token leftBrace, List<StatementSyntax> statements)
	{
		Span = statements.Count > 0 
			? leftBrace.Span.SpanTo(statements.Last().Span) 
			: leftBrace.Span;

		Statements = statements;
	}
}