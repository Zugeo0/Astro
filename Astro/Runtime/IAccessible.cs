using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime;

public interface IAccessible
{
	public DataTypes.Object Access(Interpreter interpreter, Token name);
}