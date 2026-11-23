using Microsoft.EntityFrameworkCore;
using Mnemo.Data.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Data.Queries
{
    public class GlossaryQueries
    {
        private AppDbContext _context;


        public GlossaryQueries(AppDbContext context)
        {
            _context = context;
        }


        // Queries
        public IQueryable<Glossary> GetByOwnerIdQuery(int ownerId)
            => _context.Glossaries.Where(p => p.OwnerId == ownerId);

        public IQueryable<Glossary> GetByIdQuery(int ownerId, int glossId)
            => _context.Glossaries.Where(p => p.OwnerId == ownerId && p.Id == glossId);

        public IQueryable<Glossary> GetByGuidQuery(int ownerId, Guid guid)
            => _context.Glossaries.Where(p => p.OwnerId == ownerId && p.Guid == guid);

        public IQueryable<Glossary> GetByEntryIdQuery(int ownerId, int entryId)
            => _context.GlossaryLinks.Where(l => l.Glossary.OwnerId == ownerId && l.EntryId == entryId).Select(l => l.Glossary);


        // Getters
        public async Task<bool> ExistsByGuidAsync(int ownerId, Guid guid)
            => await GetByGuidQuery(ownerId, guid).AnyAsync();

        public async Task<Glossary?> GetByGuidAsync(int ownerId, Guid guid)
            => await GetByGuidQuery(ownerId, guid).FirstOrDefaultAsync();

        public async Task<int?> GetIdByGuidAsync(int ownerId, Guid guid)
            => await GetByGuidQuery(ownerId, guid).Select(v => v.Id).FirstOrDefaultAsync();

        public async Task<bool> ExistsByIdAsync(int ownerId, int glossId)
            => await GetByIdQuery(ownerId, glossId).AnyAsync();

        public async Task<Glossary?> GetByIdAsync(int ownerId, int glossId)
            => await GetByIdQuery(ownerId, glossId).FirstOrDefaultAsync();

        public async Task<List<Glossary>> GetByEntryIdAsync(int ownerId, int entryId)
            => await GetByEntryIdQuery(ownerId, entryId).ToListAsync();
    }
}
