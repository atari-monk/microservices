using PlatformService.Models;

namespace PlatformService.Data;

public class PlatformRepo 
: IPlatformRepo
{
    private readonly AppDbContext context;

    public PlatformRepo(AppDbContext context)
    {
        this.context = context;
    }

    public bool SaveChanges()
    {
        return context.SaveChanges() >= 0;
    }

    public IEnumerable<Platform> GetAll()
    {
        return context.Platforms.ToList();
    }
    
    public Platform GetById(int id)
    {
        return context.Platforms.FirstOrDefault(
            p => p.Id == id);
    }

    public void Create(Platform plat)
    {
        if(plat == null)
            throw new ArgumentNullException(nameof(plat));
        context.Platforms.Add(plat);
    }
}