using Microsoft.AspNetCore.Mvc;
using Markdig;

namespace SupertronicsRepairSystem.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            // Load the markdown file from wwwroot/manual.md
            var markdownPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "manual.md");
            if (System.IO.File.Exists(markdownPath))
            {
                var markdown = System.IO.File.ReadAllText(markdownPath);
                var html = Markdown.ToHtml(markdown);
                ViewBag.ManualHtml = html;
            }
            else
            {
                ViewBag.ManualHtml = "<p>Manual not found.</p>";
            }
            return View();
        }
    }
}