    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DMS.Models
{
    public class T_Approval : T_Object {

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? REQUEST_DATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? APPROVE_DATE { get; set; }
        public DateTime? APPROVE_USER { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EXPIRATION_DATE { get; set; }
        public string? CA_USER { get; set; }
        public string? NA_USER { get; set; }
        public string? FA_USER { get; set; }





    }


}

