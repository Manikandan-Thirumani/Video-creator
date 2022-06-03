using Converters.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonLibrary;
using Microsoft.AspNetCore.Http;

namespace Converters.web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //FileConversion fc = new FileConversion();
            //fc.pdftoword("C:\\Users\\mthirumani\\Downloads\\sample.pdf");
            return View();
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
        public IActionResult Converter()
        {
            return View("Converter");
        }
        [HttpPost, ActionName("FileUpload")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files.Count == 0)
            {
                TempData["error"] = "Please choose file to convert";
                return View("Converter");
            }
            var uploadsfolderpath = Directory.GetCurrentDirectory() + "\\Uploads";
            var outputfolderfolderpath = Directory.GetCurrentDirectory() + "\\Output";
            string filePath = "", outputfile = "", filename="";
            createFolder(uploadsfolderpath);
            createFolder(outputfolderfolderpath);
            long size = files.Sum(f => f.Length);

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    filename = Path.GetFileNameWithoutExtension(formFile.FileName);
                     filePath = uploadsfolderpath + formFile.FileName;
                    filePaths.Add(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            using (var fc = new FileConversion())
            {
                outputfile = outputfolderfolderpath + "\\" + filename + ".docx";
                fc.pdftoword(filePath, outputfile);
            }
            // process uploaded files
            // Don't rely on or trust the FileName property without validation.
           // return Ok(new { outputfile });
           TempData["filetodownoad"] = outputfile;
           return View("Converter");
        }

        private void createFolder(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
            }
        }
        public FileResult Download(string file)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            var response = new FileContentResult(fileBytes, "application/octet-stream");
            response.FileDownloadName = Path.GetFileName(file);
            return response;
        }
    }
}
