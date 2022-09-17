using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class FunctionDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public FunctionType Type { get; }
	public AccessModifier Accessability { get; }
	public Token Keyword { get; }
	public Token Name { get; }
	public List<Token> Arguments { get; }
	public StatementSyntax Body { get; }
	public List<DeclarationFlag> Flags { get; }

	public FunctionDeclarationSyntax(TextSpan span, FunctionType type, AccessModifier accessability, Token keyword,
		Token name, List<Token> arguments, StatementSyntax body, List<DeclarationFlag> flags)
	{
		Span = span;
		Type = type;
		Accessability = accessability;
		Keyword = keyword;
		Name = name;
		Arguments = arguments;
		Body = body;
		Flags = flags;
	}
}