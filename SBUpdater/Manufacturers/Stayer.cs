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
    public class Stayer : Manufacturer
    {
        private readonly string _stayerLink = "http://www.stayer-tools.com/";
        private void ParseStayer(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var documentNode = html.DocumentNode;
            var address = documentNode.SelectSingleNode("//div[contains(@class,'photo_container')]").SelectSingleNode(".//a").Attributes["href"].Value;
            var fileName = (@"Stayer\" + url.ProductName + ".jpg").Replace("\"", "").Replace("/", "");
            client.DownloadFile(address, fileName);

            var attributes = new List<List<Attr>>();
            var rows = documentNode.SelectSingleNode("//div[@class='table_har']").SelectNodes(".//tr").ToArray();
            if (url.CategoryName.StartsWith("Электро"))
            {
                attributes.Add(new List<Attr>());
                foreach (var row in rows)
                {
                    var cols = row.ChildNodes.Where(x => x.Name == "td").ToArray();
                    attributes[0].Add(new Attr
                    {
                        AttrName = readFromHtml(cols[0].InnerText),
                        Value = readFromHtml(cols[1].InnerText),
                    });
                }
            }
            else
            {
                var attrNames = rows[0].ChildNodes.Where(x => x.Name == "td").Select(x => readFromHtml(x.InnerText)).ToArray();
                if (rows[0].ChildNodes.First(x => x.Name == "td").Attributes["rowspan"] != null)
                {
                    var tmp = rows.ToList();
                    tmp.RemoveAt(1);
                    rows = tmp.ToArray();
                }
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
                            Value = readFromHtml(cols[l].InnerText)
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
            var compl = documentNode.SelectSingleNode("//td[@class='right_td']");
            var descr = readFromHtml(documentNode.SelectSingleNode("//td[@class='left_td']").InnerHtml).Replace(@"<h2 class=""title_02_site"">Особенности</h2>", "").Replace("\t", "").Replace("<p>&nbsp;</p>", "").Replace("<br>", "");
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
                    Meta_Description = metaDescription.Replace("{0}", "Stayer " + name),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Stayer " + name)
                };
                tools.Description = description;
                tools.Height = 1M;
                tools.Image = "data/" + fileName.Replace(@"\", "/");
                tools.Length = 1M;
                tools.Manufacturer_id = 22;
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

        private void ParseStayerLinks(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var cats = html.DocumentNode.SelectSingleNode("//div[@class='left_menu']").SelectSingleNode(".//ul").SelectNodes(".//li").Where(x => x.ChildNodes.First(c => c.Name == "a").InnerHtml.Contains("strong")).First()
                .SelectSingleNode(".//ul").SelectNodes(".//li").ToArray();
            var catName = "";
            foreach (var prod in cats)
                if (prod.ChildNodes.Count == 1)
                    continue;
                else if (prod.ChildNodes.FirstOrDefault(x => x.Name == "ul") != null)
                    catName = readFromHtml(prod.FirstChild.InnerText).Replace("\n", "").Replace("\t", "");
                else
                {
                    var link = prod.ChildNodes.First(x => x.Name == "a");
                    ProductUrls.Add(new URLs
                    {
                        CategoryName = url.CategoryName + "/" + catName,
                        ProductName = readFromHtml(link.InnerText),
                        Url = link.Attributes["href"].Value,
                    });
                }
        }

        public ICommand UpdateStayerLinks
        {
            get
            {
                return new Command(() =>
                {
                    var html = new HtmlDocument();
                    var client = new WebClient();
                    html.LoadHtml(client.DownloadString(_stayerLink));
                    var doc = html.DocumentNode;
                    var lists = doc.SelectNodes("//li[contains(@class,'catmenu')]");
                    foreach (var cat in lists)
                    {
                        var catName = readFromHtml(cat.ChildNodes.First(x => x.Name == "a").InnerText.Replace("<br>", " ").Replace("  ", " "));
                        var cats = cat.ChildNodes.First(x => x.Name == "ul").ChildNodes.Where(x => x.Name == "li").ToArray();
                        foreach (var item in cats)
                        {
                            var link = item.ChildNodes.First(x => x.Name == "a");
                            CategoryUrls.Add(new URLs
                            {
                                CategoryName = catName + "/" + readFromHtml(link.InnerText),
                                Url = link.Attributes["href"].Value,
                            });
                        }
                    }
                    foreach (var link in CategoryUrls)
                        ParseStayerLinks(link);

                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 22";
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
                        ParseStayer(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateStayerPrice
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
                        for (int j = 3820; j < 7663; j++)
                        {
                            var model = ((worksheet.Cells[j, 1] as Range).Text).ToString() as string;
                            // model = model.Contains("_") ? model.Remove(model.LastIndexOf('_')) : model;
                            if (model == "")
                                continue;
                            var rec = ((dynamic)(worksheet.Cells[j, 12] as Range).Text).ToString();
                            var priceString = ((dynamic)(worksheet.Cells[j, 4] as Range).Text).ToString();
                            decimal price;
                            if (rec == "0")
                                price = (decimal)Convert.ToInt32(int.Parse((priceString).Remove((priceString).IndexOf(',')).Replace(" ", "")) * 1.23);
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
                            string cmdText = "UPDATE `oc_product` SET\r\nprice = @price WHERE sku = @sku and manufacturer_id = 22";
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
