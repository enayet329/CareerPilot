using System.ComponentModel.DataAnnotations;

namespace CareerPilot.API.Models
{
	public class User
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public virtual ICollection<UserFileInfo> UserFiles { get; set; }
	}

}

