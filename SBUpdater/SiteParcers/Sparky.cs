//using HtmlAgilityPack;
//using Microsoft.Office.Interop.Excel;
//using Microsoft.Win32;
//using MySql.Data.MySqlClient;
//using SBUpdater.Helpers;
//using SBUpdater.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace SBUpdater.SiteParcers
//{
//    public class Sparky : MainParcer
//    {

//        public List<Tools> Products = new List<Tools>();
//        public List<URLs> CategoryUrls = new List<URLs>();
//        public List<URLs> ProductUrls = new List<URLs>();

//        public ICommand UpdateLinks
//        {
//            get
//            {
//                return new Command(() =>
//                {
//                    var html = new HtmlDocument();
//                    var wClient = new WebClient();
//                    html.LoadHtml(wClient.DownloadString("http://www.hitachi-pt.ru/catalog/powertools/demolishing"));
//                    var menus = html.GetElementbyId("menu_products").ChildNodes.Where(x => x.Name == "p")
//                       .First().ChildNodes.Where(x => x.Name == "a");
//                    foreach (var item in menus)
//                    {
//                        CategoryUrls.Add(new URLs
//                        {
//                            CategoryName = readFromHtml(item.InnerText),
//                            Url = item.Attributes["href"].Value
//                        });
//                    }

//                    do
//                    {
//                        ParseLinks(CategoryUrls.First());
//                        CategoryUrls.RemoveAt(0);
//                    }
//                    while (CategoryUrls.Count > 0);
//                    var query = @"SELECT model
//FROM oc_product
//WHERE manufacturer_id = 13";
//                    Connection.Open();
//                    var products = new List<string>();
//                    var reader = LoadFromDb(query);
//                    while (reader.Read())
//                    {
//                        products.Add(reader.GetString(0));
//                    }
//                    Connection.Close();

//                    ProductUrls = ProductUrls.Where(x => !products.Contains(x.ProductName)).ToList();
//                    do
//                    {
//                        Parse(ProductUrls.First());
//                        ProductUrls.RemoveAt(0);
//                    }
//                    while (ProductUrls.Count > 0);
//                });
//            }
//        }

//        public ICommand UpdatePrice
//        {
//            get
//            {
//                return new Command(() =>
//                {
//                    var pricedProducts = new List<Tools>();
//                    var fileDlg = new OpenFileDialog();
//                    if (fileDlg.ShowDialog() ?? false)
//                    {
//                        var excel = new Microsoft.Office.Interop.Excel.Application();
//                        Workbook book = excel.Workbooks.Open(fileDlg.FileName, 0, false, 5, "", "", false,
//                           XlPlatform.xlWindows, "", true, false, 0, true, false, false);
//                        Worksheet list = (Worksheet)book.Sheets[1];
//                        for (int i = 12; i < 437; i++)
//                        {
//                            var priceString = (list.Cells[i, 5] as Range).Text.ToString();
//                            var price = (Convert.ToInt32((Int32.Parse(priceString.Remove(priceString.IndexOf(',')).Replace(" ", ""))) * 1.3) / 100 + 1) * 100;
//                            pricedProducts.Add(new Tools
//                            {
//                                Price = price,
//                                Sku = (list.Cells[i, 1] as Range).Text.ToString(),
//                                Model = (list.Cells[i, 2] as Range).Text.ToString()
//                            });
//                        }
//                        excel.Quit();

//                        foreach (var item in pricedProducts)
//                        {
//                            var query = @"UPDATE `oc_product` SET
//price = @price WHERE sku = @sku and manufacturer_id = 13";
//                            var command = new MySqlCommand(query, Connection);
//                            command.Parameters.Add(new MySqlParameter("@sku", item.Sku));
//                            command.Parameters.Add(new MySqlParameter("@price", item.Price));
//                            command.Parameters.Add(new MySqlParameter("@model", item.Model));
//                            Connection.Open();
//                            command.ExecuteNonQuery();
//                            Connection.Close();
//                        }
//                    }
//                });
//            }
//        }

//        private void ParseLinks(URLs url)
//        {
//            var html = new HtmlDocument();
//            var wClient = new WebClient();
//            var startEncoding = Encoding.GetEncoding(1251);
//            var endEncoding = Encoding.UTF8;

//            html.LoadHtml(wClient.DownloadString(url.Url));

//            var categories = html.DocumentNode.SelectNodes("//a[@class='sub']");
//            List<HtmlNode> products = new List<HtmlNode>();
//            if (categories == null)
//            {
//                products = html.GetElementbyId("main_catalogue").ChildNodes.Where(x => x.Name == "p").ToList();
//                products.RemoveAt(products.Count - 1);
//            }

//            if (categories != null)
//                foreach (var node in categories)
//                {
//                    var link = node.Attributes["href"].Value;
//                    var title = readFromHtml(node.ChildNodes[1].InnerText);
//                    if (title == "Новинки")
//                        continue;
//                    CategoryUrls.Add(new URLs
//                    {
//                        CategoryName = url.CategoryName + "/" + title,
//                        Url = link,
//                    });
//                }
//            if (products.Count != 0)
//                foreach (var node in products)
//                {
//                    var link = node.ChildNodes[1].Attributes["href"].Value;
//                    var title = link.Replace(url.Url, "").Trim('/');
//                    ProductUrls.Add(new URLs
//                    {
//                        CategoryName = url.CategoryName,
//                        ProductName = title,
//                        Url = link,
//                    });
//                }
//        }

//        private void Parse(URLs url)
//        {
//            var html = new HtmlDocument();
//            var wClient = new WebClient();

//            html.LoadHtml(wClient.DownloadString(url.Url));
//            var doc = html.DocumentNode;

//            //var image = _hitachiLink + html.GetElementbyId("bi").GetAttributeValue("src", "");
//            var fileName = ("Hitachi\\" + url.ProductName + ".jpg");
//            //wClient.DownloadFile(image, fileName);

//            var rows = html.GetElementbyId("techparam").ChildNodes.Where(x => x.Name == "tr").ToList();
//            rows.RemoveAt(0);
//            var attributes = new List<Attr>();
//            foreach (var row in rows)
//            {
//                var columns = row.ChildNodes.Where(x => x.Name == "td").ToArray();
//                var attrName = readFromHtml(columns[0].InnerText);
//                var value = readFromHtml(columns[1].InnerText);

//                attributes.Add(new Attr
//                {
//                    AttrName = attrName,
//                    Value = value
//                });
//            }
//            var descr = readFromHtml(((html.GetElementbyId("catal").OuterHtml ?? "") +
//                    (html.GetElementbyId("mc_bottom_left").InnerHtml ?? ""))
//                    .Replace("<", "&lt;").Replace(">", "&gt;"));
//            var sku = "";
//            ConfigureAttr(attributes);
//            WriteOnFile();
//            ConfirmCategory(url.CategoryName);
//            WriteCategoryes();
//            var productAttributes = new List<Models.Attribute>();
//            foreach (var item in attributes)
//            {
//                productAttributes.Add(new Models.Attribute
//                {
//                    Attribute_Group_Id = AttributesConst[item.AttrName].groupId,
//                    Id = AttributesConst[item.AttrName].attrId,
//                    Name = Attributes.First(x => x.Id == AttributesConst[item.AttrName].attrId).Name,
//                    Sort_Order = 0,
//                    Value = item.Value,
//                });
//            }
//            Products.Add(new Tools
//            {
//                Attributes = productAttributes,
//                CategoryName = url.CategoryName,
//                Description = new ProductDescription
//                {
//                    Description = descr,
//                    Meta_Description = metaDescription.Replace("{0}", "Hitachi " + url.ProductName),
//                    Meta_Keyword = metaKeywords.Replace("{0}", "Hitachi " + url.ProductName),
//                },
//                Height = 1,
//                Image = "data/" + fileName.Replace("\\", "/"),
//                Length = 1,
//                Manufacturer_id = 13,
//                Model = url.ProductName,
//                Price = 1,
//                Sku = sku,
//                Url = url.Url,
//                Weight = 1,
//                Width = 1
//            });
//            InsertNewProduct();
//        }
//    }
//}
