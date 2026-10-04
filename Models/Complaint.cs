namespace ArtGalleryFinal.Models;

// A complaint sent by a customer (optionally about one artwork)
public class Complaint
{
    public int ComplaintId { get; set; }
    public int? CustomerId { get; set; }
    public int? ArtId { get; set; }
    public string Message { get; set; } = "";
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
