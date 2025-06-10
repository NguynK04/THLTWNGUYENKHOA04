using System.ComponentModel.DataAnnotations;

namespace NguyenKhoa_Lab03.Models
{
    public class Product
    {
            public int Id { get; set; }
            [Required, StringLength(100)]
            public string Name { get; set; }
            [Range(0.01, 1000000.00)]
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string? ImageUrl { get; set; }
            public List<ProductImage?> Images { get; set; } = new();
            public int? CategoryId { get; set; }
            public Category? Category { get; set; }
        
    }
}
