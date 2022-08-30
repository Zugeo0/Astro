using AstroLang;

// Start the REPL if no argument was passed in
if (args.Length == 0)
{
	LaunchREPL();
	return;
}

// Currently no arguments are supported, but in the future this would
// be where you would specify file locations, compiler config, etc.
PrintColored(ConsoleColor.Red, "Arguments not supported yet\n");

void LaunchREPL()
{
	// Print starting message
	Console.WriteLine("Starting Astro REPL.");
	Console.WriteLine("Type 'exit' to end");

	// Create new instance to store variables, etc...
	var astro = new Astro();
	
	while (true)
	{
		// Prompt user
		PrintColored(ConsoleColor.Green, "> ");
		var input = Console.ReadLine() ?? "";

		// Process custom commands
		switch (input.Trim())
		{
			// 'exit' simply returns out of the function
			case "exit": return;
		}

		// Run the user's input
		astro.Run(input);
	}
}

void PrintColored(ConsoleColor color, string message)
{
	// Print the message in the color specified then reset it afterward
	Console.ForegroundColor = color;
	Console.Write(message);
	Console.ResetColor();
}
