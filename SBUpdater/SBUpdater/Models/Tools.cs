using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBUpdater.Models
{
    public class Tools
    {
        public int Id;
        public string Model;
        public string Sku;
        public string Image;
        public int Manufacturer_id;
        public decimal Price;
        public decimal Weight;
        public decimal Length;
        public decimal Width;
        public decimal Height;
        public ProductDescription Description;
        public List<Attribute> Attributes;
        public string Url;
        public string CategoryName;
    }
}
