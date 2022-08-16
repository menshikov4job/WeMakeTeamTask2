using Microsoft.EntityFrameworkCore;

namespace WeMakeTeamTask2
{
    public class EntityRepository
    {
        readonly AppDbContext _context;

        public EntityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Entity?> GetAsync(Guid id)
        {
            return await _context.Entities.FindAsync(id);
        }

        public async Task<int> InsertAsync(Entity entity)
        {
            _context.Entities.Add(entity);
            return await _context.SaveChangesAsync();           
        }
    }
}
