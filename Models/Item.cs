using System.ComponentModel.DataAnnotations;

namespace CRUD_API.Models
{
    public class Item
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public Product? Product { get; set; }
    }
}
