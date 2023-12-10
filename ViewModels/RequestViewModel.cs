using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace m21_e2_WEB.ViewModels
{
    public class RequestViewModel
    {
        public string RequesterName { get; set; }
        public string RequestEmail { get; set; }
        public string RequestText { get; set; }
    }
}
