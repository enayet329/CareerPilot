using CareerPilot.API.Service.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace CareerPilot.API.Service.Implementation
{
	public class StorageService : IStorageService
	{
		private readonly Cloudinary _cloudinary;

		public StorageService(Cloudinary cloudinary)
		{
			_cloudinary = cloudinary;
		}

		public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
		{
			if (fileStream.Length > 100 * 1024 * 1024)
				throw new ArgumentException("File size exceeds 100MB limit");

			var uploadParams = new RawUploadParams
			{
				File = new FileDescription(fileName, fileStream),
				PublicId = $"media/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}",
				Folder = "chatapp-media"
			};

			var uploadResult = await _cloudinary.UploadAsync(uploadParams);
			if (uploadResult.Error != null)
				throw new Exception($"Upload failed: {uploadResult.Error.Message}");

			return uploadResult.SecureUrl.ToString();
		}

		public async Task<Stream> GetFileByUrlAsync(string fileUrl)
		{
			if (string.IsNullOrWhiteSpace(fileUrl))
				throw new ArgumentException("File URL must not be empty.", nameof(fileUrl));

			using var httpClient = new HttpClient();
			var response = await httpClient.GetAsync(fileUrl);

			if (!response.IsSuccessStatusCode)
				throw new Exception($"Failed to retrieve file. Status code: {response.StatusCode}");

			var memoryStream = new MemoryStream();
			await response.Content.CopyToAsync(memoryStream);
			memoryStream.Position = 0;
			return memoryStream;
		}



	}
}
