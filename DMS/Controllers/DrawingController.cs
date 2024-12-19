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

namespace DMS.Controllers;


public class DrawingController : Controller
{

    private readonly ApplicationDbContext _context;

    public DrawingController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/Drawing")]
    public async Task<IActionResult> DrawingMain()
    {
        return View(await _context.Drawings
            .Where(drawing => drawing.TYPE == "DRAWING")
            .OrderByDescending(drawing => drawing.CREATE_DATE)
            .ToListAsync());
    }

    //public IActionResult DrawingMainPatital()
    //{
    //    var list = _context.Drawings
    //        .Where(drawing => drawing.TYPE == "DRAWING")
    //        .OrderByDescending(drawing => drawing.CREATE_DATE)
    //        .ToList();

    //    return PartialView("DrawingMain", list);
    //}

    [HttpGet("/Drawing/{OID}")]
    public async Task<IActionResult> GetDrawingDetail(string OID)
    {
        var detail = await _context.Drawings.FindAsync(OID);

        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }



    //[HttpPost("Drawing")]
    //public async Task<IActionResult> CreateDrawing([FromBody] Drawing drawing)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        drawing.OID = Encryption.CreateRandomKey();
    //        _context.Drawings.Add(drawing);
    //        _context.Objects.Add(drawing);
    //        await _context.SaveChangesAsync();
    //        return Ok(new { success = true });
    //    }
    //    return BadRequest(new { success = false });
    //}

    [HttpPost("/Drawing")]
    public async Task<IActionResult> CreateDrawing([FromForm] T_Drawing drawing)
    {
        if (ModelState.IsValid)
        {
            
            drawing.OID = Encryption.CreateRandomKey();
            drawing.TYPE = "DRAWING";
            drawing.CREATE_DATE = DateTime.Now;
            drawing.STATE = "작성중";
            drawing.REVISION = 0;
            drawing.USEFLAG = '1';

            _context.Objects.Add(drawing);
            _context.Drawings.Add(drawing);
            if (drawing.T_File != null)
            {
                if (drawing.T_File.Length == 0)
                {
                    return BadRequest(new { Message = "용량이 빈파일은 등록할수 없습니다." });
                }
                else
                {
                    var fileName = Path.GetFileName(drawing.T_File.FileName);
                    var uploadPath = Path.Combine("wwwroot", "uploads", fileName);

                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await drawing.T_File.CopyToAsync(stream);
                    }


                    var fileData = new T_File
                    {
                        FILE_NAME = drawing.T_File.FileName,
                        FILE_PATH = uploadPath,
                        FILE_SIZE = drawing.T_File.Length

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


                        }
                    }


                    _context.Files.Add(fileData);

                }


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
            // 처리 로직
            //return Json(new { success = true, message = "File uploaded successfully" });
            return Ok(new { message = "도면 등록 완료" });
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


    [HttpPut("/Drawing/{OID}")]
    public async Task<IActionResult> PutDrawing(string OID, [FromBody] T_Drawing drawing)
    {
        if (OID != drawing.OID)
        {
            return BadRequest();
        }
        _context.Entry(drawing).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DrawingExists(OID))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("/Drawing/Delete")]
    public async Task<IActionResult> DeleteDrawing([FromBody] Common common)
    {
        if (common.OIDs == null || !common.OIDs.Any())
        {
            return BadRequest();
        }

        try
        {
            var drawingsToDelete = await _context.Drawings
                .Where(d => common.OIDs.Contains(d.OID))
                .ToListAsync();

            if (drawingsToDelete.Count == 0)
            {
                return NotFound("No matching items found.");
            }

            _context.Drawings.RemoveRange(drawingsToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, deletedIds = common.OIDs });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }



    }


    private bool DrawingExists(string OID)
    {
        return _context.Drawings.Any(e => e.OID == OID);
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

