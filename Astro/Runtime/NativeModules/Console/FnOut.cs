using AstroLang.Runtime.DataTypes;
using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime.NativeModules.Console;

public class FnOut : ICallable
{
	public int Arity() => 1;

	public Object Call(Interpreter interpreter, List<Object> arguments)
	{
		var arg = arguments[0];
		System.Console.WriteLine(arg);
		return new Null();
	}
}