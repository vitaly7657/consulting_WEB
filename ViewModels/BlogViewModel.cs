namespace m21_e2_WEB.ViewModels
{
    public class BlogViewModel
    {
        public int? Id { get; set; }
        public string BlogTitle { get; set; }
        public string BlogDescription { get; set; }
        public IFormFile? PictureFile { get; set; }
    }
}
