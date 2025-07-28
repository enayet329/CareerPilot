using CareerPilot.API.CareerPIlotDbContext;
using CareerPilot.API.Service.Interface;

namespace CareerPilot.API.Service.Implementation
{
	public class UserService : IUserService
	{
		private readonly CareerPilotDbContext _dbContext;
		public UserService(CareerPilotDbContext careerPilotDbContext)
		{
			_dbContext = careerPilotDbContext ?? throw new ArgumentNullException(nameof(careerPilotDbContext));
		}
	}
}
