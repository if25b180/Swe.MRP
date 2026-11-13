using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Abstractions.Repository;

public interface IUserRepository
{
    void Add(User user);
    void Remove(int id);
}
