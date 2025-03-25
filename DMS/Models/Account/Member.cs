using System;
using System.ComponentModel.DataAnnotations;

namespace DMS.Areas.Identity.Pages.Account
{
    public class Member 
    {
        [Required(ErrorMessage = "사용자 아이디를 입력하세요.")] // Not Null 설정
        public string? MEMBER_ID { get; set; }
        [Required(ErrorMessage = "사용자 비밀번호를 입력하세요.")] // Not Null 설정
        public string? PASSWORD { get; set; }
    }
}
