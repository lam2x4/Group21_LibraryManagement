namespace WebClient.Pages.Dto.Book
{
    public class BooksODataResponse
    {
        public string ODataContext { get; set; }
        public List<BookDto> Value { get; set; } = new List<BookDto>();
    }
}
