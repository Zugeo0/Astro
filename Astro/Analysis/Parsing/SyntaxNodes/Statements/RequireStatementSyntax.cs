using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class RequireStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public Token Keyword { get; }
	public Token ModName { get; }
	public Token? Alias { get; }

	public RequireStatementSyntax(TextSpan span, Token keyword, Token modName, Token? alias)
	{
		Span = span;
		Keyword = keyword;
		ModName = modName;
		Alias = alias;
	}
}