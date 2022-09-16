using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime.NativeModules.Time;

public class FnCurrent : ICallable
{
	public int Arity() => 0;

	public DataTypes.Object Call(Interpreter interpreter, List<DataTypes.Object> arguments)
	{
		return new Number(System.Environment.TickCount64);
	}
}