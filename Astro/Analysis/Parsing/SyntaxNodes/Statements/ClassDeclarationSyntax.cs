using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class ClassDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public AccessModifier Accessability { get; }
	public Token Keyword { get; }
	public Token Name { get; }
	public List<PropertyDeclarationSyntax> Properties { get; }
	public PropertyDeclarationSyntax? Constructor { get; }

	public ClassDeclarationSyntax(TextSpan span, AccessModifier accessability, Token keyword, Token name, List<PropertyDeclarationSyntax> properties, PropertyDeclarationSyntax? constructor)
	{
		Span = span;
		Accessability = accessability;
		Keyword = keyword;
		Name = name;
		Properties = properties;
		Constructor = constructor;
	}
}