namespace AstroLang.Runtime.DataTypes;

public class String : Object
{
	public string Value { get; set; }

	public String(string value)
	{
		Value = value;
	}

	public override string ToString() => Value;
}