using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing.SyntaxNodes;

public class ProgramSyntax : SyntaxNode
{
	public StatementSyntax[] Statements { get; }
	public override TextSpan Span { get; }

	public ProgramSyntax(StatementSyntax[] statements)
	{
		Statements = statements;

		if (statements.Length <= 0)
		{
			Span = new TextSpan(0, 0);
			return;
		}
		
		var start = statements[0].Span.Start;
		var end = statements[^1].Span.Start + statements[^1].Span.Length;
		Span = new TextSpan(start, end - start);
	}
}