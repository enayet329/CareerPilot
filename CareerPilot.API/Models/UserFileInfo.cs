namespace CareerPilot.API.Models
{
	public class UserFileInfo
	{
		public int Id { get; set; }
		public string UserId { get; set; } // Foreign key

		public string FileName { get; set; }
		public string FileUrl { get; set; }
		public DateTime UploadedAt { get; set; }
		public string FileType { get; set; } // e.g., "resume", "cover letter", etc.

		public User User { get; set; }
	}
}
