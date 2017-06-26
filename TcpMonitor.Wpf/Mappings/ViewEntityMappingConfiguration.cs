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
      settings(config);
    }

    #endregion IAutoMapperConfiguration

    #region Private Methods

    private void settings(IMapperConfigurationExpression config) {
      config.CreateMap<DomainWindowSettings, WindowSettingsViewEntity>().ForMember(dest => dest.AreSettingsChanged, opt => opt.Ignore())
                                                                        .ReverseMap();
    }

    #endregion Private Methods

  }

}
