using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampusModel.MarketPlace
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        public string Code { get; set; }

        public int Visible { get; set; }

        public ICollection<Product> Categories { get; set; }
    }
}
