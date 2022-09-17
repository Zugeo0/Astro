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
	Comma,
	Semicolon,
	Bang,
	Plus,
	Minus,
	Star,
	Slash,
	Percent,
	Public,
	Private,
	Method,
	Class,
	New,
	Constructor,
	Break,
	
	// Keywords
	True,
	False,
	Null,
	Var,
	If,
	Else,
	And,
	Or,
	While,
	For,
	Function,
	Return,
	Require,
	As,
	Mod,
	
	// Special
	Inserted,
	EndOfFile,
}