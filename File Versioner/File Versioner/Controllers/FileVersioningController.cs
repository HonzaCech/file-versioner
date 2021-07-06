using FileVersioningTool.FileVersioningBackend;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileVersioningTool.Controllers
{
    public class FileVersioningController : Controller
    {
        private readonly IFileVersioner _fileVersioner;

        public FileVersioningController(IFileVersioner fileVersioner)
        {
            _fileVersioner = fileVersioner;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> Results(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                var results = await _fileVersioner.GetChangesListAsync(path);
                return View(results);
            }
            return View();
            
        }
    }
}
