namespace CareerPilot.API.Models
{
	public class UserFileInfo
	{
		public int Id { get; set; }
		public int UserId { get; set; } // Foreign key

		public string FileName { get; set; }
		public string FileUrl { get; set; }
		public string ResumeText { get; set; } // Parsed text from the resume
		public DateTime UploadedAt { get; set; }
		public string FileType { get; set; } // e.g., "resume", "cover letter", etc.

		public User User { get; set; }
	}
}
