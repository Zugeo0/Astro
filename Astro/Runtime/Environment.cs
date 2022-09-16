using System.Collections.Immutable;
using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime;

public class ModuleAlreadyExistsException : Exception { }

public class Environment
{
	private readonly List<Module> _exposedModules = new();
	private readonly Stack<Scope> _scopes = new();

	public ImmutableList<Module> ExposedModules => _exposedModules.ToImmutableList();

	public Environment()
	{
		BeginScope();
	}

	private Environment(List<Module> exposedModules, Stack<Scope> scopes)
	{
		_exposedModules = exposedModules;
		_scopes = new Stack<Scope>(scopes);
	}

	public Environment Reference() => new(_exposedModules, _scopes);

	public bool LocalDeclaredInCurrentScope(string name) => _scopes.Peek().HasLocal(name);
	
	public void DeclareLocal(string name, Object value)
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
		foreach (var scope in _scopes)
		{
			var local = scope.GetLocal(name);
			if (local is not null)
				return local;
		}

		// TODO: Add dotted accessor
		foreach (var module in _exposedModules)
		{
			var global = module.FindGlobal(name);
			if (global is not null)
				return global;
		}

		return null;
	}

	public void BeginScope() => _scopes.Push(new Scope());
	public void EndScope() => _scopes.Pop();

	public void ExposeModule(Module module)
	{
		if (FindModule(module.Name) is not null)
			throw new ModuleAlreadyExistsException();
		
		_exposedModules.Add(module);
	}

	public Module? FindModule(string name) => _exposedModules.FirstOrDefault(module => module.Name == name);
}