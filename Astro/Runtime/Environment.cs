﻿using AstroLang.Analysis.Parsing;
using AstroLang.Runtime.DataTypes;
using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime;

public class ModuleAlreadyExistsException : Exception { }

public class Environment
{
	private readonly Stack<Scope> _scopes = new();
	private readonly Dictionary<string, Module> _modules = new();

	private readonly List<Restricted<Module>> _userModules = new();
	private readonly Stack<Restricted<Module>> _moduleScope = new();

	public Module CurrentModule => _moduleScope.Peek().Value;

	public Environment()
	{
		BeginScope();
	}

	private Environment(Stack<Scope> scopes, Stack<Restricted<Module>> moduleScope, List<Restricted<Module>> userModules, Dictionary<string, Module> modules)
	{
		_scopes = new(scopes);
		_moduleScope = new(moduleScope);
		_userModules = new(userModules);
		_modules = new(modules);
	}

	public Environment ReferenceSnapshot() => new(_scopes, _moduleScope, _userModules, _modules);

	internal bool LocalDeclaredInCurrentScope(string name) => _scopes.Peek().HasLocal(name);
	
	internal void DefineLocal(string name, Object value)
	{
		_scopes.Peek().DeclareLocal(name, value);
	}

	internal void DefineProperty(string name, Object value, AccessModifier accessability)
	{
		_moduleScope.Peek().Value.AddProperty(name, value, accessability);
	}

	internal bool AssignLocal(string name, Object value)
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

	internal DataTypes.Object FindVariable(Interpreter interpreter, Token name)
	{
		var variable = _scopes
            .Select(scope => scope.GetLocal(name.Lexeme))
            .FirstOrDefault(local => local is not null);

		if (variable is not null)
			return variable;

        foreach (var mod in _moduleScope.Reverse())
		{
			var property = mod.Value.FindProperty(name.Lexeme);
			if (property is null)
				continue;

            return property.Value;	
		}

        foreach (var mod in _userModules)
		{
			if (mod.Value.Name != name.Lexeme)
				continue;

			return mod.Value;
		}

        if (_modules.ContainsKey(name.Lexeme))
			return _modules[name.Lexeme];
		
        interpreter.Error(name.Span, $"Cannot access private property '{name.Lexeme}' outside of own module");
		return new Null();
    }

    internal void BeginScope() => _scopes.Push(new Scope());
	internal void EndScope() => _scopes.Pop();

	internal void BeginModule(Interpreter interpreter, Token name, AccessModifier accessability)
	{
		if (_modules.ContainsKey(name.Lexeme))
			interpreter.Error(name.Span, $"External module '{name.Lexeme}' is already defined");
		
		var foundModule = _userModules.Find(mod => mod.Value.Name == name.Lexeme);
		_moduleScope.Push(foundModule ?? new(accessability, new Module(name.Lexeme)));
		BeginScope();
	}

	internal void EndModule()
	{
		EndScope();
		var mod = _moduleScope.Pop();
		if (!_userModules.Contains(mod))
			_userModules.Add(mod);
	}

	public void AddModule(Interpreter interpreter, Module module, Token name)
	{
		if (_modules.ContainsKey(name.Lexeme))
			interpreter.Error(name.Span, $"Module with name '{name.Lexeme}' already exists");

		_modules.Add(name.Lexeme, module);
	}

	public Function? FindEntry()
	{
		foreach (var module in _userModules)
		{
			var entry = module.Value.FindEntry();
			if (entry is not null)
				return entry;
		}

		return null;
	}

	public Module? FindModule(string name) => _userModules.Find(mod => mod.Value.Name == name)?.Value;
}