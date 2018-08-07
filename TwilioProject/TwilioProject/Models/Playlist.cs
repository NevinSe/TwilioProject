using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TwilioProject.Models
{
    public class Playlist
    {
        [Key]
        public int SongOrderID { get; set; }
        public int SongLength { get; set; }
        public string YoutubeID { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
    }
}