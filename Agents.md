# AgenTerra Agents

This document provides an overview of all available agents in the AgenTerra project.

## Project Guidelines
- The project should use the **Microsoft Agent Framework** to work with AI capabilities.
- Each feature implementation must include:
  - **Tests** to verify correctness and reliability.
  - **Samples** to demonstrate usage and provide examples.
- Create a **feature branch** for each new feature before starting implementation.

## Sample Code Organization
- Sample code should be organized into separate classes in the `samples/AgenTerra.Sample` directory.
- Each feature should have its own sample class (e.g., `ReasoningToolSample.cs`, `WorkflowSessionStateSample.cs`, `KnowledgeReadersSample.cs`).
- Sample classes should:
  - Be in the `AgenTerra.Sample` namespace
  - Include XML documentation describing the sample's purpose
  - Have a public static `RunAsync()` method that can be called from `Program.cs`
- The `Program.cs` file should provide a menu to select which sample to run, making it easy to demonstrate different features independently.


## Available Agents

### C# Expert Agent
**Location:** `agents/CSharpExpert.agent.md`

A specialized agent for C# development tasks, including:
 
 **Commit Message Guideline:**
 - When committing changes, always write a reasonable commit message.
 - The message should include a short summary of the changes made.
 
 This helps maintain clarity and traceability in the project history.
