using AstroLang.Analysis.Parsing;
using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime;

public interface IAccessible
{
	public DataTypes.Object Access(DataTypes.Object? accessor, Interpreter interpreter, Token name);
}