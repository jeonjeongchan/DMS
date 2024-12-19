using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DMS.Models
{
    public class T_Menu
    {
        [Key]
        public int SEQ { get; set; }
        public string? NAME { get; set; }
        public int? ORDER { get; set; }
        public string? ADDRESS { get; set; }
        public string? ICON { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string? CREATE_USER { get; set; }
        public DateTime? DELETE_DATE { get; set; }
        public string? DELETE_USER { get; set; }
        public char USEFLAG { get; set; }

    }



}

