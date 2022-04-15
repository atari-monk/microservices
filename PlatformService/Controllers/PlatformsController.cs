using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepo repo;
    private readonly IMapper mapper;
    private readonly ICommandDataClient commandDataClient;
    private readonly IMessageBusClient msgBusClient;

    public PlatformsController(
        IPlatformRepo repo
        , IMapper mapper
        , ICommandDataClient commandDataClient
        , IMessageBusClient msgBusClient)
    {
        this.repo = repo;
        this.mapper = mapper;
        this.commandDataClient = commandDataClient;
        this.msgBusClient = msgBusClient;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {
        Console.WriteLine("--> Getting Platforms...");
        return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(repo.GetAll()));
    }

    [HttpGet("{id}", Name = nameof(GetPlatformById))]
    public ActionResult<PlatformReadDto> GetPlatformById(int id)
    {
        var plat = repo.GetById(id);
        if(plat != null)
        {
            return Ok(mapper.Map<PlatformReadDto>(plat));
        }
        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
    {
        var platformModel = mapper.Map<Platform>(platformCreateDto);
        repo.Create(platformModel);
        repo.SaveChanges();

        var platformReadDto = mapper.Map<PlatformReadDto>(platformModel);

        try
        {
            await commandDataClient.SendPlatformToCommand(platformReadDto);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
        }
        
        try
        {
            var platformPublishedDto = mapper.Map<PlatformPublishedDto>(platformReadDto);
            platformPublishedDto.Event = "Platform_Published";
            msgBusClient.PublishNewPlatform(platformPublishedDto);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
        }

        return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
    }
}