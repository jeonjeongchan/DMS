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
using System.Linq;

namespace DMS.Controllers;

public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;

    public MenuController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 메뉴 생성
    public JsonResult CreateMenu()
    {
        List<T_Menu> MenuList = new List<T_Menu>();

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                MenuList = _context.Menus.ToList();
                transaction.Commit();

            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        return Json(MenuList);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

