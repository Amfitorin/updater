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
    public class Hitachi : Manufacturer

    {
        private readonly string _hitachiLink = "http://www.hitachi-pt.ru";
        private void ParseHitachi(URLs url)
        {
            HtmlDocument document = new HtmlDocument();
            WebClient client = new WebClient();
            document.LoadHtml(client.DownloadString(url.Url));
            HtmlNode documentNode = document.DocumentNode;
            string address = _hitachiLink + document.GetElementbyId("bi").GetAttributeValue("src", "");
            string fileName = @"Hitachi\" + url.ProductName + ".jpg";
            client.DownloadFile(address, fileName);
            List<HtmlNode> list = (from x in document.GetElementbyId("techparam").ChildNodes
                                   where x.Name == "tr"
                                   select x).ToList<HtmlNode>();
            list.RemoveAt(0);
            List<Attr> attrs = new List<Attr>();
            foreach (HtmlNode node2 in list)
            {
                HtmlNode[] nodeArray = (from x in node2.ChildNodes
                                        where x.Name == "td"
                                        select x).ToArray<HtmlNode>();
                string str3 = readFromHtml(nodeArray[0].InnerText);
                string str4 = readFromHtml(nodeArray[1].InnerText);
                Attr attr = new Attr
                {
                    AttrName = str3,
                    Value = str4
                };
                attrs.Add(attr);
            }
            string str5 = readFromHtml(((document.GetElementbyId("catal").OuterHtml ?? "") + (document.GetElementbyId("mc_bottom_left").InnerHtml ?? "")).Replace("<", "&lt;").Replace(">", "&gt;"));
            string str6 = "";
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
                Description = str5,
                Meta_Description = metaDescription.Replace("{0}", "Hitachi " + url.ProductName),
                Meta_Keyword = metaKeywords.Replace("{0}", "Hitachi " + url.ProductName)
            };
            tools.Description = description;
            tools.Height = 1M;
            tools.Image = "data/" + fileName.Replace(@"\", "/");
            tools.Length = 1M;
            tools.Manufacturer_id = 13;
            tools.Model = url.ProductName;
            tools.Price = 1M;
            tools.Sku = str6;
            tools.Url = url.Url;
            tools.Weight = 1M;
            tools.Width = 1M;
            Products.Add(tools);
            InsertNewProduct();
        }

        private void ParseHitachiLinks(URLs url)
        {
            string str;
            string str2;
            HtmlDocument document = new HtmlDocument();
            WebClient client = new WebClient();
            Encoding encoding = Encoding.GetEncoding(0x4e3);
            Encoding encoding2 = Encoding.UTF8;
            document.LoadHtml(client.DownloadString(url.Url));
            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//a[@class='sub']");
            List<HtmlNode> list = new List<HtmlNode>();
            if (nodes == null)
            {
                list = (from x in document.GetElementbyId("main_catalogue").ChildNodes
                        where x.Name == "p"
                        select x).ToList<HtmlNode>();
                list.RemoveAt(list.Count - 1);
            }
            if (nodes != null)
            {
                foreach (HtmlNode node in (IEnumerable<HtmlNode>)nodes)
                {
                    str = node.Attributes["href"].Value;
                    str2 = readFromHtml(node.ChildNodes[1].InnerText);
                    if (!(str2 == "Новинки"))
                    {
                        URLs item = new URLs
                        {
                            CategoryName = url.CategoryName + "/" + str2,
                            Url = str
                        };
                        CategoryUrls.Add(item);
                    }
                }
            }
            if (list.Count != 0)
            {
                foreach (HtmlNode node in list)
                {
                    str = node.ChildNodes[1].Attributes["href"].Value;
                    str2 = str.Replace(url.Url, "").Trim(new char[] { '/' });
                    URLs ls2 = new URLs
                    {
                        CategoryName = url.CategoryName,
                        ProductName = str2,
                        Url = str
                    };
                    ProductUrls.Add(ls2);
                }
            }
        }

        public ICommand UpdateHitachiLinks
        {
            get
            {
                return new Command(() =>
                {
                    HtmlDocument document = new HtmlDocument();
                    WebClient client = new WebClient();
                    document.LoadHtml(client.DownloadString("http://www.hitachi-pt.ru/catalog/powertools/demolishing"));
                    IEnumerable<HtmlNode> enumerable = from x in
                                                           (from x in document.GetElementbyId("menu_products").ChildNodes
                                                            where x.Name == "p"
                                                            select x).First<HtmlNode>().ChildNodes
                                                       where x.Name == "a"
                                                       select x;
                    foreach (HtmlNode node in enumerable)
                    {
                        URLs item = new URLs
                        {
                            CategoryName = readFromHtml(node.InnerText),
                            Url = node.Attributes["href"].Value
                        };
                        CategoryUrls.Add(item);
                    }
                    do
                    {
                        ParseHitachiLinks(CategoryUrls.First<URLs>());
                        CategoryUrls.RemoveAt(0);
                    }
                    while (CategoryUrls.Count > 0);
                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 13";
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
                        ParseHitachi(ProductUrls.First<URLs>());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateHitachiPrice
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
                        for (int j = 12; j < 452; j++)
                        {
                            var oldprice = decimal.Parse((worksheet.Cells[j, 5] as Range).Text);
                            var price = decimal.Round(oldprice * 1.21M, 0);
                            var item = new Tools
                            {
                                Price = (decimal)price,
                                Sku = (string)((dynamic)(worksheet.Cells[j, 1] as Range).Text).ToString(),
                                Model = (string)((dynamic)(worksheet.Cells[j, 2] as Range).Text).ToString()
                            };
                            list.Add(item);
                        }
                        application.Quit();
                        foreach (var tools2 in list)
                        {
                            var cmdText = "UPDATE `oc_product` SET\r\nprice = @price WHERE sku = @sku and manufacturer_id = 13";
                            var command = new MySqlCommand(cmdText, Connection);
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
