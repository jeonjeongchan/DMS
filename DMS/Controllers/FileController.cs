using Microsoft.AspNetCore.Mvc;
using DMS.Services;

namespace DMS.Controllers
{
    [Route("File")]
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        // 파일 다운로드
        [HttpGet("/File/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var result = _fileService.DownloadFile(fileName);
            if (result == null)
            {
                return NotFound("파일이 존재하지 않거나 이름이 제공되지 않았습니다.");
            }

            return File(result.Value.FileBytes, result.Value.ContentType, result.Value.FileName);
        }

        // 파일 삭제
        [HttpDelete("/File/{seq}")]
        public async Task<IActionResult> FileRemove(int seq)
        {
            var (success, message) = await _fileService.DeleteFileAsync(seq);
            if (!success)
            {
                return NotFound(message);
            }

            return Ok(message);
        }
    }
}
