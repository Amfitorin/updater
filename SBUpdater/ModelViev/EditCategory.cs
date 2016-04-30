using SBUpdater.Models;
using SBUpdater.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBUpdater.ModelViev
{
    public class EditCategory
    {
        public DatabaseConnectModel DB = new DatabaseConnectModel();

        EditCategory()
        {
            DB.DatabaseName = Settings.Default.DataBaseName ?? "";
            DB.DatabasePassword = Settings.Default.DatabasePassword ?? "";
            DB.DatabaseUserId = Settings.Default.DatabaseUserId ?? "";
            DB.DatabaseServer = Settings.Default.DatabaseServer ?? "";
        }
    }
}
