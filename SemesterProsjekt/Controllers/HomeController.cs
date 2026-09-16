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
// NEED
    public IActionResult Need()
    {
        var model = new NeedViewModel();
        return View(model);
    }
    
    [HttpPost]
    public IActionResult Need(NeedViewModel model)
    {
        return RedirectToAction("NeedResult", model);
    }


    public IActionResult NeedResult(NeedViewModel model)
    {
        return View(model);
    }
    
//RESOURCE    
    public IActionResult Resource()
    {
        var model = new ResourceViewModel();
        return View(model);
    }

    [HttpPost]
    public IActionResult Resource(ResourceViewModel model)
    {
        return RedirectToAction("ResourceResult", model);
    }
    
    public IActionResult ResourceResult(ResourceViewModel model)
    {
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