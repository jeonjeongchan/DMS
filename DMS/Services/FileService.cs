using DMS.Data;
using DMS.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services
{
    public class FileService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _fileStoragePath;

        public FileService(ApplicationDbContext context)
        {
            _context = context;
            _fileStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        }

        // 파일 다운로드
        public (byte[] FileBytes, string ContentType, string FileName)? DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var filePath = Path.Combine(_fileStoragePath, fileName);

            if (!System.IO.File.Exists(filePath))
                return null;

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(filePath);

            return (fileBytes, contentType, fileName);
        }

        // 파일 삭제
        public async Task<(bool Success, string Message)> DeleteFileAsync(int seq)
        {
            var file = await _context.Files.FirstOrDefaultAsync(f => f.SEQ == seq);
            if (file == null)
                return (false, "파일이 존재하지 않습니다.");

            var fileRel = await _context.R_File_Documents.FirstOrDefaultAsync(f => f.SEQ == file.SEQ);

            var filePath = Path.Combine(_fileStoragePath, file.FILE_NAME);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Files.Remove(file);
            if (fileRel != null)
            {
                _context.R_File_Documents.Remove(fileRel);
            }

            await _context.SaveChangesAsync();
            return (true, "파일이 삭제되었습니다.");
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
}
