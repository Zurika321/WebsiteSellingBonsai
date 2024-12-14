using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Models
{
    [Table("Types")]
    public class BonsaiType
    {
        [Key]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        public ICollection<Bonsai> Bonsais { get; set; }
    }
}
