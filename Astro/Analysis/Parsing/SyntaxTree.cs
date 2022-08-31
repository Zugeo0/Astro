using System.Text;
using AstroLang.Analysis.Parsing.SyntaxNodes;
using AstroLang.Analysis.Text;

namespace AstroLang.Analysis.Parsing;

public class SyntaxTree
{
	public SyntaxNode Root { get; }
	
	public SyntaxTree(SyntaxNode root)
	{
		Root = root;
	}

	public void Print(TextWriter writer, SourceText text) => writer.Write(PrintNode(Root, text, 0));
	private string PrintNode(SyntaxNode node, SourceText text, int indent)
	{
		switch (node)
		{
			// Other
			case ProgramSyntax e:
			{
				var label = PrintIndented(indent, "Program:");
				var builder = new StringBuilder();
				foreach (var statement in e.Statements)
					builder.Append($"{PrintNode(statement, text, indent + 1)}\n");
				return $"{label}\n{builder}";
			}
			
			// Statements
			case ExpressionStatementSyntax e:
			{
				var label = PrintIndented(indent, "Expression Statement:");
				var expr = PrintNode(e.Expression, text, indent + 1);
				return $"{label}\n{expr}";
			}
			
			// Expressions
			case LiteralExpressionSyntax e:
			{
				return PrintIndented(indent, $"Literal: {text.GetLexeme(e.Literal.Span)}");
			}
			case UnaryExpressionSyntax e:
			{
				var label = PrintIndented(indent, "Unary Expression:");
				var op = PrintIndented(indent + 1, $"{e.Operator}");
				var expr = PrintNode(e.Right, text, indent + 1);
				return $"{label}\n{op}\n{expr}";
			}
			case BinaryExpressionSyntax e:
			{
				var label = PrintIndented(indent, "Binary Expression:");
				var left = PrintNode(e.Left, text, indent + 1);
				var op = PrintIndented(indent + 1, $"{e.Operator}");
				var right = PrintNode(e.Right, text, indent + 1);
				return $"{label}\n{left}\n{op}\n{right}";
			}
		}

		return PrintIndented(indent, $"UNDEFINED NODE '{node.GetType()}'");
	}

	private string PrintIndented(int indent, string text) => $"{PrintIndent(indent)}{text}";
	private string PrintIndent(int indent) => new string(' ', indent * 2);
}