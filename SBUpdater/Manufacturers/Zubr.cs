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
using System.Text.RegularExpressions;
using System.IO;

namespace SBUpdater.Manufacturers
{
    public class Zubr : Manufacturer
    {
        private readonly string _zubrLink = "http://zubr.ru/";
        private void ParseZubr(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var documentNode = html.DocumentNode;
            var address = _zubrLink + documentNode.SelectSingleNode("//a[@data-id='main-image']").Attributes["href"].Value;
            var fileName = (@"Zubr\" + url.ProductName + ".png").Replace("\"", "").Replace("/", "");
            if (!Directory.Exists("Zubr"))
                Directory.CreateDirectory("Zubr");
            client.DownloadFile(address, fileName);
            var flag = html.GetElementbyId("specifications").ChildNodes.FirstOrDefault(x => x.Name == "table") == null;
            var flag2 = html.GetElementbyId("specifications").ChildNodes.FirstOrDefault(x => x.Name == "p") == null ? true : html.GetElementbyId("specifications").ChildNodes.FirstOrDefault(x => x.Name == "p").ChildNodes.FirstOrDefault(x => x.Name == "table") == null;
            var flag3 = html.GetElementbyId("specifications").ChildNodes.FirstOrDefault(x => x.Name == "div") == null;
            var attributes = new List<List<Attr>>();
            if (flag && flag2 && flag3)
                attributes.Add(new List<Attr>{
                    new Attr
                    {
                        AttrName = "Артикул",
                        Value = html.GetElementbyId("specifications").ChildNodes.First(x => x.Name == "p").ChildNodes.First(x => x.Name == "strong").InnerText.Trim(' ')
                    }});
            else
            {
                HtmlNode[] rows;
                if (!flag2)
                    rows = html.GetElementbyId("specifications").ChildNodes.First(x => x.Name == "p").ChildNodes.First(x => x.Name == "table").ChildNodes[1].ChildNodes.Where(x => x.Name == "tr").ToArray();
                else if (!flag3)
                    rows = html.GetElementbyId("specifications").ChildNodes.First(x => x.Name == "div").ChildNodes.First(x => x.Name == "table").ChildNodes[1].ChildNodes.Where(x => x.Name == "tr").ToArray();
                else
                    rows = html.GetElementbyId("specifications").ChildNodes.First(x => x.Name == "table").ChildNodes[1].ChildNodes.Where(x => x.Name == "tr").ToArray();
                var s = rows[0].ChildNodes.Where(x => x.Name == "td").ToArray()[1].InnerText;
                var reg = new Regex(@"\d");
                var first = url.ProductName.Contains(s) || reg.Match(s).Success;
                if (first)
                {
                    attributes.Add(new List<Attr>());
                    foreach (var row in rows)
                    {
                        var cols = row.ChildNodes.Where(x => x.Name == "td").ToArray();
                        attributes[0].Add(new Attr
                        {
                            AttrName = cols[0].InnerText,
                            Value = cols[1].InnerText
                        });
                    }
                }
                else
                {
                    var attrNames = rows[0].ChildNodes.Where(x => x.Name == "td").Select(x => x.InnerText).ToArray();

                    var dop = new Dictionary<inde, string>();
                    for (int i = 1; i < rows.Length; i++)
                    {
                        var l = 0;
                        attributes.Add(new List<Attr>());
                        var cols = rows[i].ChildNodes.Where(x => x.Name == "td").ToArray();
                        for (var j = 0; j < attrNames.Length; j++)
                        {
                            if (dop.Keys.FirstOrDefault(x => x.colNum == j) != null)
                            {
                                var key = dop.Keys.FirstOrDefault(x => x.colNum == j);
                                var value = dop[key];
                                dop.Remove(key);
                                if (key.count - 1 > 0)
                                    dop.Add(new inde
                                    {
                                        colNum = j,
                                        count = key.count - 1
                                    }, value);
                                attributes[i - 1].Add(new Attr
                                {
                                    AttrName = attrNames[j],
                                    Value = value
                                });
                                continue;
                            }
                            attributes[i - 1].Add(new Attr
                            {
                                AttrName = attrNames[j],
                                Value = cols[l].InnerText
                            });

                            if (cols[l].Attributes["rowspan"] != null)
                            {
                                dop.Add(new inde
                                {
                                    colNum = j,
                                    count = int.Parse(cols[l].Attributes["rowspan"].Value) - 1
                                }, cols[l].InnerText);
                            }
                            l++;
                        }
                    }
                }
            }
            var descr = html.GetElementbyId("features").InnerHtml;
            ConfigureAttr(attributes[0]);
            WriteOnFile();
            ConfirmCategory(url.CategoryName);
            WriteCategoryes();
            foreach (var attrs in attributes)
            {
                var attribute = new List<Models.Attribute>();
                var n = attrs.Count;
                for (int i = 1; i < n; i++)
                    attribute.Add(new Models.Attribute
                    {
                        Attribute_Group_Id = AttributesConst[attrs[i].AttrName].groupId,
                        Id = AttributesConst[attrs[i].AttrName].attrId,
                        Sort_Order = 0,
                        Value = attrs[i].Value,
                        Name = Attributes.First(x => x.Id == AttributesConst[attrs[i].AttrName].attrId).Name
                    });
                Tools tools = new Tools
                {
                    Attributes = attribute,
                    CategoryName = url.CategoryName
                };
                var name = url.ProductName.Contains(attrs[0].Value) ? url.ProductName : url.ProductName + " " + attrs[0].Value;
                var description = new ProductDescription
                {
                    Description = descr,
                    Meta_Description = metaDescription.Replace("{0}", "Зубр " + name),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Зубр " + name)
                };
                tools.Description = description;
                tools.Height = 1M;
                tools.Image = "data/" + fileName.Replace(@"\", "/");
                tools.Length = 1M;
                tools.Manufacturer_id = 21;
                tools.Model = name;
                tools.Price = 1M;
                tools.Sku = attrs[0].Value;
                tools.Url = url.Url;
                tools.Weight = 1M;
                tools.Width = 1M;
                Products.Add(tools);
                InsertNewProduct();
            }
        }

        private void ParseZubrLinks(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var catProd = html.GetElementbyId("menu-item").ChildNodes.First(x => x.Name == "ul").ChildNodes.Where(x => x.Name == "li").ToArray();
            var catName = "";
            foreach (var prod in catProd)
                if (prod.Attributes.Count == 0)
                    catName = prod.InnerText;
                else
                {
                    var link = prod.ChildNodes.First(x => x.Name == "a");
                    ProductUrls.Add(new URLs
                    {
                        CategoryName = url.CategoryName + "/" + catName,
                        ProductName = link.InnerText,
                        Url = _zubrLink + link.Attributes["href"].Value.TrimStart('/'),
                    });
                }
        }

        public ICommand UpdateZubrLinks
        {
            get
            {
                return new Command(() =>
                {
                    var html = new HtmlDocument();
                    var client = new WebClient();
                    html.LoadHtml(client.DownloadString(_zubrLink));
                    var doc = html.DocumentNode;
                    var lists = doc.SelectSingleNode("//div[@class='cat-footer']").ChildNodes.First(x => x.Name == "ul").ChildNodes.Where(x => x.Name == "li").ToArray();
                    foreach (var cat in lists)
                    {
                        var catName = cat.ChildNodes.First(x => x.Name == "div").ChildNodes.First(x => x.Name == "strong").InnerText;
                        var cats = cat.ChildNodes.First(x => x.Name == "ul").ChildNodes.Where(x => x.Name == "li").ToArray();
                        foreach (var item in cats)
                        {
                            var link = item.ChildNodes.First(x => x.Name == "a");
                            CategoryUrls.Add(new URLs
                            {
                                CategoryName = catName + "/" + link.InnerText,
                                Url = _zubrLink + link.Attributes["href"].Value.TrimStart('/'),
                            });
                        }
                    }
                    foreach (var link in CategoryUrls)
                        ParseZubrLinks(link);

                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 21";
                    Connection.Open();
                    List<string> products = new List<string>();
                    MySqlDataReader reader = LoadFromDb(queryString);
                    while (reader.Read())
                    {
                        products.Add(reader.GetString(0));
                    }
                    Connection.Close();
                    ProductUrls = ProductUrls.Where(x => !products.Any(l => l.Contains(x.ProductName))).ToList();
                    do
                    {
                        ParseZubr(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateZubrPrice
        {
            get
            {
                return new Command(() =>
                {
                    List<Tools> list = new List<Tools>();
                    OpenFileDialog dialog = new OpenFileDialog();
                    string queryString = "SELECT sku\r\nFROM oc_product\r\nWHERE manufacturer_id >= 21 and manufacturer_id <= 23";
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
                        for (int j = 2; j < 17973; j++)
                        {
                            var model = ((worksheet.Cells[j, 1] as Range).Text).ToString() as string;
                            var status = ((worksheet.Cells[j, 13] as Range).Text).ToString() as string;
                            if (products.FirstOrDefault(x => x == model) == null || status=="Нет")
                            {
                                if (status != "Нет")
                                    File.AppendAllText("zubr.price.txt",model+" "+ ((worksheet.Cells[j, 9] as Range).Text).ToString() as string  +"\r\n");
                                continue;
                            }
                            var rec = ((dynamic)(worksheet.Cells[j, 12] as Range).Text).ToString();
                            var priceString = ((dynamic)(worksheet.Cells[j, 4] as Range).Text).ToString();
                            decimal price;
                            if (rec == "0")
                                price = (decimal)Convert.ToInt32(int.Parse((priceString).Remove((priceString).IndexOf(',')).Replace(" ", "")) * 1.27);
                            else
                                price = Convert.ToDecimal(rec);
                            var item = new Tools
                            {
                                Price = price,
                                Model = model,
                            };
                            list.Add(item);
                        }
                        application.Quit();
                        foreach (Tools tools2 in list)
                        {
                            string cmdText = "UPDATE `oc_product` SET\r\nprice = @price, status=1 WHERE sku = @sku and  manufacturer_id >= 21 and manufacturer_id <= 23";
                            MySqlCommand command = new MySqlCommand(cmdText, Connection);
                            command.Parameters.Add(new MySqlParameter("@price", tools2.Price));
                            command.Parameters.Add(new MySqlParameter("@sku", tools2.Model));
                            Connection.Open();
                            command.ExecuteNonQuery();
                            Connection.Close();
                            System.Threading.Thread.Sleep(20);
                        }
                    }
                }, null);
            }
        }
    }
}
