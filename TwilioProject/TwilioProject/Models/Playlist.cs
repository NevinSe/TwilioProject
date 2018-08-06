using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TwilioProject.Models
{
    public class Playlist
    {
        [Key]
        public int SongOrderID { get; set; }
        public string YoutubeID { get; set; }
        public string Title { get; set; }
    }
}