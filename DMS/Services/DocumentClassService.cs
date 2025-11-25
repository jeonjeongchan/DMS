using DMS.Data;
using DMS.Models;
using Microsoft.EntityFrameworkCore;

namespace DMS.Services
{
    public class DocumentClassService
    {
        private readonly ApplicationDbContext _context;

        public DocumentClassService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> CreateAsync(T_Document_Class docClass)
        {
            _context.Document_Classes.Add(docClass);
            await _context.SaveChangesAsync();
            return (true, "문서 분류 등록 완료");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int SEQ, T_Document_Class docClass)
        {
            var docClassCheck = await _context.Document_Classes.FindAsync(SEQ);
            if (docClassCheck == null) return (false, "문서 분류 없음");

            docClassCheck.NAME = docClass.NAME;
            docClassCheck.P_SEQ = docClass.P_SEQ;

            _context.Document_Classes.Update(docClassCheck);
            await _context.SaveChangesAsync();
            return (true, "문서 분류 수정 완료");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int SEQ)
        {
            var docClassCheck = await _context.Document_Classes.FindAsync(SEQ);
            if (docClassCheck == null) return (false, "문서 분류 없음");

            _context.Document_Classes.Remove(docClassCheck);
            await _context.SaveChangesAsync();
            return (true, "문서 분류 삭제 완료");
        }

        public async Task<List<T_Document_Class>> GetAllAsync()
        {
            return await _context.Document_Classes.ToListAsync();
        }

        public async Task<T_Document_Class?> GetByIdAsync(int SEQ)
        {
            return await _context.Document_Classes.FindAsync(SEQ);
        }
    }
}
