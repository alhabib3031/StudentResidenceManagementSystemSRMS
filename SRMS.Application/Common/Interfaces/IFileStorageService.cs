namespace SRMS.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the storage and returns the relative path.
    /// </summary>
    /// <param name="content">File content bytes</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="folderName">Target folder (profile, certificates, etc.)</param>
    /// <returns>Relative path to the saved file</returns>
    Task<string> SaveFileAsync(System.IO.Stream content, string fileName, string folderName);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="relativePath">Relative path to the file</param>
    Task DeleteFileAsync(string relativePath);
}
