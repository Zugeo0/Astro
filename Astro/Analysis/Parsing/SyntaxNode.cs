using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing;

public abstract class SyntaxNode
{
	public abstract TextSpan Span { get; }
}