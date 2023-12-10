using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace m21_e2_WEB.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        //контекст в клиентском приложении не нужен, но без него не будет работать SignInManager
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
