using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TwilioProject.Models
{
    public class EventUsers
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        public int PhoneNumber { get; set; }
        [ForeignKey("Events")]
        public int EventID { get; set; }
        public Events Events { get; set; }
        [Display(Name = "Number of Messages")]
        public int NumbOfMessages { get; set; }
    }
}