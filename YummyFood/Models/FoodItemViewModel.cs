using System.ComponentModel.DataAnnotations;

namespace YummyFood.Models
{
    public class FoodItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Item type is required")]
        public string ItemType { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
        public string ItemName { get; set; }

        public string ItemImage { get; set; }

        [Required(ErrorMessage = "Item details are required")]
        [StringLength(500, ErrorMessage = "Details cannot exceed 500 characters")]
        public string ItemDetails { get; set; }

        [Required(ErrorMessage = "Item price is required")]
        public string ItemPrice { get; set; }
    }
}
