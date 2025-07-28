using CareerPilot.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.API.CareerPIlotDbContext
{
	public class CareerPilotDbContext : DbContext
	{
		public CareerPilotDbContext(DbContextOptions<CareerPilotDbContext> options) : base(options)
		{
		}

		
		public DbSet<User> Users { get; set; }
		public DbSet<UserFileInfo> UserFiles { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<UserFileInfo>()
				.HasOne(uf => uf.User)
				.WithMany(u => u.UserFiles)
				.HasForeignKey(uf => uf.UserId)
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();

			modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareerPilotDbContext).Assembly);
		}

	}
}
