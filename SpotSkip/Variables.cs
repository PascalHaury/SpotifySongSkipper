using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotSkip
{
    class Variables
    {
        public readonly string BlockListFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\BlockListV2.xml";
        public readonly string ErrorLogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\" + DateTime.Today.ToShortDateString() + "ErrorLog.log";
    }
}
