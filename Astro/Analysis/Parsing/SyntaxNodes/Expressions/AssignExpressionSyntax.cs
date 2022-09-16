using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class AssignExpressionSyntax : ExpressionSyntax
{
	public override TextSpan Span { get; }
	
	public Token Name { get; }
	public ExpressionSyntax Value { get; }

	public AssignExpressionSyntax(Token name, ExpressionSyntax value)
	{
		Name = name;
		Value = value;
		Span = name.Span.ExtendTo(value.Span);
	}
}