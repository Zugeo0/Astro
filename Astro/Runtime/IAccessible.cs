namespace AstroLang.Runtime;

public interface IAccessible
{
	public DataTypes.Object Access(Interpreter interpreter, string name);
}