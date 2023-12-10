namespace m21_e2_WEB.ViewModels
{
    public class ContactViewModel
    {
        public int? Id { get; set; }
        public string ContactText { get; set; }
        public string ContactLink { get; set; }
        public IFormFile? PictureFile { get; set; }
    }
}
