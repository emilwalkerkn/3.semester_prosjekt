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
    // GET: Viser skjemaet for registrering av et behov.
    public IActionResult Need()
    {
        var model = new NeedViewModel();
        return View(model);
    }
    
    // POST: Mottar informasjonen fra behovsskjemaet
    // og sender dataene videre til resultatsiden.
    [HttpPost]
    public IActionResult Need(NeedViewModel model)
    {
        return RedirectToAction("NeedResult", model);
    }


    public IActionResult NeedResult(NeedViewModel model)
    {
        return View(model);
    }
    
    // GET: Viser skjemaet for registrering av en ressurs. 
    public IActionResult Resource()
    {
        var model = new ResourceViewModel();
        return View(model);
    }

    // POST: Mottar informasjonen fra ressursskjemaet
    // og sender dataene videre til resultatsiden.
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