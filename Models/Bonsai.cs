﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteSellingBonsai.Models
{
    public class Bonsai
    {
        [Key]
        public int Id { get; set; } // Id khớp với cột Id trong DB

        public string BonsaiName { get; set; } // Tên ánh xạ tới cột Name

        public string Description { get; set; }
        public string FengShuiMeaning { get; set; }
        public int Size { get; set; }
        [Range(1, 100)]
        public int YearOld { get; set; }
        public int MinLife { get; set; }
        public int MaxLife { get; set; }
        public Decimal Price { get; set; }
        public int Quantity {  get; set; }

        public string? Image { get; set; }
        public Double? NOPWR { get; set; } = 0;
        public int? Rates { get; set; } = 0;

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
