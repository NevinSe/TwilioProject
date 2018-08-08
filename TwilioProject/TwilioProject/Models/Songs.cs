using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TwilioProject.Models
{
    public class Songs
    {
        [Key]
        public int? SongID { get; set; }
        [Required]
        [Display(Name = "Song Title")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Song Length")]
        public int SongLength { get; set; }
        public string YoutubeId { get; set; }
        [ForeignKey("Events")]
        public int? EventID { get; set; }
        public Events Events { get; set; }
        [Display(Name = "Likes")]
        public int? Likes { get; set; }
        [Display(Name = "Dislikes")]
        public int? Dislikes { get; set; }
        [Required]
        [Display(Name = "Banned")]
        public bool? IsBanned { get; set; }
    }
}