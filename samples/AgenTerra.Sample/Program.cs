using AgenTerra.Sample;

Console.WriteLine("=== AgenTerra Sample Demonstrations ===");
Console.WriteLine();
Console.WriteLine("Select a sample to run:");
Console.WriteLine("1. Reasoning Tools - Fox, Chicken, and Grain Puzzle");
Console.WriteLine("2. Workflow Session State - Shopping Cart");
Console.WriteLine("3. Knowledge Readers - Document Text Extraction");
Console.WriteLine();
Console.Write("Enter your choice (1, 2, or 3): ");

var choice = Console.ReadLine();

Console.WriteLine();
Console.WriteLine(new string('=', 60));
Console.WriteLine();

switch (choice)
{
    case "1":
        await ReasoningToolSample.RunAsync();
        break;
    case "2":
        await WorkflowSessionStateSample.RunAsync();
        break;
    case "3":
        await KnowledgeReadersSample.RunAsync();
        break;
    default:
        Console.WriteLine("Invalid choice. Running all samples...");
        Console.WriteLine();
        await ReasoningToolSample.RunAsync();
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();
        await WorkflowSessionStateSample.RunAsync();
        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();
        await KnowledgeReadersSample.RunAsync();
        break;
}
