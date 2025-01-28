namespace CatAPI.Models
{
    public class CatDto
    {
        public int Id { get; set; }
        public required string CatId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public required byte[] Image { get; set; }
        public DateTime Created { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
