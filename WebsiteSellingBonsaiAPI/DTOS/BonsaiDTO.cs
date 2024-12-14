using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.DTOS
{
    public class BonsaiDTO
    {
        [Key]
        public int Id { get; set; }
        [Column("Name")]
        public string BonsaiName { get; set; }
        public string Description { get; set; }
        public string FengShuiMeaning { get; set; }
        public int Size { get; set; }
        [Range(1, 100)]
        public int YearOld { get; set; }
        public int MinLife { get; set; }
        public int MaxLife { get; set; }
        public Decimal Price { get; set; }
        public int Quantity { get; set; }

        public IFormFile? Image { get; set; }
        public string? ImageOld { get; set; }
        public double? nopwr { get; set; } = 0;
        public int? rates { get; set; } = 0;

        public int TypeId { get; set; }
        [ForeignKey("TypeId")]
        public BonsaiType? Type { get; set; }

        public int StyleId { get; set; }
        [ForeignKey("StyleId")]
        public Style? Style { get; set; }

        public int GeneralMeaningId { get; set; }
        [ForeignKey("GeneralMeaningId")]
        public GeneralMeaning? GeneralMeaning { get; set; }
}
}
