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
        private FileVersioner fileVersioner;

        public IActionResult Index(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                fileVersioner.GetChangesList(path);
            }
            return View();
        }
    }
}
