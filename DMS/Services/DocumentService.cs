using System;
using DMS.Data;
using DMS.Models;
using Microsoft.EntityFrameworkCore;
using DMS.CommonUtil;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.CodeAnalysis;

namespace DMS.Services
{
    public class DocumentService
    {
        private readonly ApplicationDbContext _context;

        public DocumentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 문서 분류 가져오기
        public List<T_Document_Class> GetDocumentClasses()
        {
            var documentClasses = _context.Document_Classes
                .OrderBy(dc => dc.ORDER)
                .ToList()
                .Select(dc => new T_Document_Class
                {
                    SEQ = dc.SEQ,
                    P_SEQ = dc.P_SEQ,
                    NAME = dc.NAME,
                    LEVEL = dc.LEVEL,
                    ORDER = dc.ORDER,
                    DOC_COUNT = _context.Documents.Count(d => d.DOC_CLASS_SEQ == dc.SEQ),
                    Children = new List<T_Document_Class>()
                })
                .ToList();

            var root = new T_Document_Class();
            return root.BuildTree(documentClasses);
        }

        // 문서 목록 가져오기 (필터 optional)
        public async Task<List<T_Document>> GetDocumentsAsync(int? filterType)
        {
            var documentQuery = from d in _context.Documents
                                join c in _context.Document_Classes
                                on d.DOC_CLASS_SEQ equals c.SEQ
                                where d.TYPE == "DOCUMENT"
                                select new T_Document
                                {
                                    OID = d.OID,
                                    DOC_CLASS_SEQ = c.SEQ,
                                    TITLE = d.TITLE,
                                    NAME = d.NAME,
                                    TYPE = d.TYPE,
                                    CREATE_DATE = d.CREATE_DATE,
                                    MODIFY_DATE = d.MODIFY_DATE,
                                    CREATE_USER = d.CREATE_USER,
                                    MODIFY_USER = d.MODIFY_USER,
                                    REVISION = d.REVISION,
                                    RECENT = d.RECENT,
                                    DOC_CLASS_NAME = c.NAME
                                };

            if (filterType.HasValue)
            {
                documentQuery = documentQuery.Where(d => d.DOC_CLASS_SEQ == filterType.Value);
            }


            return await documentQuery
                .OrderByDescending(d => d.CREATE_DATE).ToListAsync();
        }

        // 문서 상세 조회
        public async Task<T_Document?> GetDocumentDetailAsync(string OID)
        {
            var documentDetail = await _context.Documents.FindAsync(OID);
            if (documentDetail == null)
                return null;

            // 문서 클래스 이름 매핑
            if (documentDetail.DOC_CLASS_SEQ != null)
            {
                var docClass = await _context.Document_Classes
                    .FindAsync(documentDetail.DOC_CLASS_SEQ);

                if (docClass != null)
                    documentDetail.DOC_CLASS_NAME = docClass.NAME;
            }

            // 관련 파일 조회
            var fileRelList = await _context.R_File_Documents
                .Where(o => o.OID == OID)
                .ToListAsync();

            var fileList = new List<string>();

            foreach (var rel in fileRelList)
            {
                var fileInfo = await _context.Files
                    .SingleOrDefaultAsync(o => o.SEQ == rel.SEQ);

                if (fileInfo != null)
                    fileList.Add(fileInfo.FILE_NAME);
            }

            if (fileList.Count > 0)
                documentDetail.fileList = fileList;

            return documentDetail;
        }


        // 문서 등록
        public async Task<(bool Success, string Message)> CreateDocumentAsync(T_Document document, string username)
        {

            document.OID = Encryption.CreateRandomKey();   
            document.TYPE = "DOCUMENT";
            document.CREATE_USER = username;
            document.CREATE_DATE = DateTime.Now;
            document.STATE = "작성중";
            document.REVISION = 0;
            document.RECENT = 1;
            document.USEFLAG = '1';

            var filelist = new List<T_File>();

            if (document.T_FILE_LIST != null)
            {
                foreach (var file in document.T_FILE_LIST)
                {
                    if (file.Length == 0)
                        return (false, "용량이 빈 파일은 등록할 수 없습니다.");
                }

                foreach (var file in document.T_FILE_LIST)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadPath = Path.Combine("wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileData = new T_File
                    {
                        FILE_NAME = fileName,
                        FILE_PATH = uploadPath,
                        FILE_SIZE = file.Length,
                        CREATE_DATE = DateTime.Now
                    };

                    // 시퀀스 조회
                    using (var connection = new OracleConnection(_context.Database.GetDbConnection().ConnectionString))
                    {
                        await connection.OpenAsync();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT JJC.FILE_SEQ.NEXTVAL FROM DUAL";
                            var result = await command.ExecuteScalarAsync();
                            fileData.SEQ = Convert.ToInt32(result);

                            var file_document = new R_File_Document
                            {
                                T_File = fileData,
                                T_Document = document,
                            };
                            _context.R_File_Documents.Add(file_document);
                        }
                    }
                    filelist.Add(fileData);
                }

                _context.Files.AddRange(filelist);
            }


            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT JJC.DOCUMENT_SEQ.NEXTVAL FROM DUAL";

            var nextVal = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            document.NAME = $"DOCUMENT_{nextVal}";

            _context.Objects.Add(document);
            _context.Documents.Add(document);

            await _context.SaveChangesAsync();
            return (true, "문서 등록 완료");
        }

        // 문서 수정
        public async Task<(bool Success, string Message)> UpdateDocumentAsync(string OID, T_Document updateDocument, string username)
        {
            if (OID != updateDocument.OID)
                return (false, "데이터가 변경되어 없는 문서입니다.");

            var document = await _context.Documents.FindAsync(OID);
            if (document == null)
                return (false, "문서를 찾을 수 없습니다.");

            if (document.RECENT == 0)
                return (false, "최신 문서만 수정 할 수 있습니다.");

            var relFileDocument = await _context.Documents
                .Where(d => d.OID == OID)
                .Include(d => d.R_FILE_DOCUMENT)
                .ToListAsync();

            document.TITLE = updateDocument.TITLE;
            document.CONTENT = updateDocument.CONTENT;
            document.DOC_CLASS_SEQ = updateDocument.DOC_CLASS_SEQ;
            document.MODIFY_DATE = DateTime.Now;
            document.MODIFY_USER = username;

            _context.Entry(document).State = EntityState.Modified;

            if (updateDocument.T_FILE_LIST != null)
            {
                var docFileCount = updateDocument.T_FILE_LIST.Count;
                if (relFileDocument[0].R_FILE_DOCUMENT != null)
                    docFileCount += relFileDocument[0].R_FILE_DOCUMENT.Count;

                if (docFileCount > 5)
                    return (false, "파일 첨부 개수는 최대 5개까지 가능합니다.");

                foreach (var file in updateDocument.T_FILE_LIST)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var uploadPath = Path.Combine("wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileData = new T_File
                    {
                        FILE_NAME = fileName,
                        FILE_PATH = uploadPath,
                        FILE_SIZE = file.Length,
                        CREATE_DATE = DateTime.Now
                    };


                    var connection = _context.Database.GetDbConnection();
                    if (connection.State != System.Data.ConnectionState.Open)
                        await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT JJC.FILE_SEQ.NEXTVAL FROM DUAL";

                        var result = await command.ExecuteScalarAsync();
                        fileData.SEQ = Convert.ToInt32(result);
                    }

                    // R_File_Document 관계 엔티티 추가
                    var file_document = new R_File_Document
                    {
                        T_File = fileData,
                        T_Document = document,
                    };
                    _context.R_File_Documents.Add(file_document);

                    // 파일 엔티티 추가
                    _context.Files.Add(fileData);
                }
            }

            await _context.SaveChangesAsync();
            return (true, "문서 편집 완료");
        }

        // 문서 삭제
        public async Task<(bool Success, string Message)> DeleteDocumentAsync(List<string> OIDs)
        {
            var documentsToDelete = await _context.Documents
                .Where(d => OIDs.Contains(d.OID))
                .Include(d => d.R_FILE_DOCUMENT)
                .ToListAsync();

            if (documentsToDelete.Count == 0)
                return (false, "데이터가 변경되어 없는 문서입니다.");

            foreach (var doc in documentsToDelete)
            {
                if (doc.RECENT == 0)
                    return (false, "최신 문서만 삭제할 수 있습니다.");

                var prevDocument = await _context.Documents.FindAsync(doc.PREVOID);
                if (prevDocument != null)
                {
                    prevDocument.RECENT = 1;
                    _context.Entry(prevDocument).State = EntityState.Modified;
                }

                _context.R_File_Documents.RemoveRange(doc.R_FILE_DOCUMENT);
            }

            _context.Documents.RemoveRange(documentsToDelete);
            await _context.SaveChangesAsync();

            return (true, "문서 삭제 완료");
        }

        // 문서 개정
        public async Task<(bool Success, string Message)> RevisionDocumentAsync(T_Document document, string username)
        {
            var documentCheck = await _context.Documents.FindAsync(document.OID);
            if (documentCheck == null) return (false, "데이터가 없는 문서입니다.");
            if (documentCheck.RECENT == 0) return (false, "이 문서는 최신 문서가 아닙니다.");

            documentCheck.RECENT = 0;
            _context.Entry(documentCheck).State = EntityState.Modified;

            var recentDocument = new T_Document
            {
                OID = Encryption.CreateRandomKey(),
                NAME = documentCheck.NAME,
                TITLE = documentCheck.TITLE,
                CONTENT = documentCheck.CONTENT,
                TYPE = "DOCUMENT",
                CREATE_USER = username,
                CREATE_DATE = DateTime.Now,
                STATE = "작성중",
                REVISION = documentCheck.REVISION + 1,
                RECENT = 1,
                USEFLAG = '1',
                PREVOID = documentCheck.OID,
                DOC_CLASS_SEQ = documentCheck.DOC_CLASS_SEQ
            };

            _context.Objects.Add(recentDocument);
            _context.Documents.Add(recentDocument);

            await _context.SaveChangesAsync();
            return (true, "문서 개정 완료");
        }
    }
}
