namespace WebClient.Dto.Book
{
	public class BookRequest
	{
		public int BookId { get; set; }
		public string Title { get; set; }
		public string ISBN13 { get; set; }
		public int PublicationYear { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public int PublisherId { get; set; }

	}
	public class ODataResponse<T>
{
    public List<T> Value { get; set; }
}
}
