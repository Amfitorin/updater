//using MySql.Data.MySqlClient;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace SBUpdater.SiteParcers
//{
//    public class MainParcer
//    {
//        public static MySqlConnection Connection;
//        private Encoding startEncoding = Encoding.GetEncoding(1251);
//        private Encoding endEncoding = Encoding.UTF8;

//        public  string readFromHtml(string attr)
//        {
//            return endEncoding.GetString(startEncoding.GetBytes(attr));
//        }

//        public MySqlDataReader LoadFromDb(string queryString)
//        {
//            var command = new MySqlCommand(queryString, Connection);
//            try
//            {
//                var rdr = command.ExecuteReader();
//                return rdr;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//                return null;
//            }
//        }

//        public void WriteOnFile()
//        {
//            var text = "";
//            foreach (var item in AttributesConst)
//            {
//                text += item.Key + "|" + item.Value.ToString() + "`";
//            }

//            File.WriteAllText("attributes.txt", text);
//        }

//        public void ReadOfFile()
//        {
//            var text = File.ReadAllText("attributes.txt");
//            var array = text.Split('`');
//            foreach (var item in array)
//            {
//                if (item == "")
//                    continue;
//                var attr = item.Split('|');
//                AttributesConst.Add(attr[0], new attrCat(int.Parse(attr[1]), int.Parse(attr[2])));
//            }
//        }

//        public void WriteCategoryes()
//        {
//            var text = "";
//            foreach (var item in CategoryConst)
//            {
//                text += item.Key + "|" + item.Value.ToString() + "#";
//            }

//            File.WriteAllText("categoryes.txt", text);
//        }

//        public void ReadCategoryes()
//        {
//            var text = File.ReadAllText("categoryes.txt");
//            var array = text.Split('#');
//            foreach (var item in array)
//            {
//                if (item == "")
//                    continue;
//                var attr = item.Split('|');
//                CategoryConst.Add(attr[0], int.Parse(attr[1]));
//            }
//        }

//        public void InsertNewProduct()
//        {
//            using (var con = Connection)
//            {
//                var date = DateTime.Now;
//                var product = Products.First();
//                var productQuery =
//@"INSERT INTO `oc_product`
//(`product_id`, `model`, `sku`, `upc`, `ean`, `jan`, `isbn`, `mpn`, `location`, `quantity`, `stock_status_id`, 
//`image`, `manufacturer_id`, `shipping`, `price`, `points`, `tax_class_id`, `date_available`, `weight`, `weight_class_id`,
//`length`, `width`, `height`, `length_class_id`, `subtract`, `minimum`, `sort_order`, `status`, `date_added`, `date_modified`, 
//`viewed`)
//VALUES (@product_id,@model,@sku,"""","""","""","""","""","""",100,6,@image,@manufacturer_id,TRUE,1,0,0,@date,1,1,1,1,1,2,FALSE,1,0,TRUE,@date,@date,1)";
//                var productCom = new MySqlCommand(productQuery, con);
//                productCom.Parameters.Add(new MySqlParameter("@product_id", ++LastProductId));
//                productCom.Parameters.Add(new MySqlParameter("@model", product.Model));
//                productCom.Parameters.Add(new MySqlParameter("@sku", product.Sku));
//                productCom.Parameters.Add(new MySqlParameter("@image", product.Image));
//                productCom.Parameters.Add(new MySqlParameter("@manufacturer_id", product.Manufacturer_id));
//                productCom.Parameters.Add(new MySqlParameter("@date", date));

//                var productDescriptionQuery =
//    @"INSERT INTO `oc_product_description`
//(`product_id`, `language_id`, `name`, `description`, `meta_description`, `meta_keyword`, `tag`) 
//VALUES (@product_id,3,@name,@description,@meta_description,@meta_keyword,"""")";
//                var productDescriptionCom = new MySqlCommand(productDescriptionQuery, con);
//                productDescriptionCom.Parameters.Add(new MySqlParameter("@product_id", LastProductId));
//                productDescriptionCom.Parameters.Add(new MySqlParameter("@name", "Hitachi " + product.Model));
//                productDescriptionCom.Parameters.Add(new MySqlParameter("@description", product.Description.Description));
//                productDescriptionCom.Parameters.Add(new MySqlParameter("@meta_description", product.Description.Meta_Description));
//                productDescriptionCom.Parameters.Add(new MySqlParameter("@meta_keyword", product.Description.Meta_Keyword));

//                var productCategoryQuery =
//    @"INSERT INTO `oc_product_to_category`(`product_id`, `category_id`) VALUES (@product_id,@category_id)";
//                var productCategoryCom = new MySqlCommand(productCategoryQuery, con);
//                productCategoryCom.Parameters.Add(new MySqlParameter("@product_id", LastProductId));
//                productCategoryCom.Parameters.Add(new MySqlParameter("@category_id", CategoryConst[product.CategoryName]));

//                var productStoreQuery =
//    @"INSERT INTO `oc_product_to_store`(`product_id`, `store_id`) VALUES (@product_id,0)";
//                var productStoreCom = new MySqlCommand(productStoreQuery, con);
//                productStoreCom.Parameters.Add(new MySqlParameter("@product_id", LastProductId));

//                con.Open();
//                productCom.ExecuteNonQuery();
//                productDescriptionCom.ExecuteNonQuery();
//                productCategoryCom.ExecuteNonQuery();
//                productStoreCom.ExecuteNonQuery();
//                con.Close();

//                foreach (var attribute in product.Attributes)
//                {
//                    var productAttributeQuery =
//    @"INSERT INTO `oc_product_attribute`(`product_id`, `attribute_id`, `language_id`, `text`) 
//VALUES (@product_id,@attribute_id,3,@text)";
//                    var productAttributeCom = new MySqlCommand(productAttributeQuery, con);
//                    productAttributeCom.Parameters.Add(new MySqlParameter("@product_id", LastProductId));
//                    productAttributeCom.Parameters.Add(new MySqlParameter("@attribute_id", attribute.Id));
//                    productAttributeCom.Parameters.Add(new MySqlParameter("@text", attribute.Value));
//                    con.Open();
//                    productAttributeCom.ExecuteNonQuery();
//                    con.Close();
//                }
//                Products.Clear();
//            }
//        }

//        private void UpdateCategoryNames()
//        {
//            foreach (var item in Categories)
//            {  
//                if (item.Parent_Id != 0)
//                    item.Name = Categories.First(x => x.Id == item.Parent_Id).Name + "/" + item.Name;
//            }
//        }

//    }
//}
