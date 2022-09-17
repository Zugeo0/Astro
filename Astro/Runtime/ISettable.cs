using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime;

public interface ISettable
{
	public DataTypes.Object Set(Interpreter interpreter, Token name, DataTypes.Object value);
}