using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DownloadMergedHtmlModel : PageModel
{
    public IActionResult OnGet(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return NotFound();
        }
      

        // Set the Content-Disposition header to make the browser download the file
        return PhysicalFile(fileName, "text/html", fileName);
    }
}
