using System;
using System.Collections.Generic;
using System.Linq;

namespace ArrowDistributorApp
{
    public class Quiver
    {
        public Dictionary<Element, int> Elements { get; }

        public Quiver()
        {
            Elements = new Dictionary<Element, int>();
            foreach (Element el in Enum.GetValues(typeof(Element)))
            {
                Elements[el] = 0;
            }
        }

        public int Total => Elements.Values.Sum();

        public override string ToString()
        {
            var parts = Elements.Select(kvp => $"{kvp.Value} {kvp.Key.ToString().ToLower()}");
            return string.Join(", ", parts);
        }
    }
}
