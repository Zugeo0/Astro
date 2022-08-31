using System.Collections.Immutable;

namespace AstroLang.Diagnostics;

public class DiagnosticList
{
	private List<Diagnostic> _diagnostics = new();

	public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();
	
	public void Add(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);
	public bool AnyErrors() => _diagnostics.Count > 0;
}