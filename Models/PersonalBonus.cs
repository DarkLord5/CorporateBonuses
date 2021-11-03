using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.Models
{
    public class PersonalBonus
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [ForeignKey("Bonus")]
        public int BonusId { get; set; }
        [Display(Name = "Enable Date")]
        [DataType(DataType.Date)]
        public DateTime EnableDate { get; set; }
    }
}
