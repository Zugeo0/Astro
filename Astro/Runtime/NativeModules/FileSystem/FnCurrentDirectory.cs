using Object = AstroLang.Runtime.DataTypes.Object;

namespace AstroLang.Runtime.NativeModules.FileSystem;

public class FnCurrentDirectory : ICallable
{
    public int Arity() => 0;

    public Object Call(Interpreter interpreter, List<Object> arguments)
    {
        return new DataTypes.String(System.Environment.CurrentDirectory);
    }
}