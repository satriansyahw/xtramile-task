namespace ArrowDistributorApp
{
    public class ArrowDistributor : IArrowDistributor
    {
        private const int MaxArrowsPerQuiver = 10;
        private readonly IArrowValidator _validator;

        public ArrowDistributor(IArrowValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public List<Quiver> Distribute(Dictionary<Element, int> stock)
        {
            if (stock == null) throw new ArgumentNullException(nameof(stock));

            int totalArrows = stock.Values.Sum();
            if (totalArrows == 0) return new List<Quiver>();

            int numQuivers = (int)Math.Ceiling((double)totalArrows / MaxArrowsPerQuiver);

            _validator.Validate(stock, numQuivers);

            var quivers = new List<Quiver>();
            for (int i = 0; i < numQuivers; i++)
            {
                var q = new Quiver();
                foreach (var kvp in stock)
                {
                    q.Elements[kvp.Key] = kvp.Value / numQuivers;
                }
                quivers.Add(q);
            }

            // Distribute remainders
            foreach (var kvp in stock)
            {
                int r = kvp.Value % numQuivers;
                if (r > 0)
                {
                    // Select 'r' quivers with the lowest total to evenly distribute the capacity
                    var targetQuivers = quivers.OrderBy(q => q.Total).Take(r);
                    foreach (var q in targetQuivers)
                    {
                        q.Elements[kvp.Key]++;
                    }
                }
            }

            return quivers;
        }
    }
}
