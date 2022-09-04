using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class VariableDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public Token Name { get; }
	public ExpressionSyntax? Initializer { get; }

	public VariableDeclarationSyntax(TextSpan span, Token name, ExpressionSyntax? initializer)
	{
		Span = span;
		Name = name;
		Initializer = initializer;
	}
}