using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class ModDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public AccessModifier AccessModifier { get; }
	public Token Keyword { get; }
	public Token Name { get; }
	public List<StatementSyntax> Declarations { get; }

	public ModDeclarationSyntax(TextSpan span, AccessModifier accessModifier, Token keyword, Token name, List<StatementSyntax> declarations)
	{
		Span = span;
		AccessModifier = accessModifier;
		Keyword = keyword;
		Name = name;
		Declarations = declarations;
	}
}