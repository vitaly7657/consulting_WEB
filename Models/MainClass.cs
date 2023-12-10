using m21_e2_WEB.ViewModels;

namespace m21_e2_WEB.Models
{
    public class MainClass
    {
        public SiteText? siteText { get; set; }

        public Request? request { get; set; }
        public List<Request>? requestsList { get; set; }
        public RequestViewModel? requestViewModel { get; set; }        

        public Project? project { get; set; }
        public IEnumerable<Project>? projectsList { get; set; }
        public ProjectViewModel? projectViewModel { get; set; }

        public Service? service { get; set; }
        public IEnumerable<Service>? servicesList { get; set; }
        public ServiceViewModel? serviceViewModel { get; set; }

        public Blog? blog { get; set; }
        public IEnumerable<Blog>? blogList { get; set; }
        public BlogViewModel? blogViewModel { get; set; }

        public Contact? contact { get; set; }
        public List<Contact>? contactsList { get; set; }
        public ContactViewModel? contactViewModel { get; set; }

        public TagLine? tagLine { get; set; }
        public List<TagLine>? tagLineList { get; set; }        
        public TagLineViewModel tagLineViewModel { get; set; }

        public RequestPeriod? requestPeriod { get; set; }

        public LoginViewModel loginViewModel { get; set; }
        public RegisterViewModel registerViewModel { get; set; }

    }
}
