using ASP.NET_Hands_on.Domain.Model;
using ASP.NET_Hands_on.Application.DTO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET_Hands_on.Application.IRepository
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
        Task<List<CustomerDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task AddAsync(Customer customer, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
        Task<Customer?> GetByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken);
        Task UpdateAsync(Customer customer, CancellationToken cancellationToken);
        Task<int> CountOrdersByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
        Task<List<Order>> GetOrdersByCustomerIdPagedAsync(int customerId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    }
}
