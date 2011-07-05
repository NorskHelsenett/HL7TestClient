using System.Collections.Generic;
using System.Linq;

namespace HL7TestClient.PersonRegistry
{
    public partial class AD : ANY
    {
        public AD()
        {
        }

        public AD(IEnumerable<ADXP> items)
            : this()
        {
            this.Items = items.ToArray();
        }
    }
}
