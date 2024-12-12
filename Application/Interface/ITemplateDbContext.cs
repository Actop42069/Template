using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Domain.Entities;

namespace Application.Interface
{
    public interface ITemplateDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DatabaseFacade Database { get; }
        DbSet<ErrorLog> ErrorLog { get; set; }
    }
}
