using System.ComponentModel.Composition;

using AutoMapper;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Repository.Models;


namespace TcpMonitor.Repository.Mappings {

  [Export(typeof(IAutoMapperConfiguration))]
  public sealed class DomainModelMappingConfiguration : IAutoMapperConfiguration {

    #region IAutoMapperConfiguration

    public void RegisterMappings(IMapperConfigurationExpression config) {
      config.CreateMap<Connection, DomainConnection>().ForMember(dest => dest.Key,            opt => opt.MapFrom(src => src.ConnectionType.StartsWith("TCP") ? $"{src.ConnectionType}/{src.LocalEndPoint.Address}/{src.LocalEndPoint.Port}/{src.RemoteEndPoint.Address}/{src.RemoteEndPoint.Port}"
                                                                                                                                                             : $"{src.ConnectionType}/{src.LocalEndPoint.Address}/{src.LocalEndPoint.Port}"))
                                                      .ForMember(dest => dest.LocalHostName,  opt => opt.Ignore())
                                                      .ForMember(dest => dest.RemoteHostName, opt => opt.Ignore());

      config.CreateMap<DomainConnection, DomainConnection>().ForMember(dest => dest.Key,            opt => opt.Ignore())
                                                            .ForMember(dest => dest.Pid,            opt => opt.Ignore())
                                                            .ForMember(dest => dest.ProcessName,    opt => opt.Ignore())
                                                            .ForMember(dest => dest.ConnectionType, opt => opt.Ignore())
                                                            .ForMember(dest => dest.LocalEndPoint,  opt => opt.Ignore())
                                                            .ForMember(dest => dest.RemoteEndPoint, opt => opt.Ignore())
                                                            .ForMember(dest => dest.LocalHostName,  opt => opt.Ignore())
                                                            .ForMember(dest => dest.RemoteHostName, opt => opt.Ignore())
                                                            .AfterMap((src, dest) => {
                                                              if (dest.Pid != 0 || src.Pid == 0) return;

                                                              dest.Pid         = src.Pid;
                                                              dest.ProcessName = src.ProcessName;
                                                            });
    }

    #endregion IAutoMapperConfiguration

  }

}
