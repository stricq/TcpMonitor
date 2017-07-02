using System.ComponentModel.Composition;

using AutoMapper;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.Mappings {

  [Export(typeof(IAutoMapperConfiguration))]
  public class ViewEntityMappingConfiguration : IAutoMapperConfiguration {

    #region IAutoMapperConfiguration

    public void RegisterMappings(IMapperConfigurationExpression config) {
      config.CreateMap<DomainWindowSettings, WindowSettingsViewEntity>().ForMember(dest => dest.AreSettingsChanged, opt => opt.Ignore())
                                                                        .ReverseMap();

      config.CreateMap<DomainConnection, ConnectionViewEntity>().ForMember(dest => dest.IsVisible,       opt => opt.Ignore())
                                                                .ForMember(dest => dest.HasChanged,      opt => opt.Ignore())
                                                                .ForMember(dest => dest.HasData,         opt => opt.Ignore())
                                                                .ForMember(dest => dest.IsClosed,        opt => opt.Ignore())
                                                                .ForMember(dest => dest.IsNew,           opt => opt.Ignore())
                                                                .ForMember(dest => dest.IsSelected,      opt => opt.Ignore())
                                                                .ForMember(dest => dest.LocalAddress,    opt => opt.ResolveUsing(src => src.LocalEndPoint.Address.ToString()))
                                                                .ForMember(dest => dest.LocalPort,       opt => opt.MapFrom(src => src.LocalEndPoint.Port))
                                                                .ForMember(dest => dest.RemoteAddress,   opt => opt.ResolveUsing(src => src.RemoteEndPoint.Address.ToString()))
                                                                .ForMember(dest => dest.RemotePort,      opt => opt.MapFrom(src => src.RemoteEndPoint.Port))
                                                                .ForMember(dest => dest.PacketsSent,     opt => opt.Ignore())
                                                                .ForMember(dest => dest.BytesSent,       opt => opt.Ignore())
                                                                .ForMember(dest => dest.PacketsReceived, opt => opt.Ignore())
                                                                .ForMember(dest => dest.BytesReceived,   opt => opt.Ignore());
    }

    #endregion IAutoMapperConfiguration

  }

}
