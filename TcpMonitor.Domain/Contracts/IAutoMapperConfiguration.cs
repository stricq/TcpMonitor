using AutoMapper;


namespace TcpMonitor.Domain.Contracts {

  public interface IAutoMapperConfiguration {

    void RegisterMappings(IMapperConfigurationExpression config);

  }

}
