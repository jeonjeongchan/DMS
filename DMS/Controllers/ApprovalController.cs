using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DMS.Models;
using System.Data;
using DMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;

namespace DMS.Controllers;

[Route("Approval")]
public class ApprovalController : Controller
{

    private readonly ApprovalService _approvalService;

    public ApprovalController(ApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpGet("/Approval")]
    public async Task<IActionResult> ApprovalMain()
    {
        var approvals = await _approvalService.GetApprovals(); // 비동기 메서드 사용
        return View(approvals);
    }


    
    [HttpGet("/Approval/Create")]
    public IActionResult CreateApproval()
    {
        return PartialView("ApprovalModalForm", new T_Approval());
    }

    [HttpPost("/Approval/Create")]
    public async Task<IActionResult> CreateApproval(T_Approval model)
    {
        if (ModelState.IsValid)
        {

           await _approvalService.AddApprovalAsync(model); // 비동기 메서드 사용

        }

        return Ok(new { success = true, message = "결재가 등록되었습니다." });
    }

    // 에러
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

