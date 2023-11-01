using VideoGuide.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoGuide.IRepository;
using ASU_Research_2022.Repository;

namespace VideoGuide.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VideoGuidDbContext _context;
        private IGenericRepository<ApplicationUser> _ApplicationUser;
        
        public UnitOfWork(VideoGuidDbContext context)
        {
            _context = context;
        }
        public IGenericRepository<ApplicationUser> ApplicationUser => _ApplicationUser ??= new GenericRepository<ApplicationUser>(_context);
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}