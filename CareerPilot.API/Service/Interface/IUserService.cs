using CareerPilot.API.Models;
using System.Threading.Tasks;

namespace CareerPilot.API.Service.Interface
{
	public interface IUserService
	{
		Task<UserFileInfo?> GetUserFileInfoByIdAsync(int id);
	}
}
