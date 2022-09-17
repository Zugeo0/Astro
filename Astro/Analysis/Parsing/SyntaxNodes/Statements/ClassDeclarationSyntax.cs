using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class ClassDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public AccessModifier Access { get; }
	public Token Keyword { get; }
	public Token Name { get; }
	public List<PropertyDeclarationSyntax> Properties { get; }
	public PropertyDeclarationSyntax? Constructor { get; }

	public ClassDeclarationSyntax(TextSpan span, AccessModifier access, Token keyword, Token name, List<PropertyDeclarationSyntax> properties, PropertyDeclarationSyntax? constructor)
	{
		Span = span;
		Access = access;
		Keyword = keyword;
		Name = name;
		Properties = properties;
		Constructor = constructor;
	}
}