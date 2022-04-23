using CommandsService.Models;

namespace CommandsService.Data;

public class CommandRepo
  : ICommandRepo
{
    private readonly AppDbContext context;

    public CommandRepo(
      AppDbContext context
    )
    {
        this.context = context;
    }

    public void CreateCommand(int platformId, Command command)
    {
      if(command == null)
      {
        throw new ArgumentNullException(nameof(command));
      }

      command.PlatformId = platformId;
      ArgumentNullException.ThrowIfNull(context.Commands);
      context.Commands.Add(command);
    }

    public void CreatePlatform(Platform plat)
    {
      if(plat == null)
      {
        throw new ArgumentNullException(nameof(plat));
      }
      ArgumentNullException.ThrowIfNull(context.Platforms);
      context.Platforms.Add(plat);
    }

    public bool ExternalPlatformExists(int externalPlatformId)
    {
      ArgumentNullException.ThrowIfNull(context.Platforms);
      return context.Platforms.Any(p => p.ExternalId == externalPlatformId);
    }

    public IEnumerable<Platform> GetAllPlatforms()
    {
      ArgumentNullException.ThrowIfNull(context.Platforms);
      return context.Platforms.ToList();
    }

    public Command GetCommand(int platformId, int commandId)
    {
      ArgumentNullException.ThrowIfNull(context.Commands);
      var cmd = context.Commands
        .Where(c => c.PlatformId == platformId 
          && c.Id == commandId).FirstOrDefault();
      if(cmd != null) return cmd;
      return new Command();
    }

    public IEnumerable<Command> GetCommandsForPlatform(int platformId)
    {
        Func<Command, string> orderBy = c => 
        {
            ArgumentNullException.ThrowIfNull(c.Platform);
            var name = c.Platform.Name;
            ArgumentNullException.ThrowIfNull(name);
            return name;
        };
        ArgumentNullException.ThrowIfNull(context.Commands);
        return context.Commands
        .Where(c => c.PlatformId == platformId)
        .OrderBy(orderBy);
    }

    public bool PlatformExists(int platformId)
    {
        ArgumentNullException.ThrowIfNull(context.Platforms);
        return context.Platforms.Any(p => p.Id == platformId);
    }

    public bool SaveChanges()
    {
        return (context.SaveChanges() >= 0);
    }
}