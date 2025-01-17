﻿using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using SBUpdater.Helpers;
using SBUpdater.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SBUpdater.Manufacturers
{
    public class filters : Manufacturer, Parser
    {
        private readonly string _link = "https://www.filter.ru/";
        List<URLs> Category = new List<URLs>();
        List<URLs> Cats = new List<URLs>();
        List<URLs> Product = new List<URLs>();
        public void ParseProduct(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var document = html.DocumentNode;
            var img = document.SelectSingleNode("//div[@class='product_list_photo_block']//img");
            var imageLink = _link + img.Attributes["src"].Value;
            var fileName = "Filter/" + Path.GetFileName(imageLink);
            var image = fileName;
            client.DownloadFile(imageLink,fileName);
            var name = img.Attributes["alt"].Value;
            var priceString = document.SelectSingleNode("//div[@class='price']").InnerText;
            var prices = Regex.Matches(priceString, "[0-9]+");
            var oldPrice = 0;
            var price = 0;
            if (prices.Count == 2)
            {
                oldPrice = int.Parse(prices[0].ToString());
                price = int.Parse(prices[1].ToString());
            }
            else
                price = int.Parse(prices[0].ToString());
            var descr = document.SelectSingleNode("//div[@class='tabber']").InnerHtml;
            var links = Regex.Matches(descr, "src=\"(.*?)\"");
            foreach (Match item in links)
            {
                var link = item.Groups[1].Value;
                fileName = "Filter/" + Path.GetFileName(link);
                if (File.Exists(fileName) || link.Contains("youtube"))
                    continue;
                client.DownloadFile(_link + link, fileName);
            }
            if (descr.IndexOf("Дополнительные материалы") != -1)
                descr = descr.Remove(descr.IndexOf("Дополнительные материалы"));
            descr = Regex.Replace(descr, "<a.*?>", "");
            descr = Regex.Replace(descr, "</a.*?>", "");
            descr = Regex.Replace(descr, "(\"img/)(.*?\")", "\"/image/Filter/$2");
            descr = Regex.Replace(descr, "(\"/)([^/]*?\")", "\"/image/Filter/$2");
            descr = Regex.Replace(descr, "(src=\")([^/]*?\")", "$1/image/Filter/$2");
            ConfirmCategory(url.CategoryName);
            Tools tools = new Tools
            {
                CategoryName = url.CategoryName
            };
            ProductDescription description = new ProductDescription
            {
                Description = descr,
                Meta_Description = "",
                Meta_Keyword = ""
            };
            tools.Description = description;
            tools.Height = 0M;
            tools.Image = image.Replace(@"\", "/");
            tools.Length = 0M;
            tools.Manufacturer_id = 8;
            tools.Model = name;
            tools.Price = oldPrice==0 ? price : oldPrice;
            tools.Sku = "";
            tools.Url = url.Url;
            tools.Weight = 1M;
            tools.Width = 1M;
            Products.Add(tools);
            InsertNewProduct();
            if (oldPrice != 0)
            {
                var commandString = "INSERT INTO `oc_product_special`(`product_id`, `customer_group_id`, `priority`, `price`, `date_start`, `date_end`) VALUES (@id,1,0,@price,2016-06-04,2017-01-01)";
                using (var connection = Connection)
                {
                    var command = new MySqlCommand(commandString, connection);
                    command.Parameters.Add("@id", LastProductId);
                    command.Parameters.Add("@price", price);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        public void ParseCategory(URLs url)
        {
            if (url.CategoryName == "Аксессуары")
                return;
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var td = html.DocumentNode.SelectNodes("//div[@class='list_header1']//a");
            foreach (var item in td)
            {
                Category.Add(new URLs
                {
                    Url = _link + item.GetAttributeValue("href", "").Replace("amp;", ""),
                    CategoryName = url.CategoryName + "/" + item.InnerText
                });
            }

        }
        public void ParseProductLinks(URLs url)
        {
            var html = new HtmlDocument();
            var client = new WebClient();
            html.LoadHtml(client.DownloadString(url.Url));
            var categories = html.DocumentNode.SelectNodes("//div[@class='list_header1']//a");
            if (html.DocumentNode.SelectNodes("//div[@class='product_list_photo_block']//a") != null)
            {
                var products = html.DocumentNode.SelectNodes("//div[@class='product_list_photo_block']//a")
                    .Select(x => { return new URLs { Url = (_link + x.GetAttributeValue("href", "")).Replace("amp;", ""), ProductName = x.InnerText, CategoryName = url.CategoryName }; });
                Product.AddRange(products);
            }
            if (categories != null)
                foreach (var cat in categories)
                    ParseProductLinks(new URLs
                    {
                        Url = _link + cat.GetAttributeValue("href", "").Replace("amp;", ""),
                        CategoryName = url.CategoryName + "/" + cat.InnerText
                    });

        }

        public System.Windows.Input.ICommand UpdateCategoryLinks
        {
            get
            {
                return new Command(() =>
                {
                    //var html = new HtmlDocument();
                    //var client = new WebClient();
                    //html.LoadHtml(client.DownloadString(_link));
                    //foreach (var item in links)
                    //    ParseCategory(item);
                    //foreach (var category in Category)
                    //ParseProductLinks(new URLs
                    //{
                    //    CategoryName = "Аксессуары",
                    //    Url = "https://www.filter.ru/index.php?act=prodlist&id=135"
                    //});
                    //foreach (var product in Product)
                    //    ParseProduct(product);
                    //
                    var commandString = "SELECT description, product_id from `oc_product_description` WHERE product_id > 78";
                    var dict = new Dictionary<int, string>();
                    using (var connection = Connection)
                    {
                        connection.Open();
                        var reader = LoadFromDb(commandString);

                        while (reader.Read())
                        {
                            if (reader.GetString(0).Contains("Скачать"))
                                dict.Add(reader.GetInt32(1), reader.GetString(0));
                        }
                        connection.Close();
                        foreach (var item in dict)
                        {
                            var descr = item.Value;
                            descr = descr.Remove(descr.IndexOf("Скачать"));
                            commandString = "UPDATE `oc_product_description` SET description = @descr WHERE product_id = @id";
                            var command = new MySqlCommand(commandString, connection);
                            command.Parameters.Add("@descr", descr);
                            command.Parameters.Add("@id", item.Key);
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }
                    MessageBox.Show("Добавление продуктов завершено");
                });
            }
        }

        public System.Windows.Input.ICommand UpdatePrice
        {
            get { throw new NotImplementedException(); }
        }
    }
}
