using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.IRepository;
using ASP.NET_Hands_on.DatabseContext;
using ASP.NET_Hands_on.Domain.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET_Hands_on.Persistence.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;

        public CustomerRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken)
            => _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        public Task<int> CountAsync(CancellationToken cancellationToken)
            => _db.Customers.AsNoTracking().CountAsync(cancellationToken);

        public Task<List<CustomerDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 30;

            return _db.Customers
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerDto(c.Id, c.Name, c.Email, c.Gender))
                .ToListAsync(cancellationToken);
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken)
            => _db.Customers.AddAsync(customer, cancellationToken).AsTask();

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => _db.SaveChangesAsync(cancellationToken);

        public async Task<Customer?> GetByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken)
        {
            return await _db.Customers.FirstOrDefaultAsync(c => c.Username == username || c.Email == email, cancellationToken);
        }

        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken)
        {
            _db.Customers.Update(customer);
            return Task.CompletedTask;
        }

        public Task<int> CountOrdersByCustomerIdAsync(int customerId, CancellationToken cancellationToken)
            => _db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId).CountAsync(cancellationToken);

        public Task<List<Order>> GetOrdersByCustomerIdPagedAsync(int customerId, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 30;

            return _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderBy(o => o.OrderId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

    }
}
