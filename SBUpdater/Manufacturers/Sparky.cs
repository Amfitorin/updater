using System;
using System.Collections.Generic;
using System.Linq;
using SBUpdater.Models;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Windows.Input;
using SBUpdater.Helpers;
using MySql.Data.MySqlClient;
using Microsoft.Win32;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace SBUpdater.Manufacturers
{
    public class Sparky : Manufacturer
    {
        private readonly string _sparkyLink = "http://sparky.ru/catalog/";

        private void ParseSparky(URLs url)
        {
            HtmlDocument document = new HtmlDocument();
            WebClient client = new WebClient();
            document.LoadHtml(client.DownloadString(url.Url));
            HtmlNode documentNode = document.DocumentNode;
            string address = "http://sparky.ru" + document.DocumentNode.SelectNodes("//a[@class='fancybox mainpic']").First<HtmlNode>().Attributes["href"].Value;
            string fileName = @"Sparky\" + url.ProductName + ".jpg";
            client.DownloadFile(address, fileName);
            List<HtmlNode> list = (from x in
                                       (from x in
                                            (from x in document.DocumentNode.SelectNodes("//div[@class='techdata']").First<HtmlNode>().ChildNodes
                                             where x.Name == "table"
                                             select x).First<HtmlNode>().ChildNodes
                                        where x.Name == "tbody"
                                        select x).First<HtmlNode>().ChildNodes
                                   where x.Name == "tr"
                                   select x).ToList<HtmlNode>();
            list.RemoveAt(0);
            List<Attr> attrs = new List<Attr>();
            foreach (HtmlNode node2 in list)
            {
                HtmlNode[] nodeArray = (from x in node2.ChildNodes
                                        where x.Name == "td"
                                        select x).ToArray<HtmlNode>();
                string str3 = nodeArray[0].InnerText;
                string str4 = nodeArray[2].InnerText;
                Attr attr = new Attr
                {
                    AttrName = str3,
                    Value = str4
                };
                attrs.Add(attr);
            }
            HtmlNodeCollection source = document.DocumentNode.SelectNodes("//ul[@class='comeswith']");
            string innerHtml = "";
            if (source != null)
            {
                innerHtml = document.DocumentNode.SelectNodes("//div[@class='htmlarea']").First<HtmlNode>().InnerHtml + "\n" + source.First<HtmlNode>().OuterHtml;
            }
            else
            {
                innerHtml = document.DocumentNode.SelectNodes("//div[@class='htmlarea']").First<HtmlNode>().InnerHtml;
            }
            string innerText = (from x in
                                    (from x in document.DocumentNode.SelectNodes("//h1[@class='modeltitle']").First<HtmlNode>().ChildNodes
                                     where x.Name == "span"
                                     select x).First<HtmlNode>().ChildNodes
                                where x.Name == "strong"
                                select x).First<HtmlNode>().InnerText;
            if (innerText == "")
                return;
            ConfigureAttr(attrs);
            WriteOnFile();
            ConfirmCategory(url.CategoryName);
            WriteCategoryes();
            List<SBUpdater.Models.Attribute> list3 = new List<SBUpdater.Models.Attribute>();
            using (List<Attr>.Enumerator enumerator2 = attrs.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Func<SBUpdater.Models.Attribute, bool> predicate = null;
                    Attr item = enumerator2.Current;
                    SBUpdater.Models.Attribute attribute = new SBUpdater.Models.Attribute
                    {
                        Attribute_Group_Id = AttributesConst[item.AttrName].groupId,
                        Id = AttributesConst[item.AttrName].attrId
                    };
                    if (predicate == null)
                    {
                        predicate = x => x.Id == AttributesConst[item.AttrName].attrId;
                    }
                    attribute.Name = Attributes.First<SBUpdater.Models.Attribute>(predicate).Name;
                    attribute.Sort_Order = 0;
                    attribute.Value = item.Value;
                    list3.Add(attribute);
                }
            }
            Tools tools = new Tools
            {
                Attributes = list3,
                CategoryName = url.CategoryName
            };
            ProductDescription description = new ProductDescription
            {
                Description = innerHtml,
                Meta_Description = metaDescription.Replace("{0}", "Sparky " + url.ProductName),
                Meta_Keyword = metaKeywords.Replace("{0}", "Sparky " + url.ProductName)
            };
            tools.Description = description;
            tools.Height = 1M;
            tools.Image = "data/" + fileName.Replace(@"\", "/");
            tools.Length = 1M;
            tools.Manufacturer_id = 15;
            tools.Model = url.ProductName;
            tools.Price = 1M;
            tools.Sku = innerText;
            tools.Url = url.Url;
            tools.Weight = 1M;
            tools.Width = 1M;
            Products.Add(tools);
            InsertNewProduct();
        }

        private void ParseSparkyLinks(URLs url)
        {
            HtmlDocument document = new HtmlDocument();
            WebClient client = new WebClient();
            document.LoadHtml(client.DownloadString(url.Url));
            HtmlNode elementbyId = document.GetElementbyId("paneswrap");
            List<HtmlNode> list = new List<HtmlNode>();
            if (elementbyId != null)
            {
                list = (from x in elementbyId.ChildNodes[1].ChildNodes
                        where x.Name == "a"
                        select x).ToList<HtmlNode>();
            }
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//a[@class='product']");
            foreach (HtmlNode node2 in list)
            {
                URLs item = new URLs
                {
                    CategoryName = url.CategoryName + "/" + node2.Attributes["title"].Value,
                    Url = _sparkyLink + node2.Attributes["href"].Value
                };
                CategoryUrls.Add(item);
            }
            if (nodes != null)
            {
                foreach (HtmlNode node2 in (IEnumerable<HtmlNode>)nodes)
                {
                    URLs ls2 = new URLs
                    {
                        CategoryName = url.CategoryName,
                        ProductName = (from x in
                                           (from x in node2.ChildNodes
                                            where x.Name == "span"
                                            select x).Last<HtmlNode>().ChildNodes
                                       where x.Name == "strong"
                                       select x).First<HtmlNode>().InnerText,
                        Url = "http://sparky.ru" + node2.Attributes["href"].Value
                    };
                    ProductUrls.Add(ls2);
                }
            }
        }
        public ICommand UpdateSparkyLinks
        {
            get
            {
                return new Command(() =>
                {
                    URLs item = new URLs
                    {
                        CategoryName = "",
                        Url = _sparkyLink + "?id_razd=1&idm=1&pdm=1&id_razd=1"
                    };
                    CategoryUrls.Add(item);
                    do
                    {
                        ParseSparkyLinks(CategoryUrls.First<URLs>());
                        CategoryUrls.RemoveAt(0);
                    }
                    while (CategoryUrls.Count > 0);
                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 15";
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
                        ParseSparky(ProductUrls.First<URLs>());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateSparkyPrice
        {
            get
            {
                return new Command(() =>
                {
                    List<Tools> list = new List<Tools>();
                    OpenFileDialog dialog = new OpenFileDialog();

                    if (dialog.ShowDialog() ?? false)
                    {
                        Microsoft.Office.Interop.Excel.Application application = (Microsoft.Office.Interop.Excel.Application)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("00024500-0000-0000-C000-000000000046")));
                        Workbook workbook = application.Workbooks.Open(dialog.FileName, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
                        Worksheet worksheet = (Worksheet)workbook.Sheets[1];
                        for (int j = 20; j < 204; j++)
                        {

                            if ((worksheet.Cells[j, 2] as Range).Text != "")
                            {
                                Tools item = new Tools
                                {
                                    Price = (decimal)decimal.Parse(((dynamic)(worksheet.Cells[j, 4] as Range).Text).ToString()),
                                    Sku = (string)((dynamic)(worksheet.Cells[j, 2] as Range).Text).ToString(),
                                    Model = (string)((dynamic)(worksheet.Cells[j, 1] as Range).Text).ToString()
                                };
                                list.Add(item);
                            }
                        }
                        application.Quit();
                        foreach (Tools tools2 in list)
                        {
                            string cmdText = "UPDATE `oc_product` SET\r\nprice = @price WHERE sku = @sku and manufacturer_id = 15";
                            MySqlCommand command = new MySqlCommand(cmdText, Connection);
                            command.Parameters.Add(new MySqlParameter("@sku", tools2.Sku));
                            command.Parameters.Add(new MySqlParameter("@price", tools2.Price));
                            command.Parameters.Add(new MySqlParameter("@model", tools2.Model));
                            Connection.Open();
                            command.ExecuteNonQuery();
                            Connection.Close();
                        }
                    }
                }, null);
            }
        }
    }
}
