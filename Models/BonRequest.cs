using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorporateBonuses.Models
{
    public class BonRequest
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Bonus")]
        public int BonusId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public string Status { get; set; }
        public int Price { get; set; }

        [Display(Name = "Approve Date")]
        [DataType(DataType.Date)]
        public DateTime ApproveDate { get; set; }
    }
}
