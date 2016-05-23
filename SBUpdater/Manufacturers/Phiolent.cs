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
using System.IO;
using System.Windows;

namespace SBUpdater.Manufacturers
{
    public class Phiolent : Manufacturer
    {
        private readonly string _link = "http://shop.phiolent.com/";
        private void ParsePhiolent(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            try
            {
                html.LoadHtml(readFromHtml(client.DownloadString(url.Url)));
                var documentNode = html.DocumentNode;
                var address = url.CategoryName;
                var sku = documentNode.SelectSingleNode("//div[@class='description']").InnerText.Split('\n')[1].Trim().Replace("Модель:","").Trim();
                var fileName = (@"Phiolent\" + sku + ".jpg").Replace("\"", "").Replace("/", "").Replace("<", "").Replace(">", "").Replace(":", "");
                client.DownloadFile(address, fileName);
                var descr = "";
                if (documentNode.SelectSingleNode("//div[@class='opacity05-article']").SelectSingleNode(".//div[@class='text']") != null)
                    descr = documentNode.SelectSingleNode("//div[@class='opacity05-article']").SelectSingleNode(".//div[@class='text']").InnerText.Replace("\t", "").Trim(new char[] { '\n', ' ' });
                var catName = documentNode.SelectSingleNode("//div[@class='navigation-route']").SelectNodes(".//a").Last().InnerText;
                var name = documentNode.SelectSingleNode("//div[@class='opacity05-article']").SelectSingleNode(".//h1").InnerText;
                name = name.Remove(name.IndexOf("//"));
                ConfirmCategory(catName);
                WriteCategoryes();

                var attribute = new List<Models.Attribute>();
                Tools tools = new Tools
                {
                    Attributes = attribute,
                    CategoryName = catName
                };
                var description = new ProductDescription
                {
                    Description = descr,
                    Meta_Description = metaDescription.Replace("{0}", "Phiolent " + name + " " + url.ProductName),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Phiolent " + name + " " + url.ProductName)
                };
                tools.Description = description;
                tools.Height = 1M;
                tools.Image = "data/" + fileName.Replace(@"\", "/");
                tools.Length = 1M;
                tools.Manufacturer_id = 34;
                tools.Model = sku;
                tools.Name = name;
                tools.Price = 1M;
                tools.Sku = sku;
                tools.Url = url.Url;
                tools.Weight = 1M;
                tools.Width = 1M;
                Products.Add(tools);
                InsertNewProduct();
            }
            catch (Exception ex)
            {
                File.AppendAllText("error.txt", url.Url + " " + url.ProductName + " " + ex.Message + "\r\n");
            }

        }

        private void ParsePhiolentLinks(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(readFromHtml( client.DownloadString(url.Url)));
            var urls = new List<URLs>();
            urls.Add(url);
            if (html.DocumentNode.SelectSingleNode("//div[@class='pagination']") == null)
                return;
            if (html.DocumentNode.SelectSingleNode("//div[@class='pagination']").ChildNodes.Any(x=>x.Name == "a"))
            {
                var pages = html.DocumentNode.SelectSingleNode("//div[@class='pagination']").SelectNodes(".//a").ToList();
                pages.RemoveRange(pages.Count - 2, 2);
                foreach (var item in pages)
                    urls.Add(new URLs
                    {
                        CategoryName = url.CategoryName,
                        Url = item.GetAttributeValue("href","")
                    });

            }
            var products = html.DocumentNode.SelectSingleNode("//div[@class='product-list']").SelectNodes("./div").ToList();
            if (urls.Count > 1)
                for (int i = 1; i < urls.Count;i++ )
                {
                    html.LoadHtml(readFromHtml(client.DownloadString(urls[i].Url)));
                    products.AddRange(html.DocumentNode.SelectSingleNode("//div[@class='product-list']").SelectNodes("./div").ToList());
                }
                    foreach (var product in products)
                    {
                        var link = product.SelectSingleNode(".//div[@class='name']").SelectSingleNode(".//a");
                        ProductUrls.Add(new URLs
                        {
                            CategoryName = url.CategoryName,
                            ProductName = link.InnerText,
                            Url = link.Attributes["href"].Value,

                        });
                    }
        }

        public ICommand UpdatePhiolentLinks
        {
            get
            {
                return new Command(() =>
                {
                    var html = new HtmlDocument();
                    var client = new WebClient();
                    html.LoadHtml(readFromHtml( client.DownloadString(_link)));
                    var links = html.GetElementbyId("menu").SelectNodes(".//li");
                    foreach (var link in links)
                    {
                        if (link.ChildNodes.Count > 3)
                            continue;
                        var a = link.SelectSingleNode(".//a");
                        ParsePhiolentLinks(new URLs
                        {
                            CategoryName = a.InnerText.Contains('(') ? a.InnerText.Remove(a.InnerText.IndexOf('(') - 1):a.InnerText,
                            Url = a.GetAttributeValue("href","")
                        });
                    }

                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 77";
                    Connection.Open();
                    List<string> products = new List<string>();
                    MySqlDataReader reader = LoadFromDb(queryString);
                    while (reader.Read())
                    {
                        products.Add(reader.GetString(0));
                    }
                    Connection.Close();
                    ProductUrls = ProductUrls.Where(x => !products.Contains(x.ProductName)).ToList();
                    do
                    {
                        ParsePhiolent(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdatePhiolentPrice
        {
            get
            {
                return new Command(() =>
                {
                    List<Tools> list = new List<Tools>();
                    OpenFileDialog dialog = new OpenFileDialog();
                    string queryString = "SELECT sku\r\nFROM oc_product\r\nWHERE manufacturer_id > 25 and manufacturer_id<34";
                    Connection.Open();
                    List<string> products = new List<string>();
                    MySqlDataReader reader = LoadFromDb(queryString);
                    while (reader.Read())
                    {
                        products.Add(reader.GetString(0));
                    }
                    Connection.Close();
                    if (dialog.ShowDialog() ?? false)
                    {
                        Microsoft.Office.Interop.Excel.Application application = (Microsoft.Office.Interop.Excel.Application)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("00024500-0000-0000-C000-000000000046")));
                        Workbook workbook = application.Workbooks.Open(dialog.FileName, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
                        Worksheet worksheet = (Worksheet)workbook.Sheets[1];
                        for (int j = 9; j < 1916; j++)
                        {
                            var model = ((worksheet.Cells[j, 1] as Range).Text).ToString();
                            if (model == "")
                                continue;
                            var priceString = ((dynamic)(worksheet.Cells[j, 14] as Range).Text).ToString();
                            var price = Convert.ToInt32(int.Parse((priceString).Remove((priceString).IndexOf(',')).Replace(" ", "")) * 0.95);
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
                            if (products.Contains(tools2.Model))
                            {
                                string cmdText = "UPDATE `oc_product` SET\r\nprice = @price, status=TRUE WHERE sku = @model and manufacturer_id > 25 and manufacturer_id<34";
                                MySqlCommand command = new MySqlCommand(cmdText, Connection);
                                command.Parameters.Add(new MySqlParameter("@price", tools2.Price));
                                command.Parameters.Add(new MySqlParameter("@model", tools2.Model));
                                Connection.Open();
                                command.ExecuteNonQuery();
                                Connection.Close();
                            }
                            else
                                File.AppendAllText("products.txt", tools2.Model + "\r\n");
                        }
                        MessageBox.Show("Обновление прайса завершено!");
                    }
                }, null);
            }
        }
    }
}
