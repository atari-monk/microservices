using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing;

public class EventProcessor
  : IEventProcessor
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IMapper mapper;

    public EventProcessor(
        IServiceScopeFactory scopeFactory
        , IMapper mapper)
    {
        this.scopeFactory = scopeFactory;
        this.mapper = mapper;
    }

    public void ProcessEvent(string msg)
    {
        var eventType = DetermineEvent(msg);
        
        switch (eventType)
        {   
            case EventType.PlatformPublished:
                AddPlatform(msg);
                break;
            default:
                break;
        }
    }

    private EventType DetermineEvent(string notificationMessage)
    {
        Console.WriteLine("--> Determining Event");
        var eventType = 
            JsonSerializer.Deserialize<GenericEventDto>(
                notificationMessage);
        ArgumentNullException.ThrowIfNull(eventType);
        switch(eventType.Event)
        {
            case "Platform_Published":
                Console.WriteLine("Platform Published Event Detected");
                return EventType.PlatformPublished;
            default :
                Console.WriteLine("--> Could not determine the event type");
                return EventType.Undetermined;
        }
    }

    private void AddPlatform(string platformPublishedMessage)
    {
        using(var scope = scopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

            var platformPublishedDto = 
                JsonSerializer.Deserialize<PlatformPublishedDto>(
                    platformPublishedMessage);
            
            try
            {
                var plat = mapper.Map<Platform>(platformPublishedDto);
                if(repo.ExternalPlatformExists(plat.ExternalId) == false)
                {
                    repo.CreatePlatform(plat);
                    repo.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"--> Platform already exists...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not add Platform to DB {ex.Message}");
            }
        }
    }
}