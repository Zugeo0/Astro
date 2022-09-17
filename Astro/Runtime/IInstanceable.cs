using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime;

public interface IInstanceable
{
	public int Arity();
	public DataTypes.Object CreateInstance(Interpreter interpreter, List<DataTypes.Object> arguments);
}