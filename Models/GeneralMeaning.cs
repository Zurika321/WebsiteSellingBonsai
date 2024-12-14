using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsai.Models
{
    [Table("GeneralMeanings")]
    public class GeneralMeaning
    {
        [Key]
        public int Id { get; set; }

        [Column("Meaning")]
        public string Meaning { get; set; }

        public ICollection<Bonsai> Bonsais { get; set; }
    }
}
