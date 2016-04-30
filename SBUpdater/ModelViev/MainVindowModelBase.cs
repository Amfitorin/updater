namespace SBUpdater.ModelViev
{
    using HtmlAgilityPack;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.Office.Interop.Excel;
    using Microsoft.Win32;
    using MySql.Data.MySqlClient;
    using SBUpdater.Helpers;
    using SBUpdater.Models;
    using SBUpdater.Properties;
    using SBUpdater.Viev;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Input;

    public class MainWindowModelBase : ModelBase
    {

        #region Vars
        public static string _attrGrName = "";
        public static string _attrGroupName = "";
        public static List<string> _attrGroupNames = new List<string>();
        public static List<AttributeGroup> _attrGroups;
        public static Dictionary<string, int> _attrGroupsDictionary;
        public static List<SBUpdater.Models.Attribute> _attributes;
        public static string _attrName = "";
        public static List<string> _attrNames = new List<string>();
        public static List<Category> _categories;
        public static List<string> _categoryNames = new List<string>();
        public static string _currentAttrName;
        public static List<string> _currentAttrNames = new List<string>();
        public static string _currentCatName;
        public static List<string> _currentCatNames = new List<string>();

        public static string _newAttrGrName = "";
        public static string _newAttrName = "";
        public static string _newCatName = "";
        public static string _parentCat = "";
        public static List<string> _skus;
        private SBUpdater.Viev.AddAttributeGroup AddAttributeGroupWindow;
        private SBUpdater.Viev.AddAttribute AddAttributeWindow;
        private SBUpdater.Viev.AddCategory AddCategoryWindow;
        
        public Dictionary<string, int> CategoryConst = new Dictionary<string, int>();
        public List<URLs> CategoryUrls = new List<URLs>();
        public SBUpdater.Viev.ConfigureAttr ConfigureAttrVindow;
        private SBUpdater.Viev.ConfirmCategory ConfirmCategoryWindow;
        public static MySqlConnection Connection;
        private static readonly string description = "Купить {0} по лучшим ценам.";
        
        private bool isSaved = false;
        public static int LastAttrGroupId = 0;
        public static int LastAttributeId = 0;
        public static int LastCategoryId = 0;
        public static int LastProductId = 0;
        public static List<Manufacturer> Manufactures;
        
        public List<Tools> Products = new List<Tools>();
        public List<URLs> ProductUrls = new List<URLs>();
        private SBUpdater.Viev.SettingWindow SettingWindow;
        
        #endregion

        #region Categories
        private void ReadCategoryes()
        {
            string[] strArray = System.IO.File.ReadAllText("categoryes.txt").Split(new char[] { '#' });
            foreach (string str2 in strArray)
            {
                if (!(str2 == ""))
                {
                    string[] strArray2 = str2.Split(new char[] { '|' });
                    CategoryConst.Add(strArray2[0], int.Parse(strArray2[1]));
                }
            }
        }
        private void UpdateCategoryNames()
        {
            using (List<Category>.Enumerator enumerator = Categories.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<Category, bool> predicate = null;
                    Category item = enumerator.Current;
                    if (item.Parent_Id != 0)
                    {
                        if (predicate == null)
                        {
                            predicate = x => x.Id == item.Parent_Id;
                        }
                        item.Name = Categories.First<Category>(predicate).Name + "/" + item.Name;
                    }
                }
            }
        }

        private void UpdateCategoryNames(Category category)
        {
            if (category.Parent_Id != 0)
            {
                category.Name = Categories.First<Category>(x => (x.Id == category.Parent_Id)).Name + "/" + category.Name;
            }
        }

        

        public ICommand AddCategory
        {
            get
            {
                return new Command(() =>
                {
                    AddCategoryWindow = new SBUpdater.Viev.AddCategory();
                    AddCategoryWindow.DataContext = this;
                    AddCategoryWindow.ShowDialog();
                }, null);
            }
        }

        public List<Category> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                _categories = value;
                FirePropertyChanged("Categories");
            }
        }

        public List<string> CategoryNames
        {
            get
            {
                return _categoryNames;
            }
            set
            {
                _categoryNames = value;
                FirePropertyChanged("CategoryNames");
            }
        }

        #endregion

        #region all
        public class inde
        {
            public int colNum;
            public int count;
        }
        public MainWindowModelBase()
        {
            DB = new DatabaseConnectModel();
            DB.DatabaseName = Settings.Default.DataBaseName ?? "";
            DB.DatabasePassword = Settings.Default.DatabasePassword ?? "";
            DB.DatabaseUserId = Settings.Default.DatabaseUserId ?? "";
            DB.DatabaseServer = Settings.Default.DatabaseServer ?? "";
        }

       

        public void ConfirmCategory(string cat)
        {
            if (!CategoryConst.ContainsKey(cat))
            {
                CurrentCatName = cat;
                ConfirmCategoryWindow = new SBUpdater.Viev.ConfirmCategory();
                ConfirmCategoryWindow.DataContext = this;
                ConfirmCategoryWindow.ShowDialog();
            }
        }

        public void InsertNewProduct()
        {
            using (MySqlConnection connection = Connection)
            {
                DateTime now = DateTime.Now;
                Tools product = Products.First<Tools>();

                string cmdText = "INSERT INTO `oc_product`\r\n(`product_id`, `model`, `sku`, `upc`, `ean`, `jan`, `isbn`, `mpn`, `location`, `quantity`, `stock_status_id`, \r\n`image`, `manufacturer_id`, `shipping`, `price`, `points`, `tax_class_id`, `date_available`, `weight`, `weight_class_id`,\r\n`length`, `width`, `height`, `length_class_id`, `subtract`, `minimum`, `sort_order`, `status`, `date_added`, `date_modified`, \r\n`viewed`)\r\nVALUES (@product_id,@model,@sku,\"\",\"\",\"\",\"\",\"\",\"\",100,6,@image,@manufacturer_id,TRUE,1,0,0,@date,1,1,1,1,1,2,TRUE,1,0,FALSE,@date,@date,1)";
                MySqlCommand command = new MySqlCommand(cmdText, connection);
                command.Parameters.Add(new MySqlParameter("@product_id", ++LastProductId));
                command.Parameters.Add(new MySqlParameter("@model", product.Model));
                command.Parameters.Add(new MySqlParameter("@sku", product.Sku));
                command.Parameters.Add(new MySqlParameter("@image", product.Image));
                command.Parameters.Add(new MySqlParameter("@manufacturer_id", product.Manufacturer_id));
                command.Parameters.Add(new MySqlParameter("@date", now));
                string str2 = "INSERT INTO `oc_product_description`\r\n(`product_id`, `language_id`, `name`, `description`, `meta_description`, `meta_keyword`, `tag`) \r\nVALUES (@product_id,3,@name,@description,@meta_description,@meta_keyword,\"\")";
                MySqlCommand command2 = new MySqlCommand(str2, connection);
                command2.Parameters.Add(new MySqlParameter("@product_id", LastProductId));
                var name = "";
                if (product.Model.Contains(product.Sku))
                    name = "\"" + product.Sku + "\"";
                else name = product.Model + " \"" + product.Sku + "\"";
                command2.Parameters.Add(new MySqlParameter("@name", product.Name + " " + Manufactures.First<Manufacturer>(x => (x.Id == product.Manufacturer_id)).Name + " " + name));
                command2.Parameters.Add(new MySqlParameter("@description", product.Description.Description));
                command2.Parameters.Add(new MySqlParameter("@meta_description", product.Description.Meta_Description));
                command2.Parameters.Add(new MySqlParameter("@meta_keyword", product.Description.Meta_Keyword));
                string str3 = "INSERT INTO `oc_product_to_category`(`product_id`, `category_id`) VALUES (@product_id,@category_id)";
                MySqlCommand command3 = new MySqlCommand(str3, connection);
                command3.Parameters.Add(new MySqlParameter("@product_id", LastProductId));
                command3.Parameters.Add(new MySqlParameter("@category_id", CategoryConst[product.CategoryName]));
                string str4 = "INSERT INTO `oc_product_to_store`(`product_id`, `store_id`) VALUES (@product_id,0)";
                MySqlCommand command4 = new MySqlCommand(str4, connection);
                command4.Parameters.Add(new MySqlParameter("@product_id", LastProductId));
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    command2.ExecuteNonQuery();
                    command3.ExecuteNonQuery();
                    command4.ExecuteNonQuery();
                    connection.Close();
                    List<int> list = new List<int>();
                    foreach (SBUpdater.Models.Attribute attribute in product.Attributes)
                    {
                        if (!list.Contains(attribute.Id))
                        {
                            string str5 = "INSERT INTO `oc_product_attribute`(`product_id`, `attribute_id`, `language_id`, `text`) \r\nVALUES (@product_id,@attribute_id,3,@text)";
                            MySqlCommand command5 = new MySqlCommand(str5, connection);
                            command5.Parameters.Add(new MySqlParameter("@product_id", LastProductId));
                            command5.Parameters.Add(new MySqlParameter("@attribute_id", attribute.Id));
                            command5.Parameters.Add(new MySqlParameter("@text", attribute.Value));
                            connection.Open();
                            command5.ExecuteNonQuery();
                            connection.Close();
                            list.Add(attribute.Id);
                        }
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                { }
            }
            Products.Clear();

        }

        public MySqlDataReader LoadFromDb(string queryString)
        {
            MySqlCommand command = new MySqlCommand(queryString, Connection);
            try
            {
                return command.ExecuteReader();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return null;
            }
        }

        public ICommand AddAttribute
        {
            get
            {
                return new Command(() =>
                {
                    AddAttributeWindow = new SBUpdater.Viev.AddAttribute();
                    AddAttributeWindow.DataContext = this;
                    AddAttributeWindow.ShowDialog();
                }, null);
            }
        }

        public ICommand AddAttributeGroup
        {
            get
            {
                return new Command(() =>
                {
                    AddAttributeGroupWindow = new SBUpdater.Viev.AddAttributeGroup();
                    AddAttributeGroupWindow.DataContext = this;
                    AddAttributeGroupWindow.ShowDialog();
                }, null);
            }
        }

        public string AttrGrName
        {
            get
            {
                return _attrGrName;
            }
            set
            {
                _attrGrName = value;
                base.FirePropertyChanged("AttrGrName");
            }
        }

        public string AttrGroupName
        {
            get
            {
                return _attrGroupName;
            }
            set
            {
                _attrGroupName = value;
                CurrentAttrNames = (from x in Attributes
                                    where x.Attribute_Group_Id == AttrGroups.First<AttributeGroup>(l => (l.Name == value)).Id
                                    select x.Name).ToList<string>();
                base.FirePropertyChanged("AttrGroupName");
            }
        }

        public List<string> AttrGroupNames
        {
            get
            {
                return _attrGroupNames;
            }
            set
            {
                _attrGroupNames = value;
                base.FirePropertyChanged("AttrGroupNames");
            }
        }

        public List<AttributeGroup> AttrGroups
        {
            get
            {
                return _attrGroups;
            }
            set
            {
                _attrGroups = value;
                base.FirePropertyChanged("AttrGroups");
            }
        }

        public List<SBUpdater.Models.Attribute> Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
                base.FirePropertyChanged("Attributes");
            }
        }

        public string AttrName
        {
            get
            {
                return _attrName;
            }
            set
            {
                _attrName = value;
                FirePropertyChanged(() => AttrName);
            }
        }

        public List<string> AttrNames
        {
            get
            {
                return _attrNames;
            }
            set
            {
                _attrNames = value;
                base.FirePropertyChanged("AttrNames");
            }
        }

        public void ConfigureAttr(List<Attr> attrs)
        {
            foreach (Attr attr in attrs)
            {
                if (!AttributesConst.ContainsKey(attr.AttrName))
                {
                    CurrentAttrName = attr.AttrName;
                    ConfigureAttrVindow = new SBUpdater.Viev.ConfigureAttr();
                    ConfigureAttrVindow.DataContext = this;
                    ConfigureAttrVindow.ShowDialog();
                }
            }
        }
        public class Attr
        {
            public string AttrName;
            public string Value;
        }

        public ICommand Checked
        {
            get
            {
                return new Command(() =>
                {
                    if (isSaved)
                    {
                        IsSaved = false;
                    }
                    else
                    {
                        IsSaved = true;
                    }
                }, null);
            }
        }

        public ICommand ConfirmCat
        {
            get
            {
                return new Command(() =>
                {
                    CategoryConst.Add(CurrentCatName, Categories.First<Category>(x => (x.Name == ParentCat)).Id);
                    ConfirmCategoryWindow.Close();
                }, null);
            }
        }

        public ICommand Connect
        {
            get
            {
                return new Command(() =>
                {
                    if (IsSaved)
                    {
                        Settings.Default.DataBaseName = DB.DatabaseName;
                        Settings.Default.DatabaseUserId = DB.DatabaseUserId;
                        Settings.Default.DatabasePassword = DB.DatabasePassword;
                        Settings.Default.DatabaseServer = DB.DatabaseServer;
                        Settings.Default.Save();
                    }
                    MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
                    {
                        Server = DB.DatabaseServer,
                        Database = DB.DatabaseName,
                        UserID = DB.DatabaseUserId,
                        Password = DB.DatabasePassword
                    };
                    Connection = new MySqlConnection(builder.ConnectionString);
                    SettingWindow.Close();
                }, null);
            }
        }

        public string CurrentAttrName
        {
            get
            {
                return _currentAttrName;
            }
            set
            {
                _currentAttrName = value;
                base.FirePropertyChanged("CurrentAttrName");
            }
        }

        public List<string> CurrentAttrNames
        {
            get
            {
                return _currentAttrNames;
            }
            set
            {
                IOrderedEnumerable<string> source = from x in value
                                                    orderby x
                                                    select x;
                _currentAttrNames = source.ToList<string>();
                base.FirePropertyChanged("CurrentAttrNames");
            }
        }

        public string CurrentCatName
        {
            get
            {
                return _currentCatName;
            }
            set
            {
                _currentCatName = value;
                base.FirePropertyChanged("CurrentCatName");
            }
        }

        public List<string> CurrentCatNames
        {
            get
            {
                return _currentCatNames;
            }
            set
            {
                IOrderedEnumerable<string> source = from x in value
                                                    orderby x
                                                    select x;
                _currentCatNames = source.ToList<string>();
                base.FirePropertyChanged("CurrentCatNames");
            }
        }

        public ICommand DatabaseConnect
        {
            get
            {
                return new Command(() =>
                {
                    SettingWindow = new SBUpdater.Viev.SettingWindow();
                    SettingWindow.DataContext = this;
                    SettingWindow.ShowDialog();
                }, null);
            }
        }

        public DatabaseConnectModel DB { get; set; }

        public bool IsSaved
        {
            get
            {
                return isSaved;
            }
            set
            {
                if (isSaved != value)
                {
                    isSaved = value;
                }
                FirePropertyChanged(() => IsSaved);
            }
        }

        public string NewAttrGrName
        {
            get
            {
                return _newAttrGrName;
            }
            set
            {
                _newAttrGrName = value;
                base.FirePropertyChanged("NewAttrGrName");
            }
        }

        public string NewAttrName
        {
            get
            {
                return _newAttrName;
            }
            set
            {
                _newAttrName = value;
                base.FirePropertyChanged("NewAttrName");
            }
        }

        public string NewCatName
        {
            get
            {
                return _newCatName;
            }
            set
            {
                _newCatName = value;
                base.FirePropertyChanged("NewCatName");
            }
        }

        public string ParentCat
        {
            get
            {
                return _parentCat;
            }
            set
            {
                _parentCat = value;
                base.FirePropertyChanged("ParentCat");
            }
        }

        public ICommand SaveAttr
        {
            get
            {
                return new Command(() =>
                {
                    AttributesConst.Add(CurrentAttrName, new attrCat(Attributes.First<SBUpdater.Models.Attribute>(x => (x.Name == AttrName)).Id, Attributes.First<SBUpdater.Models.Attribute>(x => (x.Name == AttrName)).Attribute_Group_Id));
                    ConfigureAttrVindow.Close();
                }, null);
            }
        }

        public ICommand SaveNewAttr
        {
            get
            {
                return new Command(() =>
                {
                    using (MySqlConnection connection = Connection)
                    {
                        int num = _attrGroupsDictionary[AttrGrName];
                        string cmdText = "INSERT INTO `oc_attribute`(`attribute_id`, `attribute_group_id`, `sort_order`) VALUES (@attribute_id,@attribute_group_id,0)";
                        MySqlCommand command = new MySqlCommand(cmdText, connection);
                        command.Parameters.Add(new MySqlParameter("@attribute_id", ++LastAttributeId));
                        command.Parameters.Add(new MySqlParameter("@attribute_group_id", num));
                        string str2 = "INSERT INTO `oc_attribute_description`(`attribute_id`, `language_id`, `name`) VALUES (@attribute_id,3,@name)";
                        MySqlCommand command2 = new MySqlCommand(str2, connection);
                        command2.Parameters.Add(new MySqlParameter("@attribute_id", LastAttributeId));
                        command2.Parameters.Add(new MySqlParameter("@name", NewAttrName));
                        connection.Open();
                        command.ExecuteNonQuery();
                        command2.ExecuteNonQuery();
                        connection.Close();
                        SBUpdater.Models.Attribute item = new SBUpdater.Models.Attribute
                        {
                            Attribute_Group_Id = num,
                            Id = LastAttributeId,
                            Name = NewAttrName,
                            Sort_Order = 0
                        };
                        Attributes.Add(item);
                    }
                    AttrNames.Add(NewAttrName);
                    CurrentAttrNames = AttrNames;
                    AddAttributeWindow.Close();
                }, null);
            }
        }

        public ICommand SaveNewAttrGr
        {
            get
            {
                return new Command(() =>
                {
                    using (MySqlConnection connection = Connection)
                    {
                        string cmdText = "INSERT INTO `oc_attribute_group`(`attribute_group_id`, `sort_order`) VALUES (@attrGr_id,0)";
                        MySqlCommand command = new MySqlCommand(cmdText, connection);
                        command.Parameters.Add(new MySqlParameter("@attrGr_id", ++LastAttrGroupId));
                        string str2 = "INSERT INTO `oc_attribute_group_description`(`attribute_group_id`, `language_id`, `name`) \r\nVALUES (@attrGr_id,3,@name)";
                        MySqlCommand command2 = new MySqlCommand(str2, connection);
                        command2.Parameters.Add(new MySqlParameter("@attrGr_id", LastAttrGroupId));
                        command2.Parameters.Add(new MySqlParameter("@name", NewAttrGrName));
                        connection.Open();
                        command.ExecuteNonQuery();
                        command2.ExecuteNonQuery();
                        connection.Close();
                        AttributeGroup item = new AttributeGroup
                        {
                            Id = LastAttrGroupId,
                            Name = NewAttrGrName
                        };
                        AttrGroups.Add(item);
                        AttrGroupNames.Add(NewAttrGrName);
                    }
                }, null);
            }
        }

        public ICommand SaveNewCat
        {
            get
            {
                return new Command(() =>
                {
                    Func<Category, bool> predicate = null;
                    using (MySqlConnection connection = Connection)
                    {
                        Func<Category, bool> func = null;
                        if (predicate == null)
                        {
                            predicate = x => x.Name == ParentCat;
                        }
                        int parentId = Categories.First<Category>(predicate).Id;
                        string str = description.Replace("{0}", NewCatName);
                        string str2 = metaDescription.Replace("{0}", NewCatName);
                        string str3 = metaKeywords.Replace("{0}", NewCatName);
                        string cmdText = "INSERT INTO `oc_category`\r\n(`category_id`, `parent_id`, `column`, `sort_order`, `status`, `date_added`, `date_modified`) \r\nVALUES (@category_id,@parent_id,1,0,true,@date,@date)";
                        MySqlCommand command = new MySqlCommand(cmdText, connection);
                        command.Parameters.Add(new MySqlParameter("@category_id", ++LastCategoryId));
                        command.Parameters.Add(new MySqlParameter("@parent_id", parentId));
                        command.Parameters.Add(new MySqlParameter("@date", DateTime.Now));
                        string str5 = "INSERT INTO `oc_category_description`(`category_id`, `language_id`, `name`, `description`, `meta_description`, `meta_keyword`) \r\nVALUES (@category_id,3,@name,@description,@meta_description,@meta_keyword)";
                        MySqlCommand command2 = new MySqlCommand(str5, connection);
                        command2.Parameters.Add(new MySqlParameter("@category_id", LastCategoryId));
                        command2.Parameters.Add(new MySqlParameter("@name", NewCatName));
                        command2.Parameters.Add(new MySqlParameter("@description", str));
                        command2.Parameters.Add(new MySqlParameter("@meta_description", str2));
                        command2.Parameters.Add(new MySqlParameter("@meta_keyword", str3));
                        string str6 = "INSERT INTO `oc_category_to_store`(`category_id`, `store_id`) VALUES (@category_id,0)";
                        MySqlCommand command3 = new MySqlCommand(str6, connection);
                        command3.Parameters.Add(new MySqlParameter("@category_id", LastCategoryId));
                        connection.Open();
                        command.ExecuteNonQuery();
                        command2.ExecuteNonQuery();
                        command3.ExecuteNonQuery();
                        connection.Close();
                        Category item = new Category
                        {
                            Description = str,
                            Id = LastCategoryId,
                            Language_Id = 3,
                            Meta_Description = str2,
                            Meta_Keywords = str3,
                            Name = NewCatName,
                            Parent_Id = parentId
                        };
                        Categories.Add(item);
                        int num = 1;
                        List<int> list = new List<int> {
                            LastCategoryId
                        };
                        do
                        {
                            list.Add(parentId);
                            if (func == null)
                            {
                                func = x => x.Id == parentId;
                            }
                            parentId = Categories.First<Category>(func).Parent_Id;
                            num++;
                        }
                        while (parentId != 0);
                        foreach (int num2 in list)
                        {
                            string str7 = "INSERT INTO `oc_category_path`(`category_id`, `path_id`, `level`)\r\nVALUES (@category_id,@path_id,@level)";
                            MySqlCommand command4 = new MySqlCommand(str7, connection);
                            command4.Parameters.Add(new MySqlParameter("@category_id", LastCategoryId));
                            command4.Parameters.Add(new MySqlParameter("@path_id", num2));
                            command4.Parameters.Add(new MySqlParameter("@level", --num));
                            connection.Open();
                            command4.ExecuteNonQuery();
                            connection.Close();
                        }
                        UpdateCategoryNames(Categories.Last<Category>());
                        CurrentCatNames = (from x in Categories select x.Name).ToList<string>();
                        AddCategoryWindow.Close();
                    }
                }, null);
            }
        }

        public ICommand UpdateBase
        {
            get
            {
                return new Command(() =>
                {
                    _skus = new List<string>();
                    string query = "SELECT sku\r\nFROM oc_product";
                    Connection.Open();
                    List<string> products = new List<string>();
                    MySqlDataReader reader = LoadFromDb(query);
                    while (reader.Read())
                    {
                        _skus.Add(reader.GetString(0));
                    }
                    Connection.Close();

                    string queryString = "SELECT t1.attribute_group_id, t1.attribute_id, t2.name\r\nFROM oc_attribute t1, oc_attribute_description t2\r\nWHERE t1.attribute_id = t2.attribute_id";
                    Connection.Open();
                    reader = LoadFromDb(queryString);
                    Attributes = new List<SBUpdater.Models.Attribute>();
                    while (reader.Read())
                    {
                        SBUpdater.Models.Attribute item = new SBUpdater.Models.Attribute
                        {
                            Attribute_Group_Id = reader.GetInt32(0),
                            Sort_Order = 0,
                            Id = reader.GetInt32(1),
                            Name = reader.GetString(2)
                        };
                        Attributes.Add(item);
                    }
                    Connection.Close();
                    LastAttributeId = Attributes.Max<SBUpdater.Models.Attribute>((Func<SBUpdater.Models.Attribute, int>)(x => x.Id));
                    AttrNames = (from x in Attributes select x.Name).ToList<string>();
                    CurrentAttrNames = AttrNames;
                    string str2 = "SELECT t1.category_id, t1.parent_id, t2.description, t2.meta_description, t2.meta_keyword, t2.name\r\nFROM oc_category t1, oc_category_description t2\r\nWHERE t1.category_id = t2.category_id";
                    Connection.Open();
                    reader = LoadFromDb(str2);
                    Categories = new List<Category>();
                    while (reader.Read())
                    {
                        Category category = new Category
                        {
                            Id = reader.GetInt32(0),
                            Parent_Id = reader.GetInt32(1),
                            Description = reader.GetString(2),
                            Meta_Description = reader.GetString(3),
                            Meta_Keywords = reader.GetString(4),
                            Language_Id = 3,
                            Name = reader.GetString(5)
                        };
                        Categories.Add(category);
                    }
                    Connection.Close();
                    UpdateCategoryNames();
                    LastCategoryId = Categories.Max<Category>((Func<Category, int>)(x => x.Id));
                    CurrentCatNames = (from x in Categories select x.Name).ToList<string>();
                    string str3 = "SELECT manufacturer_id, name, image\r\nFROM oc_manufacturer";
                    Connection.Open();
                    reader = LoadFromDb(str3);
                    Manufactures = new List<Manufacturer>();
                    while (reader.Read())
                    {
                        Manufacturer manufacturer = new Manufacturer
                        {
                            Id = reader.GetInt32(0),
                            Image = reader.GetString(2),
                            Name = reader.GetString(1),
                            Sort_Order = 0
                        };
                        Manufactures.Add(manufacturer);
                    }
                    Connection.Close();
                    string str4 = "SELECT t1.attribute_group_id, t2.name\r\nFROM oc_attribute_group t1, oc_attribute_group_description t2\r\nWHERE t1.attribute_group_id = t2.attribute_group_id";
                    Connection.Open();
                    AttrGroups = new List<AttributeGroup>();
                    reader = LoadFromDb(str4);
                    while (reader.Read())
                    {
                        AttributeGroup group = new AttributeGroup
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };
                        AttrGroups.Add(group);
                    }
                    Connection.Close();
                    LastAttrGroupId = AttrGroups.Max<AttributeGroup>((Func<AttributeGroup, int>)(x => x.Id));
                    AttrGroupNames = (from x in AttrGroups select x.Name).ToList<string>();
                    _attrGroupsDictionary = AttrGroups.ToDictionary<AttributeGroup, string, int>(x => x.Name, x => x.Id);
                    ReadOfFile();
                    ReadCategoryes();
                    string cmdText = "SELECT MAX(product_id) FROM oc_product";
                    MySqlCommand command = new MySqlCommand(cmdText, Connection);
                    Connection.Open();
                    reader = command.ExecuteReader();
                    reader.Read();
                    LastProductId = reader.GetInt32(0);
                    Connection.Close();
                    MessageBox.Show("Обновление данных успешно завершено)");
                }, null);
            }
        }

        
        #endregion

        #region Дополнительные функции

        public Dictionary<string, attrCat> AttributesConst = new Dictionary<string, attrCat>();
        public Encoding startEncoding = Encoding.GetEncoding(0x4e3);
        public Encoding endEncoding = Encoding.UTF8;
        public static readonly string metaDescription = "{0} по лучшей цене. Заходите, у нас отличный выбор инструментов, бесплатная доставка по Воронежу";
        public static readonly string metaKeywords = "Купить {0} по лучшей цене, с бесплатной доставкой в Воронеже";
        public class attrCat
        {
            public int attrId;
            public int groupId;

            public attrCat(int _attrId, int _groupId)
            {
                attrId = _attrId;
                groupId = _groupId;
            }

            public override string ToString()
            {
                return (attrId + "|" + groupId);
            }
        }

        public void ReadOfFile()
        {
            string[] strArray = System.IO.File.ReadAllText("attributes.txt").Split(new char[] { '`' });
            foreach (string str2 in strArray)
            {
                if (!(str2 == ""))
                {
                    string[] strArray2 = str2.Split(new char[] { '|' });
                    AttributesConst.Add(strArray2[0], new attrCat(int.Parse(strArray2[1]), int.Parse(strArray2[2])));
                }
            }
        }
        #endregion

    }
}
