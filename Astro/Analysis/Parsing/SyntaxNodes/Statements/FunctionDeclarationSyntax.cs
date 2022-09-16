using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class FunctionDeclarationSyntax : StatementSyntax
{
	public override TextSpan Span { get; }
	public Token Keyword { get; }
	public Token Name { get; }
	public List<Token> Arguments { get; }
	public StatementSyntax Body { get; }

	public FunctionDeclarationSyntax(TextSpan span, Token keyword, Token name, List<Token> arguments, StatementSyntax body)
	{
		Span = span;
		Keyword = keyword;
		Name = name;
		Arguments = arguments;
		Body = body;
	}
}