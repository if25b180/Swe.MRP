using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Abstractions.Repository;

public interface IUserRepository
{
    void Register(User user);
    string? Login(User user);
    void Remove(int id);
}
