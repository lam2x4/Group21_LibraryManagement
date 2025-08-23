namespace WebClient.Dto
{
	public class ODataResponse<T>
	{
		public List<T> Value { get; set; }
	}
}
