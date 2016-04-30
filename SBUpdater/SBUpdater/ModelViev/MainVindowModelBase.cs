using SBUpdater.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using SBUpdater.Models;
using System.Windows;
using HtmlAgilityPack;
using System.Net;
using System.Collections.ObjectModel;
using System.IO;

namespace SBUpdater.ModelViev
{
    public class MainWindowModelBase : ModelBase
    {
        public DatabaseConnectModel DB { get; set; }

        Context.DataContext Db = new Context.DataContext();
        Encoding StartEncoding = Encoding.GetEncoding(1251);
        Encoding EndEncoding = Encoding.UTF8;

        //  MySqlConnection connection;
        private List<Models.Attribute> _attributes;
        public List<Models.Attribute> Attributes
        {
            get { return _attributes; }
            set
            {
                _attributes = value;
                FirePropertyChanged("Attributes");
            }
        }
        private Dictionary<string, int> _attrGroupsDictionary;
        private List<Category> _categories;
        public List<Category> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                FirePropertyChanged("Categories");
            }
        }
        public List<Manufacturer> Manufactures;
        private List<AttributeGroup> _attrGroups;
        public List<AttributeGroup> AttrGroups
        {
            get { return _attrGroups; }
            set
            {
                _attrGroups = value;
                FirePropertyChanged("AttrGroups");
            }
        }
        public List<Tools> Products = new List<Tools>();
        public List<URLs> CategoryUrls = new List<URLs>();
        public List<URLs> ProductUrls = new List<URLs>();

        Viev.AddAttribute AddAttributeWindow;
        Viev.AddAttributeGroup AddAttributeGroupWindow;
        Viev.AddCategory AddCategoryWindow;
        Viev.ConfigureAttr ConfigureAttrVindow;
        Viev.ConfirmCategory ConfirmCategoryWindow;
        public MainWindowModelBase()
        {

            DB = new DatabaseConnectModel();
            DB.DatabaseName = Properties.Settings.Default.DataBaseName ?? "";
            DB.DatabasePassword = Properties.Settings.Default.DatabasePassword ?? "";
            DB.DatabaseUserId = Properties.Settings.Default.DatabaseUserId ?? "";
            DB.DatabaseServer = Properties.Settings.Default.DatabaseServer ?? "";
        }
        bool isSaved = false;
        public bool IsSaved
        {
            get { return isSaved; }
            set
            {
                if (isSaved != value)
                    isSaved = value;
                FirePropertyChanged(() => IsSaved);
            }
        }
        public ICommand DatabaseConnect
        {
            get
            {
                return new Command(() =>
                {
                    var sett = new Viev.SettingWindow();
                    sett.DataContext = this;
                    var win = new Window();
                    win = sett;
                    win.ShowDialog();
                });
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
                        Properties.Settings.Default.DataBaseName = DB.DatabaseName;
                        Properties.Settings.Default.DatabaseUserId = DB.DatabaseUserId;
                        Properties.Settings.Default.DatabasePassword = DB.DatabasePassword;
                        Properties.Settings.Default.DatabaseServer = DB.DatabaseServer;
                    }
                    var builder = new MySqlConnectionStringBuilder();
                    builder.Server = DB.DatabaseServer;
                    builder.Database = DB.DatabaseName;
                    builder.UserID = DB.DatabaseUserId;
                    builder.Password = DB.DatabasePassword;

                    //connection = new MySqlConnection(builder.ConnectionString);
                    //connection.Open();
                    Db = new Context.DataContext();
                    var atrr = Db.OcAttributes.ToArray();
                    var s = atrr;
                });
            }
        }
        public ICommand UpdateBase
        {
            get
            {
                return new Command(() =>
                {

                    Attributes = (
                        from attr in Db.OcAttributes
                        from attrNames in Db.OcAttributeDescriptions
                        where attr.AttributeId == attrNames.AttributeId
                        select new Models.Attribute
                        {
                            Attribute_Group_Id = attr.AttributeGroupId,
                            Id = attr.AttributeId,
                            Name = EndEncoding.GetString(StartEncoding.GetBytes(attrNames.Name)),
                            Sort_Order = 0,
                        }).ToList();
                    AttrNames = Attributes.Select(x => x.Name).ToList();
                    CurrentAttrNames = AttrNames;
                    Categories = (
                        from cat in Db.OcCategories
                        from catDescr in Db.OcCategoryDescriptions
                        where cat.CategoryId == catDescr.CategoryId
                        select new Category
                        {
                            Description = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.Description)),
                            Id = cat.CategoryId,
                            Meta_Description = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.MetaDescription)),
                            Meta_Keywords = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.MetaKeyword)),
                            Name = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.Name)),
                            Parent_Id = cat.ParentId
                        }).ToList();
                    UpdateCategoryNames();
                    CurrentCatNames = Categories.Select(x => x.Name).ToList();
                    //MenuNames = Categories.Where(x => x.Parent_Id == 0).ToDictionary(x => x.Name, x => x.Id);
                    Manufactures =
                        (from manuf in Db.OcManufacturers
                         select new Manufacturer
                         {
                             Id = manuf.ManufacturerId,
                             Image = manuf.Image,
                             Name = manuf.Name,
                             Sort_Order = manuf.SortOrder
                         }).ToList();
                    AttrGroups =
                        (from attrGroups in Db.OcAttributeGroups
                         from attrGrpDescr in Db.OcAttributeGroupDescriptions
                         where attrGroups.AttributeGroupId == attrGrpDescr.AttributeGroupId
                         select new AttributeGroup
                         {
                             Id = attrGroups.AttributeGroupId,
                             Name = EndEncoding.GetString(StartEncoding.GetBytes(attrGrpDescr.Name))
                         }).ToList();
                    AttrGroupNames = AttrGroups.Select(x => x.Name).ToList();
                    _attrGroupsDictionary = Db.OcAttributeGroupDescriptions.ToDictionary(x => EndEncoding.GetString(StartEncoding.GetBytes(x.Name)), x => x.AttributeGroupId);
                    ReadOfFile();
                    ReadCategoryes();
                });
            }
        }

        private void UpdateCategoryNames()
        {
            foreach (var item in Categories)
            {
                //var parent_id = item.Parent_Id;
                //while (parent_id != 0)
                //{
                //    item.Name = Categories.First(x => x.Id == parent_id).Name + "/" + item.Name;
                //    var parent = Categories.First(x => x.Id == parent_id);
                //    parent_id = parent.Parent_Id;
                //}
                if (item.Parent_Id != 0)
                    item.Name = Categories.First(x => x.Id == item.Parent_Id).Name + "/" + item.Name;
            }
        }
        public ICommand UpdateLinks
        {
            get
            {
                return new Command(() =>
                {
                    CategoryUrls.Add(new URLs
                    {
                        CategoryName = "",
                        Url = _link + "%D0%B4%D0%BE%D0%BC%D0%B0%D1%88%D0%BD%D0%B8%D0%B5-%D0%BC%D0%B0%D1%81%D1%82%D0%B5%D1%80%D1%81%D0%BA%D0%B8%D0%B5-%D0%BF%D1%80%D0%BE%D0%BC%D1%8B%D1%88%D0%BB%D0%B5%D0%BD%D0%BD%D0%BE%D0%B5-%D0%BF%D1%80%D0%BE%D0%B8%D0%B7%D0%B2%D0%BE%D0%B4%D1%81%D1%82%D0%B2%D0%BE-101271-ocs-c/",
                    });
                    do{
                        ParseLinks(CategoryUrls.First());
                        CategoryUrls.RemoveAt(0);
                    }
                    while (CategoryUrls.Count > 0);
                    var products = Db.OcProducts.Select(x => x.Model).ToArray();
                    ProductUrls = ProductUrls.Where(x => !products.Contains(x.ProductName)).ToList();
                    do
                    {
                        ParseProducts(ProductUrls.First());
                        ProductUrls.RemoveAt(0);
                    }
                    while (ProductUrls.Count > 0);

                    var s = 0;
                });
            }
        }
        class Attr
        {
            public string AttrName;
            public string Value;
        }
        private void ParseProducts(URLs url)
        {
            var html = new HtmlDocument();
            var wClient = new WebClient();
            var startEncoding = Encoding.GetEncoding(1251);
            var endEncoding = Encoding.UTF8;

            html.LoadHtml(endEncoding.GetString(startEncoding.GetBytes(wClient.DownloadString(url.Url))));
            var doc = html.DocumentNode;
            var image = doc.SelectNodes("//img[@itemprop='image' and @class='stageProd']").First().GetAttributeValue("src", "");
            var rows = doc.SelectNodes("//table[@class='techDetails']")
                .First().ChildNodes.Where(x => x.Name == "tbody").ToArray()
                .SelectMany(x => x.ChildNodes.Where(l => l.Name == "tr").ToArray())
                .ToArray();
            var attributes = new List<Attr>();
            foreach (var row in rows)
            {
                var columns = row.ChildNodes.Where(x => x.Name == "td").ToArray();
                var attrName = columns[0].InnerText;
                var value = columns[1].InnerText;

                attributes.Add(new Attr
                {
                    AttrName = attrName,
                    Value = value
                });
            }
            var descr = (doc.SelectNodes("//h6[@itemprop='description']").First().OuterHtml +
                    doc.SelectNodes("//ul")[12].OuterHtml)
                    .Replace("<", "&lt;").Replace(">", "&gt;");
            var sku = doc.SelectNodes("//th[@class='hook']").First().InnerText;
            ConfigureAttr(attributes);
            WriteOnFile();
            ConfirmCategory(url.CategoryName);
            WriteCategoryes();
            var productAttributes = new List<Models.Attribute>();
            foreach (var item in attributes)
            {
                productAttributes.Add(new Models.Attribute
                {
                    Attribute_Group_Id = AttributesConst[item.AttrName].groupId,
                    Id = AttributesConst[item.AttrName].attrId,
                    Name = Attributes.First(x => x.Id == AttributesConst[item.AttrName].attrId).Name,
                    Sort_Order = 0,
                    Value = item.Value
                });
            }
            Products.Add(new Tools
            {
                Attributes = productAttributes,
                CategoryName = url.CategoryName,
                Description = new ProductDescription
                {
                    Description = descr,
                    Meta_Description = metaDescription.Replace("{0}", "Bosch " + url.ProductName),
                    Meta_Keyword = metaKeywords.Replace("{0}", "Bosch " + url.ProductName),
                },
                Height=1,
                Image = image,
                Length = 1,
                Manufacturer_id = 11,
                Model = url.ProductName,
                Price = 1,
                Sku = sku,
                Url = url.Url,
                Weight = 1,
                Width = 1
            });
        }
        class attrCat
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
                return attrId + "|" + groupId;
            }
        }
        private Dictionary<string, attrCat> AttributesConst = new Dictionary<string, attrCat>();
        private Dictionary<string, int> CategoryConst = new Dictionary<string, int>();
        public ICommand Checked
        {
            get
            {
                return new Command(() =>
                {
                    if (isSaved == true)
                        IsSaved = false;
                    else
                        IsSaved = true;
                });
            }
        }
        private void ParseLinks(URLs url)
        {
            var html = new HtmlDocument();
            var wClient = new WebClient();
            var startEncoding = Encoding.GetEncoding(1251);
            var endEncoding = Encoding.UTF8;

            html.LoadHtml(endEncoding.GetString(startEncoding.GetBytes(wClient.DownloadString(url.Url))));

            var categories = html.DocumentNode.SelectNodes("//div[@class='floatBox' or @class='floatBox last']");
            var products = html.DocumentNode.SelectNodes("//div[@class='conCent jsLink']");

            if (categories != null)
                foreach (var node in categories)
                {
                    var link = node.ChildNodes.Where(x => x.Name == "div").First().ChildNodes.Where(x => x.Name == "a").First();
                    var title = link.GetAttributeValue("title", "");
                    var catLink = link.GetAttributeValue("href", "");
                    if (title == "Новинки")
                        continue;
                    CategoryUrls.Add(new URLs
                    {
                        CategoryName = url.CategoryName + "/" + title,
                        Url = _link + catLink,
                    });
                }
            if (products != null)
                foreach (var node in products)
                {
                    var link = node.ChildNodes.Where(x => x.Name == "a").First();
                    var title = node.GetAttributeValue("title", "");
                    title = title.Remove(title.Length - 13);
                    var catLink = link.GetAttributeValue("href", "");
                    ProductUrls.Add(new URLs
                    {
                        CategoryName = url.CategoryName,
                        ProductName = title,
                        Url = _link + catLink,
                    });
                }
        }

        private void WriteOnFile()
        {
            var text = "";
            foreach (var item in AttributesConst)
            {
                text += item.Key + "|" + item.Value.ToString() + "#";
            }

            File.WriteAllText("attributes.txt", text);
        }

        private void ReadOfFile()
        {
            var text = File.ReadAllText("attributes.txt");
            var array = text.Split('#');
            foreach (var item in array)
            {
                if (item == "")
                    continue;
                var attr = item.Split('|');
                AttributesConst.Add(attr[0], new attrCat(int.Parse(attr[1]), int.Parse(attr[2])));
            }
        }

        private void WriteCategoryes()
        {
            var text = "";
            foreach (var item in CategoryConst)
            {
                text += item.Key + "|" + item.Value.ToString() + "#";
            }

            File.WriteAllText("categoryes.txt", text);
        }

        private void ReadCategoryes()
        {
            var text = File.ReadAllText("categoryes.txt");
            var array = text.Split('#');
            foreach (var item in array)
            {
                if (item == "")
                    continue;
                var attr = item.Split('|');
                CategoryConst.Add(attr[0], int.Parse(attr[1]));
            }
        }


        #region SaveAttributes
        private string _currentAttrName;
        private void ConfigureAttr(List<Attr> attrs)
        {
            foreach (var attr in attrs)
            {
                if (AttributesConst.ContainsKey(attr.AttrName))
                    continue;
                CurrentAttrName = attr.AttrName;
                ConfigureAttrVindow = new Viev.ConfigureAttr();
                ConfigureAttrVindow.DataContext = this;
                ConfigureAttrVindow.ShowDialog();
            }
        }
        public ICommand AddAttribute
        {
            get
            {
                return new Command(() =>
                {
                    AddAttributeWindow = new Viev.AddAttribute();
                    AddAttributeWindow.DataContext = this;
                    AddAttributeWindow.ShowDialog();
                });
            }
        }
        private string _attrName = "";
        public string AttrName
        {
            get { return _attrName; }
            set
            {
                _attrName = value;
                FirePropertyChanged(() => AttrName);
            }
        }
        private string _newAttrName = "";
        public string NewAttrName
        {
            get { return _newAttrName; }
            set
            {
                _newAttrName = value;
                FirePropertyChanged("NewAttrName");
            }
        }
        private List<string> _attrNames = new List<string>();
        public List<string> AttrNames
        {
            get { return _attrNames; }
            set
            {
                _attrNames = value;
                FirePropertyChanged("AttrNames");
            }
        }
        private List<string> _currentAttrNames = new List<string>();
        public List<string> CurrentAttrNames
        {
            get { return _currentAttrNames; }
            set
            {
                var val = value.OrderBy(x => x);
                _currentAttrNames = val.ToList();
                FirePropertyChanged("CurrentAttrNames");
            }
        }
        public string CurrentAttrName
        {
            get { return _currentAttrName; }
            set
            {
                _currentAttrName = value;
                FirePropertyChanged("CurrentAttrName");
            }
        }
        private string _currentCatName;
        public ICommand SaveNewAttr
        {
            get
            {
                return new Command(() =>
                {
                    var database = new Context.DataContext();
                    var attrGrId = _attrGroupsDictionary[AttrGrName];
                    var lastAttrId = database.OcAttributes.Max(x => x.AttributeId);
                    var attrName = StartEncoding.GetString(EndEncoding.GetBytes(NewAttrName));
                    database.OcAttributes.InsertOnSubmit(new Context.OcAttribute { SortOrder = 0, AttributeGroupId = attrGrId });
                    database.OcAttributeDescriptions.InsertOnSubmit(new Context.OcAttributeDescription { LanguageId = 3, Name = attrName, AttributeId = ++lastAttrId });
                    database.SubmitChanges();
                    Attributes = (
                         from attr in Db.OcAttributes
                         from attrNames in Db.OcAttributeDescriptions
                         where attr.AttributeId == attrNames.AttributeId
                         select new Models.Attribute
                         {
                             Attribute_Group_Id = attr.AttributeGroupId,
                             Id = attr.AttributeId,
                             Name = EndEncoding.GetString(StartEncoding.GetBytes(attrNames.Name)),
                             Sort_Order = 0,
                         }).ToList();
                    AttrNames = Attributes.Select(x => x.Name).ToList();
                    CurrentAttrNames = AttrNames;
                    AddAttributeWindow.Close();
                });
            }
        }
        public ICommand SaveAttr
        {
            get
            {
                return new Command(() =>
                {
                    AttributesConst.Add(CurrentAttrName, new attrCat(Attributes.First(x => x.Name == AttrName).Id, Attributes.First(x => x.Name == AttrName).Attribute_Group_Id));
                    ConfigureAttrVindow.Close();
                });
            }
        }
        #endregion

        #region SaveAttributeGroup
        public ICommand AddAttributeGroup
        {
            get
            {
                return new Command(() =>
                {
                    AddAttributeGroupWindow = new Viev.AddAttributeGroup();
                    AddAttributeGroupWindow.DataContext = this;
                    AddAttributeGroupWindow.ShowDialog();
                });
            }
        }
        private string _attrGroupName = "";
        public string AttrGroupName
        {
            get { return _attrGroupName; }
            set
            {
                _attrGroupName = value;
                CurrentAttrNames = Attributes.Where(x => x.Attribute_Group_Id == AttrGroups.First(l => l.Name == value).Id).Select(x => x.Name).ToList();
                FirePropertyChanged("AttrGroupName");
            }
        }
        private string _attrGrName = "";
        public string AttrGrName
        {
            get { return _attrGrName; }
            set
            {
                _attrGrName = value;
                FirePropertyChanged("AttrGrName");
            }
        }
        private string _newAttrGrName = "";
        public string NewAttrGrName
        {
            get { return _newAttrGrName; }
            set
            {
                _newAttrGrName = value;
                FirePropertyChanged("NewAttrGrName");
            }
        }
        private List<string> _attrGroupNames = new List<string>();
        public List<string> AttrGroupNames
        {
            get { return _attrGroupNames; }
            set
            {
                _attrGroupNames = value;
                FirePropertyChanged("AttrGroupNames");
            }
        }
        public ICommand SaveNewAttrGr
        {
            get
            {
                return new Command(() =>
                {
                    var attrGrId = Db.OcAttributeGroupDescriptions.ToArray().First(x => EndEncoding.GetString(StartEncoding.GetBytes(x.Name)) == AttrGrName).AttributeGroupId;
                    Db.OcAttributeGroupDescriptions.InsertOnSubmit(new Context.OcAttributeGroupDescription { LanguageId = 3, Name = NewAttrGrName });
                    Db.SubmitChanges();
                    AttrGroups =
                       (from attrGroups in Db.OcAttributeGroups
                        from attrGrpDescr in Db.OcAttributeGroupDescriptions
                        where attrGroups.AttributeGroupId == attrGrpDescr.AttributeGroupId
                        select new AttributeGroup
                        {
                            Id = attrGroups.AttributeGroupId,
                            Name = EndEncoding.GetString(StartEncoding.GetBytes(attrGrpDescr.Name))
                        }).ToList();
                    AttrGroupNames = AttrGroups.Select(x => x.Name).ToList();
                });
            }
        }
        #endregion

        #region SaveCategories
        private string _newCatName = "";
        public string NewCatName
        {
            get { return _newCatName; }
            set
            {
                _newCatName = value;
                FirePropertyChanged("NewCatName");
            }
        }
        private string _parentCat = "";
        public string ParentCat
        {
            get { return _parentCat; }
            set
            {
                _parentCat = value;
                FirePropertyChanged("ParentCat");
            }
        }
        private List<string> _categoryNames = new List<string>();
        public List<string> CategoryNames
        {
            get { return _categoryNames; }
            set
            {
                _categoryNames = value;
                FirePropertyChanged("CategoryNames");
            }
        }
        private List<string> _currentCatNames = new List<string>();
        public List<string> CurrentCatNames
        {
            get { return _currentCatNames; }
            set
            {
                var s = value.OrderBy(x => x);
                _currentCatNames = s.ToList();
                FirePropertyChanged("CurrentCatNames");
            }
        }
        public ICommand ConfirmCat
        {
            get
            {
                return new Command(() =>
                {
                    CategoryConst.Add(CurrentCatName, Categories.First(x => x.Name == ParentCat).Id);
                    ConfirmCategoryWindow.Close();
                });
            }
        }
        public ICommand SaveNewCat
        {
            get
            {
                return new Command(() =>
                {
                    var parentId = Categories.First(x => x.Name == ParentCat).Id;
                    var lastCatId = Db.OcCategories.Max(x => x.CategoryId);
                    var catName = StartEncoding.GetString(EndEncoding.GetBytes(NewCatName));
                    Db.OcCategories.InsertOnSubmit(new Context.OcCategory { SortOrder = 0, ParentId = parentId, Column = 1, Status = true, DateAdded = DateTime.Now, DateModified = DateTime.Now });
                    Db.OcCategoryDescriptions.InsertOnSubmit(new Context.OcCategoryDescription
                    {
                        LanguageId = 3,
                        Name = catName,
                        CategoryId = ++lastCatId,
                        Description = description.Replace("{0}", catName),
                        MetaDescription = metaDescription.Replace("{0}", catName),
                        MetaKeyword = metaKeywords.Replace("{0}", catName)
                    });
                    Db.SubmitChanges();
                    Categories = (
                        from cat in Db.OcCategories
                        from catDescr in Db.OcCategoryDescriptions
                        where cat.CategoryId == catDescr.CategoryId
                        select new Category
                        {
                            Description = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.Description)),
                            Id = cat.CategoryId,
                            Meta_Description = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.MetaDescription)),
                            Meta_Keywords = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.MetaKeyword)),
                            Name = EndEncoding.GetString(StartEncoding.GetBytes(catDescr.Name)),
                            Parent_Id = cat.ParentId
                        }).ToList();
                    UpdateCategoryNames();
                    CurrentCatNames = Categories.Select(x => x.Name).ToList();
                    AddCategoryWindow.Close();
                });
            }
        }
        public ICommand AddCategory
        {
            get
            {
                return new Command(() =>
                {
                    AddCategoryWindow = new Viev.AddCategory();
                    AddCategoryWindow.DataContext = this;
                    AddCategoryWindow.ShowDialog();
                });
            }
        }
        public string CurrentCatName
        {
            get { return _currentCatName; }
            set
            {
                _currentCatName = value;
                FirePropertyChanged("CurrentCatName");
            }
        }
        private void ConfirmCategory(string cat)
        {
            if (CategoryConst.ContainsKey(cat))
                return;
            CurrentCatName = cat;
            ConfirmCategoryWindow = new Viev.ConfirmCategory();
            ConfirmCategoryWindow.DataContext = this;
            ConfirmCategoryWindow.ShowDialog();
        }
        #endregion

        private static readonly string metaKeywords = "Купить {0} по лучшей цене, с бесплатной доставкой в Воронеже";
        private static readonly string metaDescription = "{0} по лучшей цене. Заходите, у нас отличный выбор инструментов, бесплатная доставка по Воронежу";
        private static readonly string description = "Купить {0} по лучшим ценам.";

        private readonly string _link = "http://www.bosch-professional.com/ru/ru/";
    }
}
