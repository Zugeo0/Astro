using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class BreakStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public Token Keyword { get; }

	public BreakStatementSyntax(Token keyword)
	{
		Span = keyword.Span;
		Keyword = keyword;
	}
}