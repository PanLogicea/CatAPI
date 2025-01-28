using CatAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatAPI.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        [Required]
        public ICollection<CatTag> CatTags { get; set; } = new List<CatTag>();
    }
}

public class CatTag
{
    public int CatId { get; set; }
    public required Cat Cat { get; set; } 

    public int TagId { get; set; }
    public required Tag Tag { get; set; }
}
