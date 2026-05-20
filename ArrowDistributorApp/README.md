# Gin's Elemental Arrow Distributor 🏹

This is a **C# Console Application** built to solve the algorithmic challenge of distributing elemental arrows (Fire, Water, Wind, and Earth) as evenly as possible across multiple quivers.

## 🌟 The Challenge

Gin, a skilled hunter, needs to distribute elemental arrows into his quivers.
The rules for the distribution are strict:
1. **Capacity Limit**: Each quiver can hold a maximum of **10 arrows**.
2. **Elemental Requirement**: Every single quiver **must contain at least one arrow of each element** (Fire, Water, Wind, and Earth).
3. **Efficiency**: The total arrows and elements must be distributed as *evenly as possible* across the minimum number of quivers required.

## 🚀 Features

- **Interactive CLI**: A user-friendly, loop-based interactive console that allows you to easily input quantities for each arrow type.
- **Robust Validation**: 
  - Protects against invalid, negative, or extremely large inputs (Limit: 1,000,000 per element).
  - Handles mathematical impossibility (e.g., when forced to use 3 quivers but only having 1 Wind arrow).
  - Safe against forced stream closures (Ctrl+C / Ctrl+Z).
- **Clean Architecture**: The application is strictly designed with modular components, ensuring it is highly maintainable and extensible.

## 🏗️ Architecture & Design

The codebase has been refactored to cleanly separate concerns:
- **`Element.cs`**: An Enum representing the elements. Adding a new element (like *Lightning*) simply requires adding it to the Enum, without breaking the core distribution algorithm.
- **`Quiver.cs`**: Uses a `Dictionary<Element, int>` for maximum flexibility rather than hardcoding properties.
- **`IArrowValidator.cs` & `ArrowValidator.cs`**: Purely handles business constraints and error messaging.
- **`IArrowDistributor.cs` & `ArrowDistributor.cs`**: The core mathematical algorithm for division and remainder distribution. It relies on the injected validator interface for constraint checking.
- **`Program.cs`**: Acts as the entry point, handling the interactive UI and assembling the dependencies.

## 💻 How to Run

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Execution
1. Open your terminal or command prompt.
2. Navigate to the project directory (`ArrowDistributorApp`).
3. Run the following command:
   ```bash
   dotnet run
   ```
4. Follow the on-screen prompts to input your arrow quantities!

## 🧪 Example

**Input:**
- Fire: 10
- Water: 6
- Wind: 3
- Earth: 5

**Output:**
```text
Quiver 1: 4 fire, 2 water, 1 wind, 1 earth
Quiver 2: 3 fire, 2 water, 1 wind, 2 earth
Quiver 3: 3 fire, 2 water, 1 wind, 2 earth
```
*Note how perfectly even the total distribution is (8 arrows per quiver) while strictly maintaining the element requirements!*
