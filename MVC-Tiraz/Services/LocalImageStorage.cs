namespace MVC_Tiraz.Services
{
    public class LocalImageStorage : IImageStorage
    {
        private readonly IWebHostEnvironment _env;

        public LocalImageStorage(IWebHostEnvironment env) => _env = env;

        public async Task<List<string>> SaveAsync(IEnumerable<IFormFile> files)
        {
            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            var urls = new List<string>();

            foreach (var file in files.Where(f => f.Length > 0))
            {
                var name = Guid.NewGuid() + Path.GetExtension(file.FileName);

                using var stream = new FileStream(Path.Combine(folder, name), FileMode.Create);
                await file.CopyToAsync(stream);

                urls.Add("/images/products/" + name);
            }

            return urls;
        }

        public void Delete(IEnumerable<string> urls)
        {
            foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
            {
                var path = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
