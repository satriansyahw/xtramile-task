using System;
using System.Collections.Generic;

namespace ArrowDistributorApp
{
    public class ArrowValidator : IArrowValidator
    {
        public void Validate(Dictionary<Element, int> stock, int numQuivers)
        {
            foreach (var kvp in stock)
            {
                if (kvp.Value < numQuivers)
                {
                    throw new ArgumentException($"Stock of {kvp.Key} ({kvp.Value}) is insufficient to fill at least 1 arrow in each of the {numQuivers} quivers.");
                }
            }
        }
    }
}
