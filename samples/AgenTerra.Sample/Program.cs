using AgenTerra.Core.State;
using AgenTerra.Core.State.Models;

var stateStore = new InMemoryWorkflowStateStore();
var sessionId = "user_123";

// Simulate first workflow run - Add items
Console.WriteLine("=== Run 1: Adding items to cart ===");
await stateStore.SetStateAsync(sessionId, "shopping_list", new List<string> { "milk", "bread" });
await stateStore.SetStateAsync(sessionId, "total_items", 2);

var session = await stateStore.GetSessionAsync(sessionId);
Console.WriteLine($"Session created at: {session?.CreatedAt}");

// Simulate second workflow run - Add more items
Console.WriteLine("\n=== Run 2: Adding more items ===");
var existingList = await stateStore.GetStateAsync<List<string>>(sessionId, "shopping_list");
var updatedList = new List<string>(existingList ?? new List<string>()) { "eggs" };
await stateStore.SetStateAsync(sessionId, "shopping_list", updatedList);
await stateStore.SetStateAsync(sessionId, "total_items", 3);

// Simulate third workflow run - Check cart
Console.WriteLine("\n=== Run 3: Checking cart ===");
var shoppingList = await stateStore.GetStateAsync<List<string>>(sessionId, "shopping_list");
var totalItems = await stateStore.GetStateAsync<int>(sessionId, "total_items");

Console.WriteLine($"Shopping list: {string.Join(", ", shoppingList ?? new List<string>())}");
Console.WriteLine($"Total items: {totalItems}");

session = await stateStore.GetSessionAsync(sessionId);
Console.WriteLine($"Last updated: {session?.UpdatedAt}");

// Display all active sessions
Console.WriteLine("\n=== All Sessions ===");
var allSessions = await stateStore.GetAllSessionIdsAsync();
Console.WriteLine($"Active sessions: {string.Join(", ", allSessions)}");

// Cleanup
await stateStore.DeleteSessionAsync(sessionId);
Console.WriteLine("\nSession deleted successfully");
