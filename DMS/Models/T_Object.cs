using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace DMS.Models
{
    public class T_Object
    {
        [Key]
        public string? OID { get; set; }

        public string? TYPE { get; set; }
        public string? SUBTYPE { get; set; }
        public string? NAME { get; set; }
        public string? TITLE { get; set; }
        public string? POLICY_OID { get; set; }
        public string? CONTENT { get; set; }
        public int? REVISION { get; set; }
        public string? PREVOID { get; set; }
        public string? STATE { get; set; }
        public string? CREATE_USER { get; set; }
        public int? RECENT { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CREATE_DATE { get; set; }
        public string? MODIFY_USER { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? MODIFY_DATE { get; set; }
        public string? DELETE_USER { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DELETE_DATE { get; set; }
        public string? APPROVE_USER { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? APPROVE_DATE { get; set; }
        public char? USEFLAG { get; set; }

    }


}