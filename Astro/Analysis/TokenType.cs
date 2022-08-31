namespace AstroLang.Analysis;

public enum TokenType
{
	// Variable character tokens
	Identifier,
	Number,
	String,

	// Set character tokens
	LeftParen,
	RightParen,
	LeftBracket,
	RightBracket,
	LeftBrace,
	RightBrace,
	Equals,
	DoubleEquals,
	BangEquals,
	Lesser,
	LesserEquals,
	Greater,
	GreaterEquals,
	Dot,
	Semicolon,
	Bang,
	
	// Keywords
	True,
	False,
}