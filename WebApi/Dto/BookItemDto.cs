using System.ComponentModel.DataAnnotations;

namespace WebApi.Dto
{
    public class BookItemDto
    {
        public int ItemId { get; set; }
        public int BookId { get; set; }
        public string Barcode { get; set; }
        public string Status { get; set; }
        public string? ShelfLocation { get; set; }
        
        // Book information (flattened to avoid circular reference)
        public string BookTitle { get; set; }
        public string BookISBN13 { get; set; }
        public int BookPublicationYear { get; set; }
        public string? BookImageUrl { get; set; }
        public string? BookDescription { get; set; }
        public string PublisherName { get; set; }
    }
}
