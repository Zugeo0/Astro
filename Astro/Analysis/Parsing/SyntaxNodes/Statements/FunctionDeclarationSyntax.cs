using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class FunctionDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public FunctionType Type { get; }
	public Token Keyword { get; }
	public Token Name { get; }
	public List<Token> Arguments { get; }
	public StatementSyntax Body { get; }

	public FunctionDeclarationSyntax(TextSpan span, FunctionType type, Token keyword, Token name, List<Token> arguments, StatementSyntax body)
	{
		Span = span;
		Type = type;
		Keyword = keyword;
		Name = name;
		Arguments = arguments;
		Body = body;
	}
}