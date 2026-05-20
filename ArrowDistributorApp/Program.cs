using System;
using System.Collections.Generic;

namespace ArrowDistributorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IArrowValidator validator = new ArrowValidator();
            IArrowDistributor distributor = new ArrowDistributor(validator);

            Console.WriteLine("Welcome to Gin's Elemental Arrow Distributor!");
            while (true)
            {
                Console.WriteLine("\n--- Enter Arrow Quantities ---");
                int fire = GetInput("Fire arrows: ");
                int water = GetInput("Water arrows: ");
                int wind = GetInput("Wind arrows: ");
                int earth = GetInput("Earth arrows: ");



                try
                {
                    var stock = CreateStock(fire, water, wind, earth);
                    var quivers = distributor.Distribute(stock);
                    PrintQuivers(quivers);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                Console.Write("\nDo you want to distribute more arrows? (y/n): ");
                string? response = Console.ReadLine();
                if (response?.Trim().ToLower() != "y")
                {
                    break;
                }
            }
        }

        static int GetInput(string prompt)
        {
            const int MaxArrows = 1000000;
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                
                if (input == null) Environment.Exit(0);

                if (int.TryParse(input, out int value) && value >= 0)
                {
                    if (value > MaxArrows)
                    {
                        Console.WriteLine($"Limit exceeded. Please enter a number up to {MaxArrows:N0}.");
                        continue;
                    }
                    return value;
                }
                Console.WriteLine("Invalid input. Please enter a valid positive integer.");
            }
        }

        static Dictionary<Element, int> CreateStock(int fire, int water, int wind, int earth)
        {
            return new Dictionary<Element, int>
            {
                { Element.Fire, fire },
                { Element.Water, water },
                { Element.Wind, wind },
                { Element.Earth, earth }
            };
        }

        static void PrintQuivers(List<Quiver> quivers)
        {
            Console.WriteLine("\nOutput:");
            if (quivers.Count == 0)
            {
                Console.WriteLine("No quivers needed.");
                return;
            }
            
            for (int i = 0; i < quivers.Count; i++)
            {
                Console.WriteLine($"Quiver {i + 1}: {quivers[i]}");
            }
        }
    }
}
