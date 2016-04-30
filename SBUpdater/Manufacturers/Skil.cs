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
using System.Windows;

namespace SBUpdater.Manufacturers
{
    public 
        class Skil : Manufacturer
    {

        private void ParseSkil(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(readFromHtml(client.DownloadString(url.Url)));
            var documentNode = html.DocumentNode;
            var adress = documentNode.SelectNodes("//img[@class='stageProd']").First().GetAttributeValue("src", "");
            var fileName = (@"Skil\" + url.ProductName + ".jpg").Replace("/", " ").Replace("*", " ");
            client.DownloadFile(adress, fileName);
            var charackters = html.GetElementbyId("ct_2").SelectSingleNode(".//ul[@class='productBullets']").SelectNodes(".//li");
            var attrs = new List<Attr>();
            foreach (var item in charackters)
            {
                var name = "";
                var value = "";
                if (item.InnerText.Contains(":"))
                {
                    var array = item.InnerText.Split(':');
                    name = array[0];
                    value = array[1];
                }
                else
                {
                    name = item.InnerText;
                    value = "+";
                }

                var attr = new Attr
                {
                    AttrName = name,
                    Value = value
                };
                attrs.Add(attr);
            }

            var descr = html.GetElementbyId("ct_1").SelectSingleNode(".//ul[@class='productBullets']").OuterHtml + "<br/><b>Комплектация:</b><br/>" +
                html.GetElementbyId("ct_3").SelectSingleNode(".//ul[@class='productBullets']").OuterHtml;
            var sku = html.GetElementbyId("ct_3").SelectSingleNode(".//p").InnerText;
            sku = sku.Remove(0, sku.LastIndexOf(";") + 1);
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

            var tools = new Tools
            {
                Attributes = attributes,
                CategoryName = url.CategoryName
            };
            var description = new ProductDescription
            {
                Description = descr,
                Meta_Description = metaDescription.Replace("{0}", "Skil " + url.ProductName + " " + sku),
                Meta_Keyword = metaKeywords.Replace("{0}", "Skil " + url.ProductName + " " + sku)
            };
            tools.Description = description;
            tools.Height = 1M;
            tools.Image = "data/" + fileName.Replace(@"\", "/");
            tools.Length = 1M;
            tools.Manufacturer_id = 25;
            tools.Model = url.ProductName;
            tools.Price = 1M;
            tools.Sku = sku;
            tools.Url = url.Url;
            tools.Weight = 1M;
            tools.Width = 1M;
            Products.Add(tools);
            InsertNewProduct();
        }

        private void ParseSkilLinks(URLs url)
        {
            var document = new HtmlDocument();
            var client = new WebClient();
            document.LoadHtml(readFromHtml(client.DownloadString(url.Url)));
            var categories = document.DocumentNode.SelectNodes("//div[@class='floatBox' or @class='floatBox last']");
            var products = document.DocumentNode.SelectNodes("//div[@class='pDBoxLContent']");
            if (categories != null)
            {
                foreach (var category in categories)
                {
                    var cat = category.SelectSingleNode(".//a");
                    var catName = cat.GetAttributeValue("title", "");
                    var link = cat.GetAttributeValue("href", "");
                    if (!(catName == "Новые инструменты"))
                    {
                        URLs item = new URLs
                        {
                            CategoryName = url.CategoryName + "/" + catName,
                            Url = link
                        };
                        CategoryUrls.Add(item);
                    }
                }
            }
            if (products != null)
            {
                foreach (var product in products)
                {
                    var prod = product.SelectSingleNode(".//a");
                    var name = prod.SelectSingleNode(".//b").InnerText.Replace("\r\n", "").Replace("&nbsp;", "").Replace("\t", "").Replace("Skil", "").Replace("Masters", "");
                    var link = prod.GetAttributeValue("href", "");
                    URLs ls2 = new URLs
                    {
                        CategoryName = url.CategoryName,
                        ProductName = name,
                        Url = link
                    };
                    ProductUrls.Add(ls2);
                }
            }
        }
        public ICommand UpdateSkilLinks
        {
            get
            {
                return new Command(() =>
                {
                    URLs item = new URLs
                    {
                        CategoryName = "",
                        Url = "http://www.skileurope.com/ru/ru/diyocs/%D0%B8%D0%BD%D1%81%D1%82%D1%80%D1%83%D0%BC%D0%B5%D0%BD%D1%82%D1%8B/2560/ocs-diy/"
                    };
                    CategoryUrls.Add(item);
                    CategoryUrls.Add(new URLs
                    {
                        CategoryName = "",
                        Url = "http://www.skilmasters.com/ru/ru/mastersocs/%D0%B8%D0%BD%D1%81%D1%82%D1%80%D1%83%D0%BC%D0%B5%D0%BD%D1%82%D1%8B/1226/ocs-masters/",
                    });
                    CategoryUrls.Add(new URLs
                    {
                        CategoryName = "",
                        Url = "http://www.skileurope.com/ru/ru/gardenocs/%D0%B8%D0%BD%D1%81%D1%82%D1%80%D1%83%D0%BC%D0%B5%D0%BD%D1%82%D1%8B/1279/ocs-garden/",
                    });
                    do
                    {
                        ParseSkilLinks(CategoryUrls.First<URLs>());
                        CategoryUrls.RemoveAt(0);
                    }
                    while (CategoryUrls.Count > 0);
                    string queryString = "SELECT model\r\nFROM oc_product where manufacturer_id=25";
                    Connection.Open();
                    List<string> products = new List<string>();
                    MySqlDataReader reader = LoadFromDb(queryString);
                    while (reader.Read())
                    {
                        products.Add(reader.GetString(0));
                    }
                    Connection.Close();
                    ProductUrls = ProductUrls.Where(x => !products.Any(z => z.Contains(x.ProductName))).ToList();
                    do
                    {
                        ParseSkil(ProductUrls.First<URLs>());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateSkilPrice
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
                        for (int j = 1; j < 213; j++)
                        {
                            var sku = (string)((worksheet.Cells[j, 1] as Range).Text).ToString();
                            if (sku == "")
                                continue;
                            var price = decimal.Parse((worksheet.Cells[j, 4] as Range).Text);
                            var item = new Tools
                            {
                                Price = price,
                                Sku = sku,
                                Length = (worksheet.Cells[j, 5] as Range).Text == "" ? 0.0M : decimal.Parse(((worksheet.Cells[j, 5] as Range).Text)),
                                Height = (worksheet.Cells[j, 7] as Range).Text == "" ? 0.0M : decimal.Parse(((worksheet.Cells[j, 7] as Range).Text)),
                                Width = (worksheet.Cells[j, 6] as Range).Text == "" ? 0.0M : decimal.Parse(((worksheet.Cells[j, 6] as Range).Text)),
                                Weight = (worksheet.Cells[j, 8] as Range).Text == "" ? 0.0M : decimal.Parse(((worksheet.Cells[j, 8] as Range).Text)),
                            };
                            list.Add(item);
                        }
                        application.Quit();
                        foreach (var tools2 in list)
                        {
                            var cmdText = "UPDATE `oc_product` SET\r\nprice = @price, width=@width,weight=@weight,height = @height, length = @length WHERE sku = @sku and manufacturer_id = 25";
                            var command = new MySqlCommand(cmdText, Connection);
                            command.Parameters.Add(new MySqlParameter("@sku", tools2.Sku));
                            command.Parameters.Add(new MySqlParameter("@price", tools2.Price));
                            command.Parameters.Add(new MySqlParameter("@width", tools2.Width));
                            command.Parameters.Add(new MySqlParameter("@weight", tools2.Weight));
                            command.Parameters.Add(new MySqlParameter("@height", tools2.Height));
                            command.Parameters.Add(new MySqlParameter("@length", tools2.Length));
                            Connection.Open();
                            command.ExecuteNonQuery();
                            Connection.Close();
                        }
                        MessageBox.Show("Обновление прайса завершено!");
                    }
                }, null);
            }
        }
    }
}
