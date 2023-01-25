using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using ProjetoOCR.Models;
using System.Diagnostics;
using Tesseract;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace ProjetoOCR.Controllers
{
    public static class FormFileExtensions
    {
        public static byte[] GetBytes(IFormFile formFile)
        {
            using var memoryStream = new MemoryStream();
            formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CarregarAnexo(IFormFile anexo)
        {
            var file = FormFileExtensions.GetBytes(anexo);
            byte[] png = Freeware.Pdf2Png.Convert(file, 1);
            using (var engine = new TesseractEngine(@"./tessdata", "por", EngineMode.Default))
            {
                using (var img = Pix.LoadFromMemory(png))
                {
                    using (var page = engine.Process(img))
                    {
                        var text = page.GetText();
                        TempData["text"] = text;
                        Console.WriteLine("Mean confidence: {0}", page.GetMeanConfidence());

                        Console.WriteLine("Text (GetText): \r\n{0}", text);
                        Console.WriteLine("Text (iterator):");
                    }
                }
            }

            return RedirectToAction("Index");
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
}