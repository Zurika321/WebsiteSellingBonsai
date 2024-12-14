using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Styles")]
    public class Style
    {
        [Key]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        public ICollection<Bonsai> Bonsais { get; set; }
    }
}
