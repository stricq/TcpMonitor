using System.ComponentModel.Composition;

using AutoMapper;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Repository.Models.IpService;


namespace TcpMonitor.Repository.Mappings {

  [Export(typeof(IAutoMapperConfiguration))]
  public class DomainModelMappingConfiguration : IAutoMapperConfiguration {

    #region IAutoMapperConfiguration

    public void RegisterMappings(IMapperConfigurationExpression config) {
      config.CreateMap<Connection, DomainConnection>().ForMember(dest => dest.Key,            opt => opt.ResolveUsing(src => $"{src.ConnectionType}/{src.LocalEndPoint.Address.ToString()}/{src.LocalEndPoint.Port}/{src.RemoteEndPoint.Address.ToString()}/{src.RemoteEndPoint.Port}"))
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
