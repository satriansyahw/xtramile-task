using System.Collections.Generic;

namespace ArrowDistributorApp
{
    public interface IArrowValidator
    {
        void Validate(Dictionary<Element, int> stock, int numQuivers);
    }
}
