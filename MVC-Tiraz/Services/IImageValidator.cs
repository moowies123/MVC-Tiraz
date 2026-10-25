namespace MVC_Tiraz.Services
{
    public interface IImageValidator
    {
        IEnumerable<string> Validate(IEnumerable<IFormFile> files);
    }
}
