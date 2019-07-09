using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WebApp.Models
{
    [Table("ASPNETTenants")]
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DomainName { get; set; }
        public bool Default { get; set; }
    }

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public int TenantId { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Tenant> Tenants { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var user = modelBuilder.Entity<ApplicationUser>();

            user.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(
                    new IndexAttribute("UserNameIndex") { IsUnique = true, Order = 1 }));

            user.Property(u => u.TenantId)
                .IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(
                    new IndexAttribute("UserNameIndex") { IsUnique = true, Order = 2 }));
        }

        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
        {
            if (entityEntry != null && entityEntry.State == EntityState.Added)
            {
                var errors = new List<DbValidationError>();
                var user = entityEntry.Entity as ApplicationUser;

                if (user != null)
                {
                    if (this.Users.Any(u => string.Equals(u.UserName, user.UserName) && u.TenantId == user.TenantId))
                    {
                        errors.Add(new DbValidationError("User",
                            string.Format("Username {0} is already taken for AppId {1}", user.UserName, user.TenantId)));
                    }

                    if(this.RequireUniqueEmail && this.Users.Any(u => string.Equals(u.Email, user.Email) && u.TenantId == user.TenantId))
                    {
                        errors.Add(new DbValidationError("User",
                            string.Format("Email Address {0} is already taken for AppId {1}", user.UserName, user.TenantId)));
                    }
                }
                else
                {

                }
            }
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}