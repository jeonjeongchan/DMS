using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DMS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using System.Xml.Linq;
using DMS.Data;
using Microsoft.EntityFrameworkCore;
using DMS.CommonUtil;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection.Metadata;
using Azure;
using Microsoft.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Emit;

namespace DMS.Controllers;

[Route("Document")]
public class DocumentController : Controller
{

    private readonly ApplicationDbContext _context;

    public DocumentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 문서관리 메인 화면
    [HttpGet("/Document/DocClass/{filterType?}")]
    public async Task<IActionResult> DocumentMain(int? filterType)
    {
        var documentClasses = _context.Document_Classes
            .OrderBy(dc => dc.ORDER)
            .ToList() // 먼저 리스트로 가져옴
            .Select(dc => new T_Document_Class
            {
                SEQ = dc.SEQ,
                P_SEQ = dc.P_SEQ,
                NAME = dc.NAME,
                LEVEL = dc.LEVEL,
                ORDER = dc.ORDER,
                DOC_COUNT = _context.Documents.Count(d => d.DOC_CLASS_SEQ == dc.SEQ), // 연결된 문서 개수 계산
                Children = new List<T_Document_Class>() // 기본값 설정
            })
            .ToList();

        T_Document_Class docClassTree = new T_Document_Class();
        var docClassTreeMenu = docClassTree.BuildTree(documentClasses);
        ViewBag.docClasses = docClassTreeMenu;

        var documentQuery = from d in _context.Documents
                    join c in _context.Document_Classes
                    on d.DOC_CLASS_SEQ equals c.SEQ
                    where d.TYPE == "DOCUMENT"
                    select new T_Document
                    {
                        OID = d.OID,
                        DOC_CLASS_SEQ = c.SEQ,
                        TITLE = d.TITLE,
                        TYPE = d.TYPE,
                        CREATE_DATE = d.CREATE_DATE,
                        MODIFY_DATE = d.MODIFY_DATE,
                        CREATE_USER = d.CREATE_USER,
                        MODIFY_USER = d.MODIFY_USER,
                        REVISION = d.REVISION,
                        RECENT = d.RECENT,
                        DOC_CLASS_NAME = c.NAME // 별칭 사용
                    };

        if (filterType.HasValue)
        {
            documentQuery = documentQuery.Where(document => document.DOC_CLASS_SEQ == filterType);
        }

        var documents = await documentQuery
            .OrderByDescending(document => document.CREATE_DATE)
            .ToListAsync();


        return View(documents);

    }



    // 문서관리 상세 화면
    [HttpGet("/Document/{OID}")]
    public async Task<IActionResult> GetDocumentDetail(string OID)
    {
        var documentDetail = await _context.Documents.FindAsync(OID);
        if (documentDetail == null)
        {
            return NotFound();
        }


        if (documentDetail.DOC_CLASS_SEQ != null)
        {
            var docClass = await _context.Document_Classes.FindAsync(documentDetail.DOC_CLASS_SEQ);
            documentDetail.DOC_CLASS_NAME = docClass.NAME;
        }

        var docClasses = await _context.Document_Classes.ToListAsync();

        var fileRelList = await _context.R_File_Documents.Where(o => o.OID == OID).ToListAsync();

        var fileList = new List<string>();

        if (fileRelList.Count > 0)
        {
            for (var i = 0; i < fileRelList.Count; i++)
            {
                var fileInfo = await _context.Files.SingleOrDefaultAsync(o => o.SEQ == fileRelList[i].SEQ);
                if (fileInfo != null)
                {
                    fileList.Add(fileInfo.FILE_NAME);
                }

            }
            if (fileList != null)
            {
                documentDetail.fileList = fileList;
            }

        }

        return Ok(documentDetail);
    }

    // 문서 등록
    [HttpPost("/Document")]
    public async Task<IActionResult> CreateDocument(T_Document document)
    {
        if (ModelState.IsValid)
        {
            document.OID = Encryption.CreateRandomKey();
            document.TYPE = "DOCUMENT";
            document.CREATE_USER = HttpContext.Session.GetString("Username");
            document.CREATE_DATE = DateTime.Now;
            document.STATE = "작성중";
            document.REVISION = 0;
            document.RECENT = 1;
            document.USEFLAG = '1';

            var filelist = new List<T_File>();

            if (document.T_FILE_LIST != null)
            {
                for (var i = 0; i < document.T_FILE_LIST.Count; i++)
                {
                    if (document.T_FILE_LIST[i].Length == 0)
                    {
                        return BadRequest(new { Message = "용량이 빈파일은 등록할수 없습니다." });
                    }
                }

                for (var i = 0; i < document.T_FILE_LIST.Count; i++)
                {
                    var fileName = Path.GetFileName(document.T_FILE_LIST[i].FileName);
                    var uploadPath = Path.Combine("wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await document.T_FILE_LIST[i].CopyToAsync(stream);
                    }

                    var fileData = new T_File
                    {
                        FILE_NAME = fileName,
                        FILE_PATH = uploadPath,
                        FILE_SIZE = document.T_FILE_LIST[i].Length,
                        CREATE_DATE = DateTime.Now

                    };

                    using (var connection = new OracleConnection(_context.Database.GetDbConnection().ConnectionString))
                    {
                        await connection.OpenAsync();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT JJC.FILE_SEQ.NEXTVAL FROM DUAL";
                            var result = await command.ExecuteScalarAsync();
                            int sequenceValue = Convert.ToInt32(result);
                            fileData.SEQ = sequenceValue;

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

            _context.Objects.Add(document);
            _context.Documents.Add(document);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Error: {dbEx.InnerException?.Message}");
                return StatusCode(500, "Database error occurred");
            }

            return Ok(new { message = "문서 등록 완료" });
        }
        else
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }
        return BadRequest(new { success = false });
    }

    // 문서 수정
    [HttpPut("/Document/{OID}")]
    public async Task<IActionResult> PutDocument(string OID, T_Document updateDocument)
    {
        if (OID != updateDocument.OID)
        {
            return BadRequest();
        }
        
        try
        {
            var document = await _context.Documents.FindAsync(OID);

            if (document == null)
            {
                return BadRequest();
            }

            var relFileDocument = await _context.Documents
                .Where(d => d.OID == OID)
                .Include(d => d.R_FILE_DOCUMENT)
                .ToListAsync();


            _context.Entry(document).State = EntityState.Modified;

            document.TITLE = updateDocument.TITLE;
            document.CONTENT = updateDocument.CONTENT;
            document.DOC_CLASS_SEQ = updateDocument.DOC_CLASS_SEQ;
            document.MODIFY_DATE = DateTime.Now;
            document.MODIFY_USER = HttpContext.Session.GetString("Username");


            if (updateDocument.T_FILE_LIST != null)
            {
                var docFileCount = updateDocument.T_FILE_LIST.Count;
                if (relFileDocument[0].R_FILE_DOCUMENT != null)
                {
                    docFileCount += relFileDocument[0].R_FILE_DOCUMENT.Count;
                }

                if (docFileCount > 5)
                {
                    return BadRequest(new { message = "파일 첨부 개수는 최대 5개까지 가능합니다." });
                }

                for (var i = 0; i < updateDocument.T_FILE_LIST.Count; i++)
                {
                    var fileName = Path.GetFileName(updateDocument.T_FILE_LIST[i].FileName);
                    var uploadPath = Path.Combine("wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await updateDocument.T_FILE_LIST[i].CopyToAsync(stream);
                    }

                    var fileData = new T_File
                    {
                        FILE_NAME = fileName,
                        FILE_PATH = uploadPath,
                        FILE_SIZE = updateDocument.T_FILE_LIST[i].Length,
                        CREATE_DATE = DateTime.Now

                    };

                    // DB 접속
                    var connectionOracle = "Data Source=localhost:1521/FREEPDB1;User Id=JJC;Password=Qwer1234;";
                    using (var connection = new OracleConnection(connectionOracle))
                    //using (var connection = new OracleConnection(_context.Database.GetDbConnection().ConnectionString))
                    {
                        await connection.OpenAsync();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT JJC.FILE_SEQ.NEXTVAL FROM DUAL";
                            var result = await command.ExecuteScalarAsync();
                            int sequenceValue = Convert.ToInt32(result);
                            fileData.SEQ = sequenceValue;

                            var file_document = new R_File_Document
                            {
                                T_File = fileData,
                                T_Document = document,
                            };

                            _context.R_File_Documents.Add(file_document);
                        }
                    }
                    _context.Files.Add(fileData);
                }


            }

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DocumentExists(OID))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Ok(new { message = "문서 편집 완료" });
    }


    // 문서 삭제
    [HttpDelete("/Document/Delete")]
    public async Task<IActionResult> DeleteDocument([FromBody] Common common)
    {
        if (common.OIDs == null || !common.OIDs.Any())
        {
            return BadRequest();
        }

        try
        {
            var documentsToDelete = await _context.Documents
                .Where(d => common.OIDs.Contains(d.OID))
                .Include(d => d.R_FILE_DOCUMENT)
                .ToListAsync();

            if (documentsToDelete.Count == 0)
            {
                return BadRequest("데이터가 변경되어 없는 문서 입니다.");
            } 


            for (var i = 0; i < documentsToDelete.Count; i++)
            {
                if (documentsToDelete[i].RECENT == 0)
                {
                    return BadRequest("최신 문서만 삭제 할수있습니다.");
                }
                else
                {
                    var prevDocument = await _context.Documents.FindAsync(documentsToDelete[i].PREVOID);
                    if (prevDocument != null)
                    {
                        prevDocument.RECENT = 1;
                        //_context.Entry(prevDocument).Property(x => x.RECENT).IsModified = true;
                        _context.Entry(prevDocument).State = EntityState.Modified;          
                    }

                }
                
                for (var j = 0; j < documentsToDelete[i].R_FILE_DOCUMENT.Count; j++)
                {
                    _context.R_File_Documents.RemoveRange(documentsToDelete[i].R_FILE_DOCUMENT); // 연결된 파일 삭제
                }
            }
           


            _context.Documents.RemoveRange(documentsToDelete);

            await _context.SaveChangesAsync();
            return Ok(new { success = true, deletedIds = common.OIDs });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }



    }

    // 문서 개정
    [HttpPut("/Document/Revision")]
    public async Task<IActionResult> RevisionDocument([FromBody] T_Document document)
    {
        if (ModelState.IsValid)
        {
            // 실제 데이터에 문서가 있는지 확인
            var documentCheck = await _context.Documents.FindAsync(document.OID);
            if (documentCheck == null)
            {
                return BadRequest("데이터가 없는 문서입니다.");
            }

            // 개정 전 최신 문서 확인
            if (documentCheck.RECENT == 0)
            {
                return BadRequest("이 문서는 최신문서가 아닙니다.");
            }

            // 최신 문서 변경
            documentCheck.RECENT = 0;
            _context.Entry(documentCheck).State = EntityState.Modified;
            

            // 개정된 문서 등록
            T_Document recentDocument = new T_Document();
            recentDocument.OID = Encryption.CreateRandomKey();
            recentDocument.TITLE = documentCheck.TITLE;
            recentDocument.CONTENT = documentCheck.CONTENT;
            recentDocument.TYPE = "DOCUMENT";
            recentDocument.CREATE_USER = HttpContext.Session.GetString("Username");
            recentDocument.CREATE_DATE = DateTime.Now;
            recentDocument.STATE = "작성중";
            recentDocument.REVISION = documentCheck.REVISION + 1;
            recentDocument.RECENT = 1;
            recentDocument.USEFLAG = '1';
            recentDocument.PREVOID = documentCheck.OID;
            recentDocument.DOC_CLASS_SEQ = documentCheck.DOC_CLASS_SEQ;

            _context.Objects.Add(recentDocument);
            _context.Documents.Add(recentDocument);

            await _context.SaveChangesAsync();


        }

        return Ok(new { success = true }); 
    }


    // 문서분류 등록
    [HttpPost("/DocumentClass")]
    public async Task<IActionResult> CreateDocumentClass(T_Document_Class docClass)
    {
        if (ModelState.IsValid)
        {
            _context.Document_Classes.Add(docClass);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Error: {dbEx.InnerException?.Message}");
                return StatusCode(500, "Database error occurred");
            }

            return Ok(new { message = "문서 분류 등록 완료" });
        }
        else
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }
        return BadRequest(new { success = false });
    }


    // 문서분류 수정
    [HttpPut("/DocumentClass/{SEQ}")]
    public async Task<IActionResult> PutDocumentClass(int SEQ, T_Document_Class docClass)
    {
        if (SEQ != docClass.SEQ)
        {
            return BadRequest();
        }

        try
        {
            var docClassCheck = await _context.Document_Classes.FindAsync(SEQ);

            if (docClassCheck == null)
            {
                return BadRequest();
            }

            _context.Entry(docClassCheck).State = EntityState.Modified;

            docClassCheck.NAME = docClass.NAME;
            docClassCheck.P_SEQ = docClass.P_SEQ;
            _context.Document_Classes.Update(docClassCheck);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }

        return Ok(new { message = "문서분류 편집 완료" });
    }



    // 문서분류 삭제
    [HttpDelete("/DocumentClass")]
    public async Task<IActionResult> DeleteDocumentClass(T_Document_Class docClass)
    {
        if (ModelState.IsValid)
        {
            var docClassCheck = _context.Document_Classes.FirstOrDefault(d => d.SEQ == docClass.SEQ);

            if (docClassCheck != null)
            {
                _context.Document_Classes.Remove(docClassCheck);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Error: {dbEx.InnerException?.Message}");
                return StatusCode(500, "Database error occurred");
            }

            return Ok(new { message = "문서 분류 삭제 완료" });
        }
        else
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }
        return BadRequest(new { success = false });
    }


    // 문서분류 전체 조회
    [HttpGet("/DocumentClass")]
    public async Task<ActionResult> GetDocumentClass()
    {
        var documentClass = await _context.Document_Classes.ToListAsync();
        if (documentClass == null)
        {
            return NotFound();
        }

        return Ok(documentClass);
    }


    // 문서분류 조회
    [HttpGet("/DocumentClass/{SEQ}")]
    public async Task<IActionResult> GetDocumentClass(int SEQ)
    {
        var documentClass = await _context.Document_Classes.FindAsync(SEQ);
        if (documentClass == null)
        {
            return NotFound();
        }

        return Ok(documentClass);
    }


    // 에러
    private bool DocumentExists(string OID)
    {
        return _context.Documents.Any(e => e.OID == OID);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

