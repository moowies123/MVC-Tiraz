namespace MVC_Tiraz.Services
{
    public class ImageValidator : IImageValidator
    {
        private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp", ".jfif" };
        private const long MaxBytes = 2 * 1024 * 1024;

        public IEnumerable<string> Validate(IEnumerable<IFormFile> files)
        {
            foreach (var file in files.Where(f => f.Length > 0))
            {
                if (!Allowed.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                    yield return $"\"{file.FileName}\" must be a .jpg, .png or .webp file.";

                if (file.Length > MaxBytes)
                    yield return $"\"{file.FileName}\" must be 2 MB or smaller.";
            }
        }
    }
}
