using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Identity.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IdentityDbContext context, string? adminPasswordHash = null)
        {
            if (await context.Roles.AnyAsync())
            {
                return;
            }

            var roles = SeedRoles();
            var permissions = SeedPermissions();

            await context.Roles.AddRangeAsync(roles);
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            var rolePermissions = SeedRolePermissions(roles, permissions);
            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(adminPasswordHash))
            {
                var adminRole = roles.First(r => r.Name == "Admin");
                var adminUser = User.Create(
                    Guid.NewGuid(),
                    PersonName.Create("System", "Admin"),
                    Email.Create("admin@clinicdental.com"),
                    Username.Create("admin"),
                    adminPasswordHash);

                adminUser.AssignRole(adminRole);
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }

        private static List<Role> SeedRoles()
        {
            return
            [
                Role.Create(1, "Admin", "Full system access"),
                Role.Create(2, "Dentist", "Dental practitioner"),
                Role.Create(3, "Patient", "Clinic patient"),
                Role.Create(4, "Receptionist", "Front desk staff"),
            ];
        }

        private static List<Permission> SeedPermissions()
        {
            return
            [
                Permission.Create(1, "users.read", "Users", "View user details"),
                Permission.Create(2, "users.write", "Users", "Create and update users"),
                Permission.Create(3, "users.delete", "Users", "Delete users"),

                Permission.Create(4, "patients.read", "Patients", "View patient records"),
                Permission.Create(5, "patients.write", "Patients", "Create and update patients"),
                Permission.Create(6, "patients.delete", "Patients", "Delete patients"),

                Permission.Create(7, "appointments.read", "Appointments", "View appointments"),
                Permission.Create(8, "appointments.write", "Appointments", "Create and update appointments"),
                Permission.Create(9, "appointments.delete", "Appointments", "Delete appointments"),

                Permission.Create(10, "billing.read", "Billing", "View billing information"),
                Permission.Create(11, "billing.write", "Billing", "Create and update invoices"),
                Permission.Create(12, "billing.delete", "Billing", "Delete invoices"),

                Permission.Create(13, "treatments.read", "Treatments", "View treatment plans"),
                Permission.Create(14, "treatments.write", "Treatments", "Create and update treatment plans"),
                Permission.Create(15, "treatments.delete", "Treatments", "Delete treatment plans"),
            ];
        }

        private static List<RolePermission> SeedRolePermissions(List<Role> roles, List<Permission> permissions)
        {
            var admin = roles.First(r => r.Name == "Admin");
            var dentist = roles.First(r => r.Name == "Dentist");
            var patient = roles.First(r => r.Name == "Patient");
            var receptionist = roles.First(r => r.Name == "Receptionist");

            var all = permissions.ToList();

            var dentistPerms = permissions
                .Where(p => p.Category is "Patients" or "Appointments" or "Treatments")
                .ToList();

            var receptionistPerms = permissions
                .Where(p => p is { Category: "Patients", Name: "patients.read" or "patients.write" }
                    || p is { Category: "Appointments", Name: "appointments.read" or "appointments.write" })
                .ToList();

            var patientPerms = permissions
                .Where(p => p is { Category: "Patients", Name: "patients.read" }
                    || p is { Category: "Appointments", Name: "appointments.read" })
                .ToList();

            return MapRolePermissions(all, admin)
                .Concat(MapRolePermissions(dentistPerms, dentist))
                .Concat(MapRolePermissions(receptionistPerms, receptionist))
                .Concat(MapRolePermissions(patientPerms, patient))
                .ToList();
        }

        private static IEnumerable<RolePermission> MapRolePermissions(List<Permission> perms, Role role)
        {
            return perms.Select(p => RolePermission.Create(default, role.Id, p.Id));
        }
    }
}
