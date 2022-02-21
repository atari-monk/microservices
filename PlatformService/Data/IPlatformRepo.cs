using PlatformService.Models;

namespace PlatformService.Data;

public interface IPlatformRepo
{
    bool SaveChanges();

    IEnumerable<Platform> GetAll();
    Platform GetById(int id);
    void Create(Platform plat);
}