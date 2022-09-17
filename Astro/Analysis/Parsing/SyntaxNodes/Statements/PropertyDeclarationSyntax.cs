using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class PropertyDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public AccessModifier Access { get; }
	public StatementSyntax Declaration { get; }

	public PropertyDeclarationSyntax(TextSpan span, AccessModifier access, StatementSyntax declaration)
	{
		Span = span;
		Access = access;
		Declaration = declaration;
	}
}