using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class ReturnStatementSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public Token ReturnKeyword { get; }
	public ExpressionSyntax Value { get; }

	public ReturnStatementSyntax(TextSpan span, Token returnKeyword, ExpressionSyntax value)
	{
		Span = span;
		ReturnKeyword = returnKeyword;
		Value = value;
	}
}