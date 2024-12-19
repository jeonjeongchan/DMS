using System;
using System.ComponentModel.DataAnnotations;

namespace DMS.Models
{

    public class LoginModel 
    {
        [Required(ErrorMessage = "사용자 아이디를 입력하세요.")] // Not Null 설정
        public string? LOGIN_ID { get; set; }
        [Required(ErrorMessage = "사용자 비밀번호를 입력하세요.")] // Not Null 설정
        public string? PASSWORD { get; set; }
    }
}
