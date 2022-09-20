using AstroLang.Runtime.DataTypes;
using Object = AstroLang.Runtime.DataTypes.Object;
using String = AstroLang.Runtime.DataTypes.String;

namespace AstroLang.Runtime.NativeModules.FileSystem;

public class FnReadFileToString : ICallable
{
    public int Arity() => 1;

    public Object Call(Interpreter interpreter, List<Object> arguments)
    {
        if (arguments[0] is not String str)
            return new Null();

        return new String(File.ReadAllText(str.Value));
    }
}