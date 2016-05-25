using SBUpdater.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SBUpdater.Manufacturers
{
    interface Parser
    {
        void ParseProduct(URLs url);
        void ParseProductLinks(URLs url);
        ICommand UpdateCategoryLinks
        {
            get;
        }
        ICommand UpdatePrice
        {
            get;
        }
    }
}
