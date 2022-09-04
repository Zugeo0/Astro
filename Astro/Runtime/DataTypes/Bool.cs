namespace AstroLang.Runtime.DataTypes;

public class Bool : Object
{
	public bool Value { get; set; }

	public Bool(bool value)
	{
		Value = value;
	}
	
	public override string ToString() => $"{Value}";
}