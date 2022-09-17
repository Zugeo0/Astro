using AstroLang.Analysis.Parsing;
using AstroLang.Runtime.DataTypes;
using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime;

public class ModuleAlreadyExistsException : Exception { }

public class Environment
{
	private readonly Stack<Scope> _scopes = new();
	private readonly Dictionary<string, Module> _modules = new();

	public Environment()
	{
		BeginScope();
	}

	private Environment(Stack<Scope> scopes)
	{
		_scopes = new Stack<Scope>(scopes);
	}

	public Environment Reference() => new(_scopes);

	public bool LocalDeclaredInCurrentScope(string name) => _scopes.Peek().HasLocal(name);
	
	public void DefineLocal(string name, Object value)
	{
		_scopes.Peek().DeclareLocal(name, value);
	}

	public bool AssignLocal(string name, Object value)
	{
		foreach (var scope in _scopes)
		{
			if (!scope.HasLocal(name))
				continue;
			
			scope.SetLocal(name, value);
			return true;
		}

		return false;
	}

	public DataTypes.Object? FindVariable(string name)
	{
		if (_modules.ContainsKey(name))
			return _modules[name];

		return _scopes
			.Select(scope => scope.GetLocal(name))
			.FirstOrDefault(local => local is not null);
	}

	public void BeginScope() => _scopes.Push(new Scope());
	public void EndScope() => _scopes.Pop();

	public void AddModule(Interpreter interpreter, Module module, Token name)
	{
		if (_modules.ContainsKey(name.Lexeme))
			interpreter.Error(name.Span, $"Module with name '{name.Lexeme}' already exists");

		_modules.Add(name.Lexeme, module);
	}
}