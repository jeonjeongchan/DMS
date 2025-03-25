using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DMS.Models
{
    public class R_File_Document
    {
        public string? OID { get; set; }
        //[NotMapped]
        public T_Document? T_Document { get; set; }

        public int? SEQ { get; set; }
        //[NotMapped]
        public T_File? T_File { get; set; }

    }



}

