using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;

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
        protected void LoadImage(string address,string fileName)
        {
            var client = new WebClient();
            var directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            client.DownloadFile(address, fileName);
        }
    }
}
