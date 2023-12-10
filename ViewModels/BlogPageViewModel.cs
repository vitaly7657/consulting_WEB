using m21_e2_WEB.Models;

namespace m21_e2_WEB.ViewModels
{
    public class BlogPageViewModel
    {
        public IEnumerable<Blog?> blog { get; set; }
        public SiteText? siteText { get; set; }

    }
}
