using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DMS.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace DMS.Controllers;

[Route("files")]
public class FileController : Controller
{
    private readonly ILogger<FileController> _logger;

    public FileController(ILogger<FileController> logger)
    {
        _logger = logger;
    }

    private readonly string _fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

    [HttpGet("download")]
    public IActionResult Download(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest("파일 이름이 제공되지 않았습니다.");
        }

        var filePath = Path.Combine(_fileStoragePath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("파일이 존재하지 않습니다.");
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var contentType = GetContentType(filePath);
        return File(fileBytes, contentType, fileName);
    }

    private string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

}

