using Swe.MRP.BL.Model;

namespace Swe.MRP.BL.Abstractions.Repository;

public interface IMediaEntryRepository
{
    void Create(MediaEntry media);
    MediaEntry? GetById(int mediaId);
    void Update(int toUpdateId, MediaEntry mediaEdit);
    void Delete(int mediaEntryId);
}
