namespace AstroLang.Runtime.DataTypes;

public class NativeFunction : Object, ICallable
{
	public ICallable Value { get; }
	
	public NativeFunction(ICallable value)
	{
		Value = value;
	}

	public int Arity() => Value.Arity();
	public Object Call(Interpreter interpreter, List<Object> arguments) => Value.Call(interpreter, arguments);

	public static NativeFunction From<T>() where T : ICallable, new() => new NativeFunction(new T());

	public override string ToString() => "NativeFunction";
}