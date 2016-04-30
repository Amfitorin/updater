using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;

namespace SBUpdater.Manufacturers
{
    public class Manufacturer : ModelViev.MainWindowModelBase
    {

        

        public string readFromHtml(string attr)
        {
            return endEncoding.GetString(startEncoding.GetBytes(attr));
        }

        

        public void WriteOnFile()
        {
            string contents = "";
            foreach (KeyValuePair<string, attrCat> pair in AttributesConst)
            {
                string str2 = contents;
                contents = str2 + pair.Key + "|" + pair.Value.ToString() + "`";
            }
            System.IO.File.WriteAllText("attributes.txt", contents);
        }

        public void WriteCategoryes()
        {
            string contents = "";
            foreach (KeyValuePair<string, int> pair in CategoryConst)
            {
                string str2 = contents;
                contents = str2 + pair.Key + "|" + pair.Value.ToString() + "#";
            }
            System.IO.File.WriteAllText("categoryes.txt", contents);
        }


       

       
    }
}
