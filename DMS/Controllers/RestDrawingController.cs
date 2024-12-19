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
using Azure.Core;
using System.Linq;

namespace DMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RestDrawingController : ControllerBase
{

    private readonly ApplicationDbContext _context;

    public RestDrawingController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET : api/RestDrawing
    [HttpGet]
    public async Task<IActionResult> GetDrawing()
    {
        var view = await _context.Drawings
            .Where(drawing => drawing.TYPE == "DRAWING")
            .ToListAsync();

        return Ok(view);
    }


    // GET : api/RestDrawing/{OID}
    [HttpGet("{OID}")]
    public async Task<IActionResult> GetDrawingDetail(string OID)
    {
        var detail = await _context.Drawings.FindAsync(OID);

        if (detail == null)
        {
            return NotFound();
        }

        return Ok(detail);
    }


    // POST : api/RestDrawing
    [HttpPost]
    public async Task<IActionResult> CreateDrawing(T_Drawing drawing)
    {
        if (ModelState.IsValid)
        {
            drawing.OID = CreateRandomKey();
            _context.Drawings.Add(drawing);
            _context.Objects.Add(drawing);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
        return BadRequest(new { success = false });
    }

    //PUT: api/RestDrawing/{OID}
    [HttpPut("{OID}")]
    public async Task<IActionResult> PutDrawing(string OID, T_Drawing drawing)
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

    //DELETE: api/RestDrawing/{OIDs}
    [HttpDelete]
    public async Task<IActionResult> DeleteDrawing(Common common)
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


    public string CreateRandomKey()
    {
        return Guid.NewGuid().ToString();
    }
}

