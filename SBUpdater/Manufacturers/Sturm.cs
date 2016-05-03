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
    public class Sturm : Manufacturer
    {

        private void ParseSturm(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            try
            {
                html.LoadHtml(client.DownloadString(url.Url));
                var documentNode = html.DocumentNode;
                var address = "";
                var fileName = "";
                if (documentNode.SelectSingleNode("//img[@class='main']") != null)
                {
                    address = "http://otvertka.ru" + documentNode.SelectSingleNode("//img[@class='main']").Attributes["src"].Value;
                    fileName = (@"Sturm\" + url.ProductName + ".jpeg").Replace("\"", "").Replace("/", "").Replace("<", "").Replace(">", "").Replace(":", "");
                    client.DownloadFile(address, fileName);
                }
                var rows = documentNode.SelectSingleNode("//div[@class='product_review product_instructions']").SelectNodes(".//tr");
                var attrs = new List<HtmlNode>();
                if (rows != null)
                {
                    attrs = rows.ToList();
                    attrs.RemoveAt(0);
                }

                var attributes = new List<Attr>();
                foreach (var item in attrs)
                {
                    var cols = item.ChildNodes.Where(x => x.Name == "td").ToArray();
                    attributes.Add(new Attr
                    {
                        AttrName = cols[0].InnerText.Trim(new char[] { '\t', '\n' }),
                        Value = cols[1].InnerText.Trim(new char[] { '\t', '\n' }),
                    });
                }
                var descr = "";
                if (documentNode.SelectSingleNode("//div[@class='product_review']") != null)
                    descr = documentNode.SelectSingleNode("//div[@class='product_review']").SelectSingleNode(".//div[@class='text']").InnerText.Replace("\t", "").Trim(new char[] { '\n', ' ' });
                var catName = url.CategoryName + "/" + documentNode.SelectNodes("//a[@class='none-decoration']").Last().InnerText;
                var name = documentNode.SelectSingleNode("//div[@class='maincontent']").SelectSingleNode(".//div[@class='head']").SelectSingleNode(".//h1").InnerText.Trim(new char[] { '\t', '\n' });
                name = name.Remove(name.IndexOf(" S"));
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
                var description = new ProductDescription
                {
                    Description = descr,
                    Meta_Description = metaDescription.Replace("{0}", "Sturm " + name + " " + url.ProductName),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Sturm " + name + " " + url.ProductName)
                };
                tools.Description = description;
                tools.Height = 1M;
                tools.Image = "data/" + fileName.Replace(@"\", "/");
                tools.Length = 1M;
                tools.Manufacturer_id = 26;
                tools.Model = url.ProductName;
                tools.Name = name;
                tools.Price = 1M;
                tools.Sku = url.ProductName;
                tools.Url = url.Url;
                tools.Weight = 1M;
                tools.Width = 1M;
                Products.Add(tools);
                InsertNewProduct();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.txt", url.Url + " " + url.ProductName + " " + ex.Message + "\r\n");
            }

        }

        private void ParseSturmLinks(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var products = html.DocumentNode.SelectNodes("//span[@class='zagolovok']");

            foreach (var product in products)
            {
                var link = product.SelectSingleNode(".//a");
                ProductUrls.Add(new URLs
                {
                    CategoryName = "",
                    ProductName = link.InnerText.Split(' ').Last(),
                    Url = "http://otvertka.ru" + link.Attributes["href"].Value,

                });
            }
        }

        public ICommand UpdateSturmLinks
        {
            get
            {
                return new Command(() =>
                {
                    for (int i = 1; i < 50; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "Электроинструмент",
                            Url = "http://otvertka.ru/catalog/electroinstrument/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 7; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "Силовая техника",
                            Url = "http://otvertka.ru/catalog/silovaja-texnika/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 4; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "Сварочное оборудование",
                            Url = "http://otvertka.ru/catalog/svarochnoe-oborudovanie/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 9; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "sadovo-dachnaja",
                            Url = "http://otvertka.ru/catalog/sadovo-dachnaja/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 4; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "klimat",
                            Url = "http://otvertka.ru/catalog/klimat/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 5; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "stanki",
                            Url = "http://otvertka.ru/catalog/stanki/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "sroy-oborudovanie",
                            Url = "http://otvertka.ru/catalog/sroy-oborudovanie/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 5; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "nasosnoe-oborudovanie",
                            Url = "http://otvertka.ru/catalog/nasosnoe-oborudovanie/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 93; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "ruchnoiinstrument",
                            Url = "http://otvertka.ru/catalog/ruchnoiinstrument/?page=" + i + "&brand=3455",
                        });
                    }
                    for (int i = 1; i < 8; i++)
                    {
                        ParseSturmLinks(new URLs
                        {
                            CategoryName = "izmiritilniy-instrument",
                            Url = "http://otvertka.ru/catalog/izmiritilniy-instrument/?page=" + i + "&brand=3455",
                        });
                    }
                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 26";
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
                        ParseSturm(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateSturmPrice
        {
            get
            {
                return new Command(() =>
                {
                    List<Tools> list = new List<Tools>();
                    OpenFileDialog dialog = new OpenFileDialog();
                    string queryString = "SELECT sku\r\nFROM oc_product\r\nWHERE manufacturer_id > 25 and manufacturer_id<30";
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
                        for (int j = 7; j < 2705; j++)
                        {
                            var model = ((worksheet.Cells[j, 5] as Range).Text).ToString();
                            var status = ((worksheet.Cells[j, 7] as Range).Text).ToString();
                            if (status == "" || status == "-")
                                continue;
                            var price = ((dynamic)(worksheet.Cells[j, 15] as Range).Text).ToString();
                            var item = new Tools
                            {
                                Price = decimal.Parse(price),
                                Model = model,
                            };
                            list.Add(item);
                        }
                        application.Quit();
                        foreach (Tools tools2 in list)
                        {
                            if (products.Contains(tools2.Model))
                            {
                                string cmdText = "UPDATE `oc_product` SET\r\nprice = @price, status=TRUE WHERE sku = @model and manufacturer_id > 25 and manufacturer_id<30";
                                MySqlCommand command = new MySqlCommand(cmdText, Connection);
                                command.Parameters.Add(new MySqlParameter("@price", tools2.Price));
                                command.Parameters.Add(new MySqlParameter("@model", tools2.Model));
                                Connection.Open();
                                command.ExecuteNonQuery();
                                Connection.Close();
                                products.Remove(tools2.Model);
                            }
                            else
                                File.AppendAllText("sturm.txt", tools2.Model + "\r\n");
                        }
                        File.AppendAllText("sturm.txt", "------------------\r\n");
                        foreach (var product in products)
                        {
                            File.AppendAllText("sturm.txt", product + "\r\n");
                        }
                        MessageBox.Show("Обновление прайса завершено!");
                    }
                }, null);
            }
        }
    }
}
