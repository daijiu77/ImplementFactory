using System;
using System.Collections.Generic;
using System.DJ.ImplementFactory.Commons;
using System.Text;

namespace Test.Framework.Entities
{
    public class Plan : BaseModel
    {
        public string PName { get; set; }
        public string Detail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Plan plan { get; set; }
        public List<Plan> plans { get; set; }
    }
}
