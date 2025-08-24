namespace WebClient.Pages.Dto.Book
{
    public class BookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public int PublisherId { get; set; }
        public string ISBN13 { get; set; }
        public int PublicationYear { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public PublisherDto Publisher { get; set; }
    }
}
