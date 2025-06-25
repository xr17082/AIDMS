using AIDMS.Shared.Application.Interfaces.Services;
using AIDMS.Shared.Constants.Permission;
using AIDMS.Shared.Constants.Role;
using AIDMS.Shared.Constants.User;
using AIDMS.Shared.Infrastructure.Contexts;
using AIDMS.Shared.Infrastructure.Helpers;
using AIDMS.Shared.Infrastructure.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AIDMS.Shared.Infrastructure
{
    public class DatabaseSeeder : IDataInitializer
    {
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DatabaseSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, AppDbContext db, ILogger<DatabaseSeeder> logger)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            AddAdministrator();
            AddBasicUser();
            _db.SaveChanges();
        }

        public void AddAdministrator()
        {
            Task.Run(async () =>
            {
                var adminRole = new ApplicationRole(RoleConstants.AdministratorRole, "Administrator Role with full permissions");
                var adminRoleInDB = await _roleManager.FindByNameAsync(RoleConstants.AdministratorRole);
                if (adminRoleInDB == null)
                {
                    await _roleManager.CreateAsync(adminRole);
                    adminRoleInDB = await _roleManager.FindByNameAsync(RoleConstants.AdministratorRole);
                    _logger.LogInformation("Administrator role created");
                }

                var adminUser = new ApplicationUser
                {
                    FirstName = "Administrator",
                    LastName = "",
                    Email = "admin@aidms.com",
                    UserName = "admin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                };

                var adminUserInDB = await _userManager.FindByNameAsync(adminUser.UserName);
                if(adminUserInDB == null)
                {
                    await _userManager.CreateAsync(adminUser, UserConstants.DefaultPassword);
                    var result = await _userManager.AddToRoleAsync(adminUser, RoleConstants.AdministratorRole);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Administrator user created");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError(error.Description);
                        }
                    }
                }

                foreach (var permission in Permissions.GetRegisteredPermissions())
                {
                    await _roleManager.AddPermissionClaim(adminRoleInDB, permission);
                }
            }).GetAwaiter().GetResult();
        }

        private void AddBasicUser()
        {
            Task.Run(async () =>
            {
                var basicRole = new ApplicationRole(RoleConstants.BasicRole, "Basic role with default permissions");
                var basicRoleInDB = await _roleManager.FindByNameAsync(RoleConstants.BasicRole);
                if (basicRoleInDB == null)
                {
                    await _roleManager.CreateAsync(basicRole);
                    _logger.LogInformation("Basic role created");
                }

                var basicUser = new ApplicationUser
                {
                    FirstName = "User",
                    LastName = "",
                    Email = "user@aidms.com",
                    UserName = "user",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedOn = DateTime.Now,
                    IsActive = true
                };

                var basicUserInDB = await _userManager.FindByNameAsync(basicUser.UserName);
                if (basicUserInDB == null)
                {
                    await _userManager.CreateAsync(basicUser, UserConstants.DefaultPassword);
                    await _userManager.AddToRoleAsync(basicUser, RoleConstants.BasicRole);
                    _logger.LogInformation("User with basic role created");
                }
            }).GetAwaiter().GetResult();
        }
    }
}
