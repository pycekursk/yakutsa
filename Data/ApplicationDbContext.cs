using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using yakutsa.Models;

namespace yakutsa.Data
{
  public class ApplicationDbContext : IdentityDbContext<AppUser>
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseMySql("server=37.77.105.24;database=yakutsa;uid=pycek;password=6m7sd38L;ConvertZeroDateTime=True", new MySqlServerVersion(new Version(8, 0, 23)));
      base.OnConfiguring(optionsBuilder);
    }

    public void CreateSU()
    {
      AppUser pycek = new AppUser();
      pycek.Name = "Руслан";
      pycek.Email = "pycek@list.ru";
      pycek.UserName = pycek.Email;
      PasswordHasher<string> hasher = new PasswordHasher<string>();
      pycek.PasswordHash = hasher.HashPassword(pycek.UserName, "3339393");
      pycek.EmailConfirmed = true;
      pycek.NormalizedEmail = pycek.Email.ToUpper();
      pycek.NormalizedUserName = pycek.NormalizedEmail;

      IdentityRole guestRole = new IdentityRole("guest");
      IdentityRole adminRole = new IdentityRole("admin");
      IdentityRole managerRole = new IdentityRole("manager");
      IdentityRole clientRole = new IdentityRole("client");
      Roles.Add(guestRole);
      Roles.Add(clientRole);
      Roles.Add(managerRole);
      Roles.Add(adminRole);

      Users.Add(pycek);

      IdentityUserRole<string> adminUserRole = new IdentityUserRole<string> { RoleId = adminRole.Id, UserId = pycek.Id };

      UserRoles.Add(adminUserRole);

      SaveChanges();
    }

    public void CreateUser()
    {
      AppUser max = new AppUser();
      max.Name = "Максимилиан";
      max.Email = "maxkuts@yandex.ru";
      max.UserName = max.Email;
      PasswordHasher<string> hasher = new PasswordHasher<string>();
      max.PasswordHash = hasher.HashPassword(max.UserName, "3339393");
      max.EmailConfirmed = true;
      max.NormalizedEmail = max.Email.ToUpper();
      max.NormalizedUserName = max.NormalizedEmail;

      IdentityRole guestRole = new IdentityRole("guest");
      IdentityRole adminRole = new IdentityRole("admin");
      IdentityRole managerRole = new IdentityRole("manager");
      IdentityRole clientRole = new IdentityRole("client");
      Roles.Add(guestRole);
      Roles.Add(clientRole);
      Roles.Add(managerRole);
      Roles.Add(adminRole);

      Users.Add(max);

      IdentityUserRole<string> adminUserRole = new IdentityUserRole<string> { RoleId = managerRole.Id, UserId = max.Id };

      UserRoles.Add(adminUserRole);

      SaveChanges();
    }
  }
}