namespace AstroLang.Runtime.DataTypes;

public class Object
{
	private bool IsTruthful()
	{
		return this switch
		{
			Null => false,
			Bool b => !b.Value,
			_ => true
		};
	}

	private Bool IsEqual(Object other)
	{
		if (this is Null && other is Null)
			return new Bool(true);

		if (this is Null)
			return new Bool(false);

		return new Bool(this.Equals(other));
	}

	public static Bool operator ==(Object left, Object right) => left.IsEqual(right);
	public static Bool operator !=(Object left, Object right) => !left.IsEqual(right);
	
	public static Bool operator !(Object value) => new(!value.IsTruthful());
}