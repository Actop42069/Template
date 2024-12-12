using Application.Interface;
using Common.Interfaces;
using Domain.Entities;
using Domain.Events;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistance
{
    public class TemplateDbContext : IdentityDbContext<User,
                                                  Role,
                                                  string,
                                                  IdentityUserClaim<string>,
                                                  UserRole,
                                                  IdentityUserLogin<string>,
                                                  IdentityRoleClaim<string>,
                                                  IdentityUserToken<string>>,
                                                  ITemplateDbContext
    {
        private readonly IEventDispatcherService _eventDispatcherService;

        public TemplateDbContext(DbContextOptions<TemplateDbContext> options,
                            IEventDispatcherService eventDispatcherService) : base(options)
        {
            _eventDispatcherService = eventDispatcherService;
        }

        public DbSet<ErrorLog> ErrorLog { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // always run OnModelCreating before running custom configuration to avoid overwrite of custom navigation on identity user
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            var dbInitializer = new DbInitializer(builder);
            dbInitializer.SeedUserAndRole();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                QueueDomainEvents();
                return base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                if (_eventDispatcherService != null)
                {
                    _eventDispatcherService.ClearQueue();
                }

                Console.WriteLine(ex);
                throw;
            }
        }

        private void QueueDomainEvents()
        {
            var addedEntities = ChangeTracker.Entries<ICreatedEvent>().Where(w => w.State == EntityState.Added);
            foreach (var addedEntity in addedEntities)
            {
                var entity = new CreatedEvent(addedEntity.Entity);
                _eventDispatcherService.QueueNotification(entity);
            }

            var updatedEntities = ChangeTracker.Entries<IUpdatedEvent>().Where(w => w.State == EntityState.Modified);
            foreach (var updatedEntity in updatedEntities)
            {
                var entity = new UpdatedEvent(updatedEntity.Entity);
                _eventDispatcherService.QueueNotification(entity);
            }
        }
    }
}
