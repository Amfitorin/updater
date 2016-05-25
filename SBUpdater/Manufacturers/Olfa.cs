using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SBUpdater.Models;
using SBUpdater.Helpers;
using HtmlAgilityPack;
using System.Net;
using MySql.Data.MySqlClient;

namespace SBUpdater.Manufacturers
{
    class Olfa : Manufacturer, Parser
    {
        private readonly string _link = "http://www.olfa.ru/";
        public ICommand UpdateCategoryLinks
        {
            get
            {
                return new Command(() =>
                {
                    ReadOfFile();
                    ReadCategoryes();
                    var html = new HtmlDocument();
                    var client = new WebClient();
                    html.LoadHtml(readFromHtml(client.DownloadString(_link)));
                    var links = html.DocumentNode.SelectNodes("//td[@class='menu']/a");
                    foreach (var link in links)
                    {
                        ParseProductLinks(new URLs
                        {
                            CategoryName = link.InnerText,
                            Url = link.GetAttributeValue("href", "")
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
                        ParseProduct(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);
                }, null);
            }
        }

        public ICommand UpdatePrice
        {
            get
            {
                return null;
            }
        }

        public void ParseProduct(URLs url)
        {
            
        }

        public void ParseProductLinks(URLs url)
        {
            
        }
    }
}
