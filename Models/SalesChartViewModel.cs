using System.Collections.Generic;

namespace ArtGalleryFinal.Models
{
    public class SalesChartViewModel
    {
        public List<string> Months { get; set; } = new List<string>();
        public List<int> SalesAmounts { get; set; } = new List<int>();
        public int TotalSales { get; set; }
        public int TotalOrders { get; set; }
    }
}
