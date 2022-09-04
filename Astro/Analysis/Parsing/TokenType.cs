namespace AstroLang.Analysis.Parsing;

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
	Plus,
	Minus,
	Star,
	Slash,
	Percent,
	
	// Keywords
	True,
	False,
	Null,
	Var,
	If,
	Else,
	And,
	Or,
	
	// Special
	EndOfFile,
}