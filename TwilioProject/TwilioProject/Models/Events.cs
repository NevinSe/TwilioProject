using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TwilioProject.Models
{
    public class Events
    {
        [Key]
        public int EventID { get; set; }
        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; }
        [ForeignKey("ApplicationUser")]
        public string HostID { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        [Required]
        [Display(Name = "Event Code")]
        public int EventCode { get; set; }
    }
}