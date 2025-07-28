namespace CareerPilot.API.Service.Interface
{
	public interface IStorageService
	{
		Task<string> UploadFileAsync(Stream fileStream, string fileName);
		Task<Stream> GetFileByUrlAsync(string fileUrl);
	}
}
