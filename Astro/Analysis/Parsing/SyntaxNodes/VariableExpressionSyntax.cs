using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class VariableExpressionSyntax : ExpressionSyntax
{
	public override TextSpan Span { get; }
	
	public Token Name { get; }

	public VariableExpressionSyntax(Token name)
	{
		Name = name;
		Span = name.Span;
	}
}