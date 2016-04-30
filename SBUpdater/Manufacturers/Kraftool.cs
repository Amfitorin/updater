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
    public class Kraftool : Manufacturer
    {
        private readonly string _kraftoolLink = "http://kraftool.com/test/RUS/";
        private void ParseKraftool(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var documentNode = html.DocumentNode;
            var address = _kraftoolLink + documentNode.SelectNodes("//td").Where(x => x.Attributes["background"] != null && x.Attributes["background"].Value == "../images/bg-t.gif").First().SelectSingleNode(".//img").Attributes["src"].Value.Remove(0, 3);
            var fileName = (@"Kraftool\" + url.ProductName + ".gif").Replace("\"", "").Replace("/", "").Replace("\"", "").Replace(":", "").Replace("&quot;", "");
            client.DownloadFile(address, fileName);
            var productName = documentNode.SelectSingleNode("//td[@class='zagTovar']").ChildNodes[1].InnerText;
            var articul = productName.Substring(productName.LastIndexOf("(") + 1).TrimEnd(')');
            productName = productName.Remove(productName.LastIndexOf("("));
            var desNode = documentNode.SelectSingleNode("//td[@class='text']");
            var attributNode = desNode.ChildNodes.FirstOrDefault(x => x.Name == "table");
            var attributes = new List<List<Attr>>();
            var descr = "";
            var attrNames = new List<string>();
            if (attributNode != null)
            {
                descr = desNode.InnerHtml.Replace(attributNode.OuterHtml, "").Replace("\t", "");
                var rows = attributNode.SelectNodes(".//tr").ToArray();
                var l = 0;
                var dop = new Dictionary<inde, HtmlNode>();
                var n = 0;
                var count = 1;
                for (int i = 0; i < rows.Length; i++)
                {

                    var cols = rows[i].SelectNodes(".//td").ToList();
                    var tmp = new Dictionary<inde, HtmlNode>(dop);
                    foreach (var item in dop)
                    {
                        var key = item.Key;
                        key.count -= 1;
                        var value = item.Value;
                        if (value.Attributes["rowspan"] != null)
                            value.Attributes["rowspan"].Remove();
                        tmp.Remove(key);
                        if (key.count > 0)
                            tmp.Add(key, value);
                        cols.Insert(key.colNum, value);
                    }
                    if (i == 0)
                    {
                        foreach (var col in cols)
                            if (col.InnerText.Contains("Артикул"))
                            {
                                if (col.Attributes["colspan"] != null)
                                    l += int.Parse(col.Attributes["colspan"].Value) - 1;
                                attrNames.Add(col.InnerText);
                                n = cols.IndexOf(col);
                            }
                            else
                                attrNames.Add(col.InnerText);

                    }
                    else
                    {
                        if (cols[0].Attributes["colspan"] != null)
                        {
                            count++;
                            continue;
                        }
                        attributes.Add(new List<Attr>());
                        for (int j = l; j < cols.Count; j++)
                        {
                            if (cols[j].Attributes["rowspan"] != null)
                            {
                                dop.Add(new inde
                                {
                                    colNum = j,
                                    count = int.Parse(cols[j].Attributes["rowspan"].Value) - 1
                                }, cols[j]);
                            }
                            attributes[i - count].Add(new Attr
                            {
                                AttrName = attrNames[j - l],
                                Value = cols[j].InnerText,
                            });
                        }
                        attributes[i - count].Reverse(0, n + 1);
                    }
                }
                attrNames.Reverse(0, n + 1);

            }
            else
            {
                attributes.Add(new List<Attr>());
                attributes[0].Add(new Attr
                {
                    AttrName = "Артикул",
                    Value = articul
                });
            }


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
                var name = productName + " " + attrs[0].Value;
                var description = new ProductDescription
                {
                    Description = descr,
                    Meta_Description = metaDescription.Replace("{0}", "Kraftool " + name),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Kraftool " + name)
                };
                tools.Description = description;
                tools.Height = 1M;
                tools.Image = "data/" + fileName.Replace(@"\", "/");
                tools.Length = 1M;
                tools.Manufacturer_id = 23;
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

        private void ParseKraftoolLinks(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            HtmlNode[] links;
            if (html.DocumentNode.SelectNodes("//a[@class='list']") != null)
            {
                links = html.DocumentNode.SelectNodes("//a[@class='list']").ToArray();
                foreach (var prod in links)

                    ProductUrls.Add(new URLs
                    {
                        CategoryName = url.CategoryName,
                        ProductName = prod.InnerText,
                        Url = _kraftoolLink + prod.Attributes["href"].Value.Replace("../", ""),
                    });
            }
            else
                ProductUrls.Add(new URLs
                {
                    CategoryName = url.CategoryName,
                    ProductName = "",
                    Url = url.Url,
                });
        }

        public ICommand UpdateKraftoolLinks
        {
            get
            {
                return new Command(() =>
                {
                    //var html = new HtmlDocument();
                    //var client = new WebClient();
                    //html.LoadHtml(client.DownloadString(_kraftoolLink + "catalog.htm"));
                    //var doc = html.DocumentNode;
                    //var lists = doc.SelectNodes("//td[@class='pathbold']").Where(x => x.ChildNodes.Any(z => z.Name == "table")).ToArray();
                    //foreach (var list in lists)
                    //{
                    //    var rows = list.SelectNodes(".//tr").Where(x => !x.ChildNodes.Any(z => z.Name == "td" && z.Attributes["colspan"] != null && z.Attributes["colspan"].Value != "2")).ToArray();
                    //    var catName = "";
                    //    for (int i = 0; i < rows.Length; i++)
                    //        if (i == 0)
                    //            catName = rows[i].SelectSingleNode(".//a[@class='textSmall']").InnerText;
                    //        else
                    //        {
                    //            var link = rows[i].SelectSingleNode(".//a[@class='pathbold']");
                    //            CategoryUrls.Add(new URLs
                    //            {
                    //                CategoryName = catName + "/" + link.InnerText,
                    //                Url = _kraftoolLink + link.Attributes["href"].Value,
                    //            });
                    //        }
                    //}
                    CategoryUrls.Add(new URLs
                    {
                        CategoryName = "Сверла",
                        Url = "http://kraftool.com/test/RUS/razdel/06.htm"
                    });
                    foreach (var link in CategoryUrls)
                        ParseKraftoolLinks(link);

                    string queryString = "SELECT model\r\nFROM oc_product\r\nWHERE manufacturer_id = 23";
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
                        ParseKraftool(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdateKraftoolPrice
        {
            get
            {
                return new Command(() =>
                {
                    List<Tools> list = new List<Tools>();
                    OpenFileDialog dialog = new OpenFileDialog();
                    string queryString = "SELECT sku\r\nFROM oc_product\r\nWHERE manufacturer_id = 21 && price='1.0000'";
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
                        for (int j = 1103; j < 3121; j++)
                        {
                            var model = ((worksheet.Cells[j, 1] as Range).Text).ToString() as string;
                            model = model.Contains("_") ? model.Remove(model.LastIndexOf('_')) : model;
                            if (model == "" || products.FirstOrDefault(x => x == model) == null)
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
                            string cmdText = "UPDATE `oc_product` SET\r\nprice = @price WHERE sku = @sku and manufacturer_id = 23";
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
