using Microsoft.AspNetCore.Hosting;
using SRMS.Application.Common.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FileStorageService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> SaveFileAsync(Stream content, string fileName, string folderName)
    {
        if (content == null)
            throw new ArgumentException("File content is empty");

        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await content.CopyToAsync(fileStream);
        }

        return $"/uploads/{folderName}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return Task.CompletedTask;

        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
