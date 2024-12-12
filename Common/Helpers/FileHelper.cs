using Microsoft.AspNetCore.Http;

namespace Common.Helpers
{
    public static class FileHelper
    {
        private static readonly string _rootPath = GetProjectRootPath();

        public static async Task<string> ReadEmailTemplateAsync(string templateName, CancellationToken cancellationToken)
        {
            //Your template file should be in the debug\release folder. 
            string filePath = Path.Combine(_rootPath, $"EmailTemplates\\{templateName}");
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            string html = string.Empty;
            using (var sr = new StreamReader(filePath))
            {
                html = await sr.ReadToEndAsync();
            }

            return html;
        }

        public static async Task<byte[]> GetFileBytesAsync(this IFormFile formFile)
        {
            using var mstream = new MemoryStream();
            await formFile.CopyToAsync(mstream);
            var fileBytes = mstream.ToArray();

            return fileBytes;
        }

        public static string GetExtension(this IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension))
            {
                var contentType = file.ContentType;
                extension = $".{contentType.Split('/')[1]}";
            }
            return extension;
        }

        private static string GetProjectRootPath()
        {
            var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (currentDirectory != null && !currentDirectory.GetDirectories("Common").Any())
            {
                currentDirectory = currentDirectory.Parent;
            }
            if (currentDirectory == null)
                throw new FileNotFoundException();
            return Path.Combine(currentDirectory.FullName, "Common");
        }
    }
}
