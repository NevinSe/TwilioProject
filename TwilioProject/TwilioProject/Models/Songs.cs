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
        public int SongID { get; set; }
        [Required]
        [Display(Name = "Song Name")]
        public string SongName { get; set; }
        [Required]
        [Display(Name = "Artist")]
        public string Artist { get; set; }
        [Required]
        [Display(Name = "Song Lenght")]
        public int SongLength { get; set; }
        [Required]
        [Display(Name = "Genere")]
        public string Genre { get; set; }
        [Display(Name = "Album Cover Art")]
        public string AlbumCoverIMG { get; set; }
        [ForeignKey("Events")]
        public int EventID { get; set; }
        public Events Events { get; set; }
        [Display(Name = "Likes")]
        public int Likes { get; set; }
        [Display(Name = "Dislikes")]
        public int Dislikes { get; set; }
        [Required]
        [Display(Name = "Banned")]
        public bool IsBanned { get; set; }
    }
}