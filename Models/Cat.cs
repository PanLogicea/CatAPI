using CatAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatAPI.Models
{
    public class Cat
    {
        public Cat()
        {
            CatTags = new List<CatTag>();
            CatId = string.Empty;
            Image = Array.Empty<byte>();
        }

        [Key]
        [Required]
        public int Id { get; set; }
        public string CatId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Image { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public ICollection<CatTag> CatTags { get; set; }
    }
}
