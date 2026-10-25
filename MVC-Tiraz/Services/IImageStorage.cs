namespace MVC_Tiraz.Services
{
    public interface IImageStorage
    {
        Task<List<string>> SaveAsync(IEnumerable<IFormFile> files);
        void Delete(IEnumerable<string> urls);
    }
}
