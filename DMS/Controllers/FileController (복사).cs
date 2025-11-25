//using System.Diagnostics;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using DMS.Models;
//using Microsoft.AspNetCore.StaticFiles;
//using Microsoft.EntityFrameworkCore;
//using DMS.Data;
//using System.Security.Cryptography;

//namespace DMS.Controllers;

//[Route("File")]
//public class FileController : Controller
//{

//    private readonly ApplicationDbContext _context;

//    public FileController(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    private readonly string _fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

//    // 파일 다운로드
//    [HttpGet("/File/{fileName}")]
//    public IActionResult Download(string fileName)
//    {
//        if (string.IsNullOrEmpty(fileName))
//        {
//            return BadRequest("파일 이름이 제공되지 않았습니다.");
//        }

//        var filePath = Path.Combine(_fileStoragePath, fileName);

//        if (!System.IO.File.Exists(filePath))
//        {
//            return NotFound("파일이 존재하지 않습니다.");
//        }

//        var fileBytes = System.IO.File.ReadAllBytes(filePath);
//        var contentType = GetContentType(filePath);
//        return File(fileBytes, contentType, fileName);
//    }

//    // 파일 삭제
//    [HttpDelete("/File/{seq}")]
//    public async Task<IActionResult> FileRemove(int seq)
//    {

//        // 1. 데이터베이스에서 파일 정보 찾기
//        var file = _context.Files.FirstOrDefault(f => f.SEQ == seq);
//        if (file == null)
//        {
//            return NotFound("파일이 존재하지 않습니다.");
//        }

//        var fileRel = _context.R_File_Documents.FirstOrDefault(f => f.SEQ == file.SEQ);


//        // 2. 서버 파일 경로 설정
//        var filePath = Path.Combine(_fileStoragePath, file.FILE_NAME);
//        if (System.IO.File.Exists(filePath))
//        {
//            System.IO.File.Delete(filePath); // 3. 파일 삭제
//        }

//        // 4. 데이터베이스에서 삭제
//        _context.Files.Remove(file);
//        _context.R_File_Documents.Remove(fileRel);
//        await _context.SaveChangesAsync();

//        return Ok("파일이 삭제되었습니다.");

//    }



//    private string GetContentType(string path)
//    {
//        var provider = new FileExtensionContentTypeProvider();
//        if (!provider.TryGetContentType(path, out var contentType))
//        {
//            contentType = "application/octet-stream";
//        }
//        return contentType;
//    }





//}

