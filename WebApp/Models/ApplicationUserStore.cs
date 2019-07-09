using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApp.Models
{
    public class ApplicationUserStore<TUser> : UserStore<TUser> where TUser : ApplicationUser
    {
        public ApplicationUserStore(DbContext context)
            :base(context)
        {
        }

        public int TenantId { get; set; }

        public override Task CreateAsync(TUser user)
        {
            if(user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.TenantId = this.TenantId;
            return base.CreateAsync(user); 
        }

        public override Task<TUser> FindByEmailAsync(string email)
        {
            return this.GetUserAggregateAsync(u => u.Email.ToUpper() == email.ToUpper() && u.TenantId == this.TenantId);
        }

        public override Task<TUser> FindByNameAsync(string userName)
        {
            return this.GetUserAggregateAsync(u => u.UserName.ToUpper() == userName.ToUpper() && u.TenantId == this.TenantId);
        }
    }
}