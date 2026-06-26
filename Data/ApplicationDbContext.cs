using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomerList.Data;

// Identity needs a DbContext to persist users, roles, and the
// relationships between them. Inheriting from IdentityDbContext
// gives us all the standard Identity tables (Users, Roles, UserRoles,
// etc.) without having to define them ourselves.
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}