using CareerPilot.API.CareerPIlotDbContext;
using CareerPilot.API.Models;
using CareerPilot.API.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.API.Service.Implementation
{
	public class UserService : IUserService
	{
		private readonly CareerPilotDbContext _dbContext;

		public UserService(CareerPilotDbContext careerPilotDbContext)
		{
			_dbContext = careerPilotDbContext ?? throw new ArgumentNullException(nameof(careerPilotDbContext));
		}

		// User CRUD

		public async Task<User> CreateUserAsync(User user)
		{
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();
			return user;
		}

		public async Task<User?> GetUserByIdAsync(int id)
		{
			return await _dbContext.Users
				.Include(u => u.UserFiles)
				.FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task<List<User>> GetAllUsersAsync()
		{
			return await _dbContext.Users
				.Include(u => u.UserFiles)
				.ToListAsync();
		}

		public async Task<bool> UpdateUserAsync(User user)
		{
			var existingUser = await _dbContext.Users.FindAsync(user.Id);
			if (existingUser == null)
				return false;

			existingUser.UserName = user.UserName;
			existingUser.Email = user.Email;
			existingUser.PasswordHash = user.PasswordHash;
			existingUser.ProfilePictureUrl = user.ProfilePictureUrl;

			_dbContext.Users.Update(existingUser);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteUserAsync(int id)
		{
			var user = await _dbContext.Users.FindAsync(id);
			if (user == null)
				return false;

			_dbContext.Users.Remove(user);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		// UserFileInfo CRUD

		public async Task<UserFileInfo> CreateUserFileInfoAsync(UserFileInfo userFileInfo)
		{
			_dbContext.UserFiles.Add(userFileInfo);
			await _dbContext.SaveChangesAsync();
			return userFileInfo;
		}

		public async Task<UserFileInfo?> GetUserFileInfoByIdAsync(int id)
		{
			return await _dbContext.UserFiles
				.Include(uf => uf.User)
				.FirstOrDefaultAsync(uf => uf.Id == id);
		}

		public async Task<List<UserFileInfo>> GetUserFileInfosByUserIdAsync(int userId)
		{
			return await _dbContext.UserFiles
				.Where(uf => uf.UserId == userId)
				.Include(uf => uf.User)
				.ToListAsync();
		}

		public async Task<bool> UpdateUserFileInfoAsync(UserFileInfo userFileInfo)
		{
			var existingFile = await _dbContext.UserFiles.FindAsync(userFileInfo.Id);
			if (existingFile == null)
				return false;

			existingFile.FileName = userFileInfo.FileName;
			existingFile.FileUrl = userFileInfo.FileUrl;
			existingFile.ResumeText = userFileInfo.ResumeText;
			existingFile.UploadedAt = userFileInfo.UploadedAt;
			existingFile.FileType = userFileInfo.FileType;
			existingFile.UserId = userFileInfo.UserId;

			_dbContext.UserFiles.Update(existingFile);
			await _dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteUserFileInfoAsync(int id)
		{
			var fileInfo = await _dbContext.UserFiles.FindAsync(id);
			if (fileInfo == null)
				return false;

			_dbContext.UserFiles.Remove(fileInfo);
			await _dbContext.SaveChangesAsync();
			return true;
		}
	}
}
