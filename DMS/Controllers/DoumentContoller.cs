using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DMS.Models;
using System.Data;
using DMS.Services;

namespace DMS.Controllers;

[Route("Document")]
public class DocumentController : Controller
{

    private readonly DocumentService _documentService;
    private readonly DocumentClassService _docClassService;

    public DocumentController(DocumentService documentService, DocumentClassService docClassService)
    {
        _documentService = documentService;
        _docClassService = docClassService;
    }

    // =========================
    // 문서 관련
    // =========================

    // 문서관리 메인 화면
    [HttpGet("DocClass/{filterType?}")]
    public async Task<IActionResult> DocumentMain(int? filterType)
    {
        var docClassTreeMenu = _documentService.GetDocumentClasses();
        ViewBag.docClasses = docClassTreeMenu;
        ViewBag.CurrentPageNum = filterType;
        var documents = await _documentService.GetDocumentsAsync(filterType);
        return View(documents);
    }



    // 문서 등록
    [HttpPost]
    public async Task<IActionResult> Create(T_Document document)
    {
        var username = HttpContext.Session.GetString("Username") ?? "Unknown";
        var result = await _documentService.CreateDocumentAsync(document, username);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // 문서 수정
    [HttpPut("{OID}")]
    public async Task<IActionResult> Update(string OID, T_Document document)
    {
        var username = HttpContext.Session.GetString("Username") ?? "Unknown";
        var result = await _documentService.UpdateDocumentAsync(OID, document, username);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // 문서 삭제
    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromBody] Common common)
    {
        var result = await _documentService.DeleteDocumentAsync(common.OIDs);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // 문서 개정
    [HttpPut("Revision")]
    public async Task<IActionResult> Revision([FromBody] T_Document document)
    {
        var username = HttpContext.Session.GetString("Username") ?? "Unknown";
        var result = await _documentService.RevisionDocumentAsync(document, username);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // 문서 상세 조회
    [HttpGet("{OID}")]
    public async Task<IActionResult> Detail(string OID)
    {
        var documentDetail = await _documentService.GetDocumentDetailAsync(OID);
        if (documentDetail == null) return NotFound();
        return Ok(documentDetail);
    }

    // =========================
    // 문서분류 관련
    // =========================

    // 문서분류 등록
    [HttpPost("/DocumentClass")]
    public async Task<IActionResult> CreateClass(T_Document_Class docClass)
    {
        var result = await _docClassService.CreateAsync(docClass);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // 문서분류 수정
    [HttpPut("/DocumentClass/{SEQ}")]
    public async Task<IActionResult> UpdateClass(int SEQ, T_Document_Class docClass)
    {
        var result = await _docClassService.UpdateAsync(SEQ, docClass);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // 문서분류 삭제
    [HttpDelete("/DocumentClass/{SEQ}")]
    public async Task<IActionResult> DeleteClass(int SEQ)
    {
        var result = await _docClassService.DeleteAsync(SEQ);
        if (!result.Success) return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    // 문서분류 전체 조회
    [HttpGet("/DocumentClass")]
    public async Task<IActionResult> GetAllClass()
    {
        var list = await _docClassService.GetAllAsync();
        return Ok(list);
    }

    // 문서분류 조회 (SEQ)
    [HttpGet("/DocumentClass/{SEQ}")]
    public async Task<IActionResult> GetClassById(int SEQ)
    {
        var item = await _docClassService.GetByIdAsync(SEQ);
        if (item == null) return NotFound();
        return Ok(item);
    }



    // 에러
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

