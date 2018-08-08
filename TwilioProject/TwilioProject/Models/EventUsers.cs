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
        public string UserID { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [ForeignKey("Events")]
        public int? EventID { get; set; }
        public Events Events { get; set; }
        public bool? isBanned { get; set; }
        public string Id1 { get; set; }
        public string Id2 { get; set; }
        public string Id3 { get; set; }
        public string Id4 { get; set; }
        public string Id5 { get; set; }
        public string Title1 { get; set; }
        public string Title2 { get; set; }
        public string Title3 { get; set; }
        public string Title4 { get; set; }
        public string Title5 { get; set; }
        

    }
}