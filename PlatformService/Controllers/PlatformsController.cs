using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepo repo;
    private readonly IMapper mapper;

    public PlatformsController(
        IPlatformRepo repo
        , IMapper mapper)
    {
        this.repo = repo;
        this.mapper = mapper;
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
    public ActionResult<PlatformReadDto> CreatePlatform(PlatformCreateDto platformCreateDto)
    {
        var platformModel = mapper.Map<Platform>(platformCreateDto);
        repo.Create(platformModel);
        repo.SaveChanges();

        var platformReadDto = mapper.Map<PlatformReadDto>(platformModel);

        return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
    }
}