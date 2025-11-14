using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Core.Service.GenericModels
{
    public class ApiBaseResponse
    {
        public string message { get; set; }
        public string status { get; set; }
        public int status_code { get; set; }
        public bool is_success { get; set; }
    }
}
