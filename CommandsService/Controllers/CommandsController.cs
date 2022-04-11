using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers;

[Route("api/c/platforms/{platformId}/[controller]")]
[ApiController]
public class CommandsController 
    : ControllerBase
{
    private readonly ICommandRepo repo;
    private readonly IMapper mapper;

    public CommandsController(
        ICommandRepo repo
        , IMapper mapper
    )
    {
        this.repo = repo;
        this.mapper = mapper;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(
        int platformId
    )
    {
        Console.WriteLine($"--> Hit GetCommandsForPlatform: {platformId}");

        if(!repo.PlatformExists(platformId))
        {
            return NotFound();
        }

        var commands = repo.GetCommandsForPlatform(platformId);

        return Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
    }

    [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
    public ActionResult<CommandReadDto> GetCommandForPlatform(
        int platformId
        , int commandId
    )
    {
        Console.WriteLine($"--> Hit GetCommandForPlatform: {platformId} / {commandId}");

        if(!repo.PlatformExists(platformId))
        {
            return NotFound();
        }

        var command = repo.GetCommand(platformId, commandId);

        if(command is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<CommandReadDto>(command));
    }

    [HttpPost]
    public ActionResult<CommandReadDto> CreateCommandForPlatform(
        int platformId
        , CommandCreateDto commandDto)
    {
        Console.WriteLine($"--> Hit CreateCommandForPlatform: {platformId}");

        if(!repo.PlatformExists(platformId))
        {
            return NotFound();
        }              

        var command = mapper.Map<Command>(commandDto);

        repo.CreateCommand(platformId, command);
        repo.SaveChanges();

        var commandReadDto = mapper.Map<CommandReadDto>(command);

        return CreatedAtRoute(nameof(GetCommandForPlatform)
            , new { platformId = platformId, commandId = commandReadDto.Id}, commandReadDto);
    }   
}