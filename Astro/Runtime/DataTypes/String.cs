namespace AstroLang.Runtime.DataTypes;

public class String : Object
{
	public string Value { get; set; }

	public String(string value)
	{
		Value = value;
	}
	
	public override bool Equals(object? obj) => obj is DataTypes.String v && Value == v.Value;

	public override string ToString() => Value;
	public override string TypeString() => "string";
}