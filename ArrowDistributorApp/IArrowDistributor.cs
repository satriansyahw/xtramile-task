using System.Collections.Generic;

namespace ArrowDistributorApp
{
    public interface IArrowDistributor
    {
        List<Quiver> Distribute(Dictionary<Element, int> stock);
    }
}
