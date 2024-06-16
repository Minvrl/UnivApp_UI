namespace Univ.UI.Models
{
    public class StudentCreateRequest
    {
        public string Fullname { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public int GroupId { get; set; }
        public IFormFile? File { get; set; }
    }
}
