using VideoGuide.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoGuide.IRepository;

namespace VideoGuide.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<ApplicationUser> ApplicationUser { get; }
        
        Task Save();
    }
}
