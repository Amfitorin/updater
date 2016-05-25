using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using SBUpdater.Models;
using System.Windows.Input;

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
            if (AttributesConst.Count == 0)
                ReadOfFile();
            string contents = "";
            foreach (KeyValuePair<string, attrCat> pair in AttributesConst)
            {
                string str2 = contents;
                contents = str2 + pair.Key + "|" + pair.Value.ToString() + "`";
            }
            File.WriteAllText("attributes.txt", contents);
        }
        public void WriteCategoryes()
        {
            if (CategoryConst.Count == 0)
                ReadCategoryes();
            string contents = "";
            foreach (KeyValuePair<string, int> pair in CategoryConst)
            {
                string str2 = contents;
                contents = str2 + pair.Key + "|" + pair.Value.ToString() + "#";
            }
            File.WriteAllText("categoryes.txt", contents);
        }
        protected List<Models.Attribute> AttributesReturn(List<Attr> attributes)
        {
            ConfigureAttr(attributes);
            WriteOnFile();
            var result = new List<Models.Attribute>();
            foreach (var item in attributes)
                result.Add(new Models.Attribute
                {
                    Attribute_Group_Id = AttributesConst[item.AttrName].groupId,
                    Id = AttributesConst[item.AttrName].attrId,
                    Value = item.Value,
                    Name = Attributes.First(x=>x.Id == AttributesConst[item.AttrName].attrId).Name,
                });
            return result;
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
