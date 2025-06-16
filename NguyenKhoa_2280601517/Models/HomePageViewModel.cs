namespace NguyenKhoa_2280601517.Models
{
    public class HomePageViewModel
    {
        public List<Product> FeaturedProducts { get; set; }
        public List<Category> FeaturedCategories { get; set; }
        public List<Category> Category { get; set; } // Added this property to fix CS0117  
    }
}
