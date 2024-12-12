namespace Application.Interface
{
    public interface IFileService
    {
        Task SaveFile(string fileName, Stream fileStream);
        Task DeleteFile(string fileName);
        Task<byte[]> GetFile(string fileName);
    }
}
