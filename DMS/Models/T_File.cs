using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DMS.Models
{
    public class T_File
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? SEQ { get; set; }

        public string? FILE_NAME { get; set; }
        public long? FILE_SIZE { get; set; }
        public int? ORDER { get; set; }
        public string? FILE_PATH  { get; set; }
        public string? CREATE_USER { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string? DESCRIPTION { get; set; }
        public char? REPRESENT_FILE { get; set; }


        [JsonIgnore]
        public ICollection<R_File_Document?>? R_FILE_DOCUMENT { get; set; }
    }

}


