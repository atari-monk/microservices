using AutoMapper;
using Grpc.Core;
using PlatformService.Data;

namespace PlatformService.SyncDataServices.Grpc;

public class GrpcPlatformService
    : GrpcPlatform.GrpcPlatformBase
{
    private readonly IPlatformRepo repo;
    private readonly IMapper mapper;

    public GrpcPlatformService(
        IPlatformRepo repo
        , IMapper mapper)
    {
        this.repo = repo;
        this.mapper = mapper;
    }

    public override Task<PlatformResponse> GetAllPlatforms(
        GetAllRequest request
        , ServerCallContext context)
    {
        var response = new PlatformResponse();
        var platforms = repo.GetAll();
        foreach (var plat in platforms)
        {
            response.Platform.Add(mapper.Map<GrpcPlatformModel>(plat));
        } 

        return Task.FromResult(response);
    }
}