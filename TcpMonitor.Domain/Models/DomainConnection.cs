using System.Diagnostics.CodeAnalysis;
using System.Net;

using Str.Common.Extensions;

using TcpMonitor.Domain.Contracts;


namespace TcpMonitor.Domain.Models;


[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by Automapper.")]
public class DomainConnection {

    #region Constructor

    public DomainConnection() { }

    public DomainConnection(DomainConnection source) {
        Key            = $"{source.Key}";
        Pid            = source.Pid;
        ProcessName    = source.ProcessName;
        ConnectionType = source.ConnectionType;
        State          = source.State;
        LocalEndPoint  = source.LocalEndPoint;
        RemoteEndPoint = source.RemoteEndPoint;
        LocalHostName  = source.LocalHostName;
        RemoteHostName = source.RemoteHostName;
    }

    #endregion Constructor

    #region Properties

    public string? Key { get; set; }

    public int Pid { get; set; }

    public required string ProcessName { get; set; }

    public required string ConnectionType { get; init; }

    public required string State { get; init; }

    public IPEndPoint? LocalEndPoint { get; set; }

    public IPEndPoint? RemoteEndPoint { get; set; }

    public required string LocalHostName { get; set; }

    public required string RemoteHostName { get; set; }

    #endregion Properties

    #region Domain Methods

    public Task ResolveHostNamesAsync(IConnectionsService connectionsService) {
        connectionsService.GetProcessNameAsync(Pid).ContinueWith(task => ProcessName = task.IsCompletedSuccessfully ? task.FireAndWait() : String.Empty).FireAndForget();

        if (LocalEndPoint != null) connectionsService.GetHostNameAsync(LocalEndPoint).ContinueWith(task => LocalHostName = task.IsCompletedSuccessfully ? task.FireAndWait() : String.Empty).FireAndForget();

        if (RemoteEndPoint != null) connectionsService.GetHostNameAsync(RemoteEndPoint).ContinueWith(task => RemoteHostName = task.IsCompletedSuccessfully ? task.FireAndWait() : String.Empty).FireAndForget();

        return Task.CompletedTask;
    }

    #endregion Domain Methods

    #region Overrides

    public override string ToString() {
        return Key ?? String.Empty;
    }

    #endregion Overrides

}
