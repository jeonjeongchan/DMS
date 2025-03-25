using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Models
{
    public class T_Member : T_Object
    {
        public string? DEPARTMENT_OID { get; set; }
        public int? GROUP_SEQ { get; set; }
        public string? MEMBER_ID { get; set; }
        public string? PASSWORD { get; set; }
        public DateTime BIRTH_DATE { get; set; }
        public string? POSITION { get; set; }
        public string? PHONE { get; set; }
        public DateTime? RESIGN_DATE { get; set; }
        public DateTime? RETIRE_DATE { get; set; }
        public string? PHOTO { get; set; }
        public string? GENDER { get; set; }

        [NotMapped]
        public string? PasswordHash { get; set; }
    }


}
