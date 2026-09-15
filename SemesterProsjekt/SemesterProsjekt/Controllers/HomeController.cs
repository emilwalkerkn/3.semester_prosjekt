using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SemesterProsjekt.Models;

namespace SemesterProsjekt.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Need()
    {
        var model = new NeedViewModel();
        return View(model);
    }
    
    [HttpPost]
    public IActionResult Need(NeedViewModel model)
    {
        ViewData["Message"] = $"Behov registrert: {model.Type}, {model.Description}, {model.Location}";

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}