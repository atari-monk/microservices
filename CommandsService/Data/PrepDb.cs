using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;

namespace CommandsService.Data;

public static class PrepDb
{
    public static void PrepPopulation(
        IApplicationBuilder appBuilder
    )
    {
        using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
        {
            var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();
            ArgumentNullException.ThrowIfNull(grpcClient);
            var platforms = grpcClient.ReturnAllPlatforms();

            SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
        }    
    }

    private static void SeedData(
        ICommandRepo? repo
        , IEnumerable<Platform> platforms)
    {
        Console.WriteLine("Seeding new platforms...");
        ArgumentNullException.ThrowIfNull(repo);
        foreach (var plat in platforms)
        {
            if(repo.PlatformExists(plat.ExternalId) == false)
            {
                repo.CreatePlatform(plat);
                repo.SaveChanges();
            }
        }
    }
}