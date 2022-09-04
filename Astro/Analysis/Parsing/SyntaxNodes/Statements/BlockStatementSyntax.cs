using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class BlockStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	
	public List<StatementSyntax> Statements { get; }

	public BlockStatementSyntax(TextSpan span, List<StatementSyntax> statements)
	{
		Span = span;
		Statements = statements;
	}
}