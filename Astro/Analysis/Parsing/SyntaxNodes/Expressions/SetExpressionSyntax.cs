using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class SetExpressionSyntax : ExpressionSyntax
{
	public override TextSpan Span { get; }
	public ExpressionSyntax Target { get; }
	public Token Name { get; }
	public ExpressionSyntax Value { get; }

	public SetExpressionSyntax(ExpressionSyntax target, Token name, ExpressionSyntax value)
	{
		Target = target;
		Name = name;
		Value = value;
		Span = target.Span.ExtendTo(value.Span);
	}
}