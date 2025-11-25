using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DMS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using DMS.Services;


namespace DMS.Controllers;

//[Route("Account")]
public class AccountController : Controller
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    // 로그인 화면
    [HttpGet]
    public IActionResult Login()
    {
        var username = HttpContext.Session.GetString("Username");
        if (!string.IsNullOrEmpty(username))
        {
            return RedirectToAction("DashBoard", "Main");
        }

        return View();
    }

    // 회원가입 화면
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // 회원가입
    [HttpPost]
    public IActionResult Register(T_Member member)
    {
        if (ModelState.IsValid)
        {
            var memberCheck = _accountService.CheckMemberID(member.MEMBER_ID);
            if (memberCheck != null)
            {
                ModelState.AddModelError("MEMBER_ID", "이미 사용 중인 아이디입니다.");
                return View(member);
            }

            member.GRADE = "USER";
            _accountService.RegisterService(member);

            // 알림 메시지 설정
            TempData["SuccessMessage"] = "회원가입이 성공적으로 완료 되었습니다.";
            return RedirectToAction("Login"); // 회원가입 후 로그인 페이지로 리다이렉션
        }

        // 유효성 검증 실패 시
        return View(member);
    }

    // 로그인
    [HttpPost]
    public async Task<IActionResult> Login(T_Member member)
    {
        if (ModelState.IsValid)
        {
            // 사용자 인증 로직 (예: DB에서 확인)
            bool isValidUser = _accountService.CheckUserCredentials(member.MEMBER_ID, member.PASSWORD); // 사용자 인증 메서드
          
            if (isValidUser)
            {
                var memberCheck = _accountService.CheckMemberID(member.MEMBER_ID);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, member.MEMBER_ID),
                    new Claim(ClaimTypes.Role, "User")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                HttpContext.Session.SetString("OID", memberCheck.OID);
                HttpContext.Session.SetString("Username", memberCheck.NAME);
                HttpContext.Session.SetString("UserID", memberCheck.MEMBER_ID);
                HttpContext.Session.SetString("Grade", memberCheck.GRADE);

                return RedirectToAction("DashBoard", "main");
            }

            ModelState.AddModelError(string.Empty, "로그인을 실패하였습니다.");
        }
        else
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }

        return View(member);
    }


    // 로그아웃
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }


    public async Task<IActionResult> MyInfo()
    {
        var member_id = HttpContext.Session.GetString("UserID");
        var member = _accountService.CheckMemberID(member_id);
        return Ok(member); 
    }

    public async Task<IActionResult> Edit(string OID, [FromBody] T_Member member)
    {
        if (OID != member.OID)
        {
            return BadRequest();
        }

       
        bool check = _accountService.EditService(OID, member);

        if (check)
        {
            return Ok(new { message = "변경 완료" });
        }
        else
        {
            return BadRequest( new { message = "변경 실패" });
        }
        
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

