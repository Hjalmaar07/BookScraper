Console.WriteLine("Press Enter to exit...");
ConsoleKeyInfo keyInfo = Console.ReadKey(true);
if (keyInfo.Key == ConsoleKey.Enter)
{
    Environment.Exit(0);
}
