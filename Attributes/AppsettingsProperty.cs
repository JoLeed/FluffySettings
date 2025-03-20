using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffySettings.Attributes
{
    public class AppsettingsProperty : Attribute
    {
        public string PropertyName { get; set; } = "";
    }
}
