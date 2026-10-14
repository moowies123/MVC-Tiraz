namespace MVC_Tiraz.ViewModels
{
    public class DashboardVM
    {
        public decimal TotalSales { get; set; }
        public int TotalVisitors { get; set; }
        public int TotalSold { get; set; }
        public List<Product> Product { get; set; } = new();
    }
}
