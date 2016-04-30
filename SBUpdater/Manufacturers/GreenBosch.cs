using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using SBUpdater.Helpers;
using SBUpdater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SBUpdater.Manufacturers
{
    public class GreenBosch : Manufacturer
    {

        private readonly string _link = "http://www.bosch-professional.com/ru/ru/";

        private void ParseGreenBosch(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(readFromHtml(client.DownloadString(url.Url)));
            var documentNode = html.DocumentNode;
            var attributeValue = documentNode.SelectNodes("//img[@itemprop='image' and @class='stageProd']").First().GetAttributeValue("src", "");
            var fileName = (@"GreenBosch\" + url.ProductName + ".jpg").Replace("/", " ").Replace("*", " ");
            client.DownloadFile(attributeValue, fileName);
            var nodeArray = documentNode.SelectNodes("//table[@class='techDetails']").First().ChildNodes
                .Where(x => x.Name == "tbody").ToArray().SelectMany(x => x.ChildNodes.Where(l => l.Name == "tr").ToArray()).ToArray();
            var attrs = new List<Attr>();
            foreach (var item in nodeArray)
            {
                var nodeArray2 = (from x in item.ChildNodes
                                  where x.Name == "td"
                                  select x).ToArray();
                var str3 = nodeArray2[0].InnerText;
                var str4 = nodeArray2[1].InnerText;
                var attr = new Attr
                {
                    AttrName = str3,
                    Value = str4
                };
                attrs.Add(attr);
            }
            var descr = (documentNode.SelectNodes("//h6[@itemprop='description']") == null) ? "" : (documentNode.SelectNodes("//h6[@itemprop='description']").First().OuterHtml
                + html.GetElementbyId("ct_1").SelectSingleNode(".//ul").OuterHtml);//.Replace("<", "&lt;").Replace(">", "&gt;");
            var sku = documentNode.SelectNodes("//th[@class='hook']").First().InnerText;
            var komplect = html.GetElementbyId("ct_3").SelectSingleNode(".//table").SelectNodes(".//tr").ToList();
            var skus = komplect.First().SelectNodes(".//th").ToList();
            komplect.RemoveAt(0);
            skus.RemoveAt(0);
            var komplekts = new List<string>[skus.Count];
            for (int i = 0; i < skus.Count; i++)
                komplekts[i] = new List<string>();
            foreach (var item in komplect)
            {
                var cols = item.SelectNodes(".//td").ToList();
                var name = cols.First().InnerText;
                cols.RemoveAt(0);
                for (int i = 0; i < cols.Count; i++)
                    if (cols[i].SelectSingleNode(".//img") != null)
                        komplekts[i].Add(name);
            }
            ConfigureAttr(attrs);
            WriteOnFile();
            ConfirmCategory(url.CategoryName);
            WriteCategoryes();
            var attributes = new List<Models.Attribute>();
            foreach (var item in attrs)
            {
                var attribute = new Models.Attribute
                {
                    Attribute_Group_Id = AttributesConst[item.AttrName].groupId,
                    Id = AttributesConst[item.AttrName].attrId
                };
                var attrId = AttributesConst[item.AttrName].attrId;
                attribute.Name = Attributes.First(x => x.Id == attrId).Name;
                attribute.Sort_Order = 0;
                attribute.Value = item.Value;
                attributes.Add(attribute);
            }

            for (int i = 0; i < skus.Count; i++)
            {
                var tools = new Tools
                {
                    Attributes = attributes,
                    CategoryName = url.CategoryName
                };
                var description = new ProductDescription
                {
                    Description = descr + "<br/>" + "<b>Комплектация:</b>",
                    Meta_Description = metaDescription.Replace("{0}", "Bosch " + url.ProductName + " " + skus[i].InnerText),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Bosch " + url.ProductName + " " + skus[i].InnerText)
                };
                foreach (var item in komplekts[i])
                    description.Description += "<br/>" + item;
                tools.Description = description;
                tools.Height = 1M;
                tools.Image = "data/" + fileName.Replace(@"\", "/");
                tools.Length = 1M;
                tools.Manufacturer_id = 11;
                tools.Model = url.ProductName;
                tools.Price = 1M;
                tools.Sku = skus[i].InnerText;
                tools.Url = url.Url;
                tools.Weight = 1M;
                tools.Width = 1M;
                Products.Add(tools);
                InsertNewProduct();
            }
        }

        private void ParseGreenBoschLinks(URLs url)
        {
            HtmlNode node2;
            string attributeValue;
            string str2;
            HtmlDocument document = new HtmlDocument();
            WebClient client = new WebClient();
            Encoding encoding = Encoding.GetEncoding(0x4e3);
            Encoding encoding2 = Encoding.UTF8;
            document.LoadHtml(encoding2.GetString(encoding.GetBytes(client.DownloadString(url.Url))));
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//div[@class='floatBox' or @class='floatBox last']");
            HtmlNodeCollection nodes2 = document.DocumentNode.SelectNodes("//div[@class='conCent jsLink']");
            if (nodes != null)
            {
                foreach (HtmlNode node in (IEnumerable<HtmlNode>)nodes)
                {
                    node2 = (from x in
                                 (from x in node.ChildNodes
                                  where x.Name == "div"
                                  select x).First<HtmlNode>().ChildNodes
                             where x.Name == "a"
                             select x).First<HtmlNode>();
                    attributeValue = node2.GetAttributeValue("title", "");
                    str2 = node2.GetAttributeValue("href", "");
                    if (!(attributeValue == "Новинки"))
                    {
                        URLs item = new URLs
                        {
                            CategoryName = url.CategoryName + "/" + attributeValue,
                            Url = _link + str2
                        };
                        CategoryUrls.Add(item);
                    }
                }
            }
            if (nodes2 != null)
            {
                foreach (HtmlNode node in (IEnumerable<HtmlNode>)nodes2)
                {
                    node2 = (from x in node.ChildNodes
                             where x.Name == "a"
                             select x).First<HtmlNode>();
                    attributeValue = node.GetAttributeValue("title", "").Replace(" Professional", "");
                    str2 = node2.GetAttributeValue("href", "");
                    URLs ls2 = new URLs
                    {
                        CategoryName = url.CategoryName,
                        ProductName = attributeValue,
                        Url = _link + str2
                    };
                    ProductUrls.Add(ls2);
                }
            }
        }
        public ICommand UpdateGreenBoschLinks
        {
            get
            {
                return new Command(() =>
                {
                    URLs item = new URLs
                    {
                        CategoryName = "",
                        Url = _link + "%D0%B4%D0%BE%D0%BC%D0%B0%D1%88%D0%BD%D0%B8%D0%B5-%D0%BC%D0%B0%D1%81%D1%82%D0%B5%D1%80%D1%81%D0%BA%D0%B8%D0%B5-%D0%BF%D1%80%D0%BE%D0%BC%D1%8B%D1%88%D0%BB%D0%B5%D0%BD%D0%BD%D0%BE%D0%B5-%D0%BF%D1%80%D0%BE%D0%B8%D0%B7%D0%B2%D0%BE%D0%B4%D1%81%D1%82%D0%B2%D0%BE-101271-ocs-c/"
                    };
                    CategoryUrls.Add(item);
                    do
                    {
                        ParseGreenBoschLinks(CategoryUrls.First<URLs>());
                        CategoryUrls.RemoveAt(0);
                    }
                    while (CategoryUrls.Count > 0);
                    string queryString = "SELECT model\r\nFROM oc_product";
                    Connection.Open();
                    List<string> products = new List<string>();
                    MySqlDataReader reader = LoadFromDb(queryString);
                    while (reader.Read())
                    {
                        products.Add(reader.GetString(0));
                    }
                    Connection.Close();
                    ProductUrls = (from x in ProductUrls
                                   where !products.Contains(x.ProductName)
                                   select x).ToList<URLs>();
                    do
                    {
                        ParseGreenBosch(ProductUrls.First<URLs>());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

    }
}
