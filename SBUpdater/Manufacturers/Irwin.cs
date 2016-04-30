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
    public class Irwin : Manufacturer
    {
        private readonly string _irwinLink = "http://handtool.ru/producers/irwin.html?page=";
        private void ParseIrwin(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            var loaded = false;
            do
            {


                try
                {
                    html.LoadHtml(client.DownloadString(url.Url));
                    loaded = true;
                }
                catch (Exception ex) { }
            }
            while (!loaded);
            var documentNode = html.DocumentNode;
            var address = "http://handtool.ru" + readFromHtml(html.GetElementbyId("zoom01").Attributes["href"].Value);
            var fileName = (@"Irwin\" + url.ProductName + ".").Replace("\"", "").Replace("/", "");
            client.DownloadFile(address, fileName);
            var attrNames = html.GetElementbyId("CorpProdOptions").ChildNodes[1].ChildNodes[1]
                .ChildNodes.Where(x => x.Name == "td" && x.InnerText != "&nbsp;").ToArray()
                .Select(x => readFromHtml(x.InnerText)).ToArray();

            var attrs = html.GetElementbyId("CorpProdOptions").ChildNodes[3].ChildNodes.Where(x => x.Name == "tr").ToArray();
            var n = attrNames.Length;
            var l = attrs.Length;
            var attr = new string[l, n];
            var catName = readFromHtml(documentNode.SelectSingleNode("//ul[@class='B_crumbBox']").ChildNodes.Where(x => x.Name == "li" && x.Attributes["class"].Value == "B_crumb").Last().InnerText);
            for (int i = 0; i < l; i++)
            {
                var cols = attrs[i].ChildNodes.Where(x => (x.Name == "td" || x.Name == "th") && x.Attributes["class"] == null).ToArray();
                for (int j = 0; j < n; j++)
                    attr[i, j] = readFromHtml(cols[j].InnerText);
            }
            var descr = readFromHtml(documentNode.SelectSingleNode("//article").ChildNodes[0].OuterHtml);
            for (int i = 0; i < l; i++)
            {

                var attributes = new List<Attr>();
                for (int j = 1; j < n - 1; j++)
                {
                    attributes.Add(new Attr
                    {
                        AttrName = attrNames[j],
                        Value = attr[i, j]
                    });
                }
                ConfigureAttr(attributes);
                WriteOnFile();
                ConfirmCategory(catName);
                WriteCategoryes();
                var attribute = new List<Models.Attribute>();
                foreach (var item in attributes)
                    attribute.Add(new Models.Attribute
                    {
                        Attribute_Group_Id = AttributesConst[item.AttrName].groupId,
                        Id = AttributesConst[item.AttrName].attrId,
                        Sort_Order = 0,
                        Value = item.Value,
                        Name = Attributes.First(x => x.Id == AttributesConst[item.AttrName].attrId).Name
                    });
                Tools tools = new Tools
                {
                    Attributes = attribute,
                    CategoryName = catName
                };
                var name = attr[i, 0] + " " + attr[i, n - 1];
                var description = new ProductDescription
                {
                    Description = descr,
                    Meta_Description = metaDescription.Replace("{0}", "Irwin " + name),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Irwin " + name)
                };
                tools.Description = description;
                tools.Height = 1M;
                tools.Image = "data/" + fileName.Replace(@"\", "/");
                tools.Length = 1M;
                tools.Manufacturer_id = 17;
                tools.Model = name;
                tools.Price = 1M;
                tools.Sku = attr[i, n - 1];
                tools.Url = url.Url;
                tools.Weight = 1M;
                tools.Width = 1M;
                Products.Add(tools);
                InsertNewProduct();
            }
        }

        private void ParseIrwinLinks(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var products = html.GetElementbyId("products").ChildNodes.Where(x => x.Name == "li").ToArray();

            foreach (var product in products)
            {
                var link = product.ChildNodes.Where(x => x.Name == "article").First().ChildNodes.Where(x => x.Name == "div").First().ChildNodes.Where(x => x.Name == "a").First();
                ProductUrls.Add(new URLs
                {
                    CategoryName = "",
                    ProductName = readFromHtml(link.Attributes["title"].Value),
                    Url = "http://handtool.ru/" + link.Attributes["href"].Value,

                });
            }
        }

        public ICommand UpdateIrwinLinks
        {
            get
            {
                return new Command(() =>
                {
                    for (int i = 1; i < 14; i++)
                    {
                        ParseIrwinLinks(new URLs
                        {
                            CategoryName = "",
                            Url = _irwinLink + i,
                        });
                    }
                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 17";
                    Connection.Open();
                    List<string> products = new List<string>();
                    MySqlDataReader reader = LoadFromDb(queryString);
                    while (reader.Read())
                    {
                        products.Add(reader.GetString(0).Remove(reader.GetString(0).LastIndexOf(" ")));
                    }
                    Connection.Close();
                    ProductUrls = ProductUrls.Where(x => !products.Contains(x.ProductName)).ToList();
                    do
                    {
                        ParseIrwin(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateIrwinPrice
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
                        for (int j = 6; j < 771; j++)
                        {
                            var model = ((worksheet.Cells[j, 1] as Range).Text).ToString();
                            if (model == "")
                                continue;
                            var priceString = ((dynamic)(worksheet.Cells[j, 5] as Range).Text).ToString();
                            var price = Convert.ToInt32(int.Parse((priceString).Remove((priceString).IndexOf(',')).Replace(" ", "")));
                            var item = new Tools
                            {
                                Price = (decimal)price,
                                Model = model,
                            };
                            list.Add(item);
                        }
                        application.Quit();
                        foreach (Tools tools2 in list)
                        {
                            string cmdText = "UPDATE `oc_product` SET\r\nprice = @price WHERE sku = @sku and manufacturer_id = 17";
                            MySqlCommand command = new MySqlCommand(cmdText, Connection);
                            command.Parameters.Add(new MySqlParameter("@price", tools2.Price));
                            command.Parameters.Add(new MySqlParameter("@sku", tools2.Model));
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
