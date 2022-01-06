using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpFileUpload.Models
{
    public class CustomResponseBody
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public JsonData[] Data { get; set; }
    }
}
