using System.Globalization;

namespace AstroLang.Runtime.DataTypes;

public class Number : Object
{
	public double Value { get; set; }

	public Number(double value)
	{
		Value = value;
	}

	public Number(string number)
	{
		Value = double.Parse(number, CultureInfo.InvariantCulture);
	}

	public static Number operator +(Number left, Number right) => new(left.Value + right.Value);
	public static Number operator -(Number left, Number right) => new(left.Value - right.Value);
	public static Number operator *(Number left, Number right) => new(left.Value * right.Value);
	public static Number operator /(Number left, Number right) => new(left.Value / right.Value);
	public static Number operator %(Number left, Number right) => new(left.Value % right.Value);
	
	public static Bool operator < (Number left, Number right) => new(left.Value < right.Value);
	public static Bool operator <=(Number left, Number right) => new(left.Value <= right.Value);
	public static Bool operator > (Number left, Number right) => new(left.Value > right.Value);
	public static Bool operator >=(Number left, Number right) => new(left.Value >= right.Value);
	
	public static Number operator -(Number right) => new(-right.Value);
	
	public override bool Equals(object? obj) => obj is Number v && Value == v.Value;

	public override string ToString() => $"{Value}";
}