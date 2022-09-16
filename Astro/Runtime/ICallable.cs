namespace AstroLang.Runtime;

public interface ICallable
{
	public int Arity();
	public DataTypes.Object Call(Interpreter interpreter, List<DataTypes.Object> arguments);
}