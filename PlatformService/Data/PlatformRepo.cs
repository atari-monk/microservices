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
        ArgumentNullException.ThrowIfNull(context.Platforms);
        return context.Platforms.ToList();
    }
    
    public Platform GetById(int id)
    {
        ArgumentNullException.ThrowIfNull(context.Platforms);
        var plat =  context.Platforms.FirstOrDefault(
            p => p.Id == id);
        ArgumentNullException.ThrowIfNull(plat);
        return plat;
    }

    public void Create(Platform plat)
    {
        ArgumentNullException.ThrowIfNull(plat);
        ArgumentNullException.ThrowIfNull(context.Platforms);
        context.Platforms.Add(plat);
    }
}