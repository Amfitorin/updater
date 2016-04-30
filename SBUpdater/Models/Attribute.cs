using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBUpdater.Models
{
    public class Attribute
    {
        public int Id;
        public int Attribute_Group_Id;
        public int Sort_Order = 0;
        public string Name;
        public string Value;
    }
}
