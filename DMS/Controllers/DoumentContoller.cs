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


namespace DMS.Controllers;


public class DocumentController : Controller
{

    private readonly ApplicationDbContext _context;

    public DocumentController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/Document")]
    public async Task<IActionResult> DocumentMain()
    {
        return View(await _context.Documents
            .Where(document => document.TYPE == "DOCUMENT")
            .OrderByDescending(document => document.CREATE_DATE)
            .ToListAsync());
    }


    [HttpGet("/Document/{OID}")]
    public async Task<IActionResult> GetDocumentDetail(string OID)
    {
        var detail = await _context.Documents.FindAsync(OID);
        var fileList = new List<T_File>();
        string sql = "SELECT * FROM JJC.T_FILE WHERE SEQ = :SEQ";

        if (detail.T_FILE_SEQ != null) {


            //string connectionString = _context.Database.GetDbConnection().ConnectionString;
            string connectionString = "Data Source=211.244.81.163:9090/ORCL; User Id=JJC; Password=Qwer1234;";


            using (var connection = new OracleConnection(connectionString))
            {
                await connection.OpenAsync();

                //using (var command = connection.CreateCommand())
                using (var command = new OracleCommand(sql, connection))
                {
                    command.Parameters.Add(new OracleParameter("SEQ", OracleDbType.Int32) { Value = detail.T_FILE_SEQ });

                    // 데이터 읽기
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var data = new T_File
                            {
                                SEQ = reader.GetInt32(0), // 첫 번째 컬럼
                                FILE_NAME = reader.GetString(1), // 두 번째 컬럼
                                FILE_SIZE = reader.GetInt32(2), // 세 번째 컬럼
                                FILE_PATH = reader.GetString(4)
                            };
                            fileList.Add(data);
                        }
                    }
                }
                detail.file_list = fileList;
            }

        }

        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }


    [HttpPost("/Document")]
    public async Task<IActionResult> CreateDocument(T_Document document)
    {
        if (ModelState.IsValid)
        {

            document.OID = Encryption.CreateRandomKey();
            document.TYPE = "DOCUMENT";
            document.CREATE_DATE = DateTime.Now;
            document.STATE = "작성중";
            document.REVISION = 0;
            document.USEFLAG = '1';


            if (document.T_File != null)
            {
                if (document.T_File.Length == 0)
                {
                    return BadRequest(new { Message = "용량이 빈파일은 등록할수 없습니다." });
                }
                else
                {
                    //DateTime dateTime = DateTime.UtcNow;
                    //long timestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

                    //var fileName = Path.GetFileNameWithoutExtension(document.T_File.FileName);
                    //fileName = fileName + "_" + timestamp;
                    //fileName = fileName + Path.GetExtension(document.T_File.FileName);
                    
                    var fileName = Path.GetFileName(document.T_File.FileName);
                    var uploadPath = Path.Combine("wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await document.T_File.CopyToAsync(stream);
                    }

                    var fileData = new T_File
                    {
                        FILE_NAME = fileName,
                        FILE_PATH = uploadPath,
                        FILE_SIZE = document.T_File.Length

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
                            document.T_FILE_SEQ = sequenceValue;

                        }
                    }

                    _context.Files.Add(fileData);

                }


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


    [HttpPut("/Document/{OID}")]
    public async Task<IActionResult> PutDocument(string OID, T_Document document)
    {
        if (OID != document.OID)
        {
            return BadRequest();
        }
        _context.Entry(document).State = EntityState.Modified;

        try
        {

            document.TYPE = "DOCUMENT";
            document.MODIFY_DATE = DateTime.Now;
            document.STATE = "작성중";
            document.REVISION = 0;
            document.USEFLAG = '1';

            if (document.T_File != null)
            {
                if (document.T_File.Length == 0)
                {
                    return BadRequest(new { Message = "용량이 빈파일은 등록할수 없습니다." });
                }
                else
                {
                    //DateTime dateTime = DateTime.UtcNow;
                    //long timestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

                    //var fileName = Path.GetFileNameWithoutExtension(document.T_File.FileName);
                    //fileName = fileName + "_" + timestamp;
                    //fileName = fileName + Path.GetExtension(document.T_File.FileName);

                    var fileName = Path.GetFileName(document.T_File.FileName);
                    var uploadPath = Path.Combine("wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await document.T_File.CopyToAsync(stream);
                    }

                    var fileData = new T_File
                    {
                        FILE_NAME = fileName,
                        FILE_PATH = uploadPath,
                        FILE_SIZE = document.T_File.Length,
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
                            document.T_FILE_SEQ = sequenceValue;

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
                .ToListAsync();

            if (documentsToDelete.Count == 0)
            {
                return NotFound("No matching items found.");
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

