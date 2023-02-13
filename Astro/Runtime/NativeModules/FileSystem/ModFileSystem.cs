using AstroLang.Analysis.Parsing;
using AstroLang.Runtime.DataTypes;

namespace AstroLang.Runtime.NativeModules.FileSystem;

public class ModFileSystem : INativeModule
{
    public Module Define()
    {
        var module = new Module(Name());

        module.AddProperty("CurrentDirectory", NativeFunction.From<FnCurrentDirectory>(), AccessModifier.Public);
        module.AddProperty("ReadFileToString", NativeFunction.From<FnReadFileToString>(), AccessModifier.Public);

        return module;
    }

    public string Name()
    {
        return "FileSystem";
    }
}