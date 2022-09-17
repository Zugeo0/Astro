using AstroLang.Runtime.DataTypes;
using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime;

public class ModuleAlreadyExistsException : Exception { }

public class Environment
{
	private readonly Stack<Scope> _scopes = new();
	private readonly List<Module> _modules = new();

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
		foreach (var module in _modules.Where(module => module.Name == name))
			return module;

		return _scopes
			.Select(scope => scope.GetLocal(name))
			.FirstOrDefault(local => local is not null);
	}

	public void BeginScope() => _scopes.Push(new Scope());
	public void EndScope() => _scopes.Pop();

	public void AddModule(Module module)
	{
		if (FindModule(module.Name) is not null)
			throw new ModuleAlreadyExistsException();
		
		_modules.Add(module);
	}

	public Module? FindModule(string name) => _modules.Find(m => m.Name == name);
}