using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class BlockStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	
	public List<StatementSyntax> Statements { get; }

	public BlockStatementSyntax(Token leftBrace, List<StatementSyntax> statements, Token rightBrace)
	{
		Span = leftBrace.Span.SpanTo(rightBrace.Span);
		Statements = statements;
	}
}