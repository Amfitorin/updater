using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBUpdater.Models
{
    public class Category
    {
        public int Id;
        public int Parent_Id;
        public int Language_Id = 3;
        public string Name;
        public string Description;
        public string Meta_Description;
        public string Meta_Keywords;

    }
}
