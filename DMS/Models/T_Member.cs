using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Models
{
    public class T_Member
    {
        public string? OID { get; set; }
        public string? DEPARTMENT_OID { get; set; }
        public string? GROUP_SEQ { get; set; }
        public string? MEMBER_ID { get; set; }
        public string? PASSWORD { get; set; }
        public string? NAME { get; set; }
        public DateTime BIRTH_DATE { get; set; }
        public string? POSITION { get; set; }
        public string? PHONE { get; set; }
    }


}
