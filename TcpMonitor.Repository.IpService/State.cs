

namespace TcpMonitor.Repository.IpService {

  public static class State {

    public const int MIB_TCP_STATE_CLOSED     =  1;
    public const int MIB_TCP_STATE_LISTEN     =  2;
    public const int MIB_TCP_STATE_SYN_SENT   =  3;
    public const int MIB_TCP_STATE_SYN_RCVD   =  4;
    public const int MIB_TCP_STATE_ESTAB      =  5;
    public const int MIB_TCP_STATE_FIN_WAIT1  =  6;
    public const int MIB_TCP_STATE_FIN_WAIT2  =  7;
    public const int MIB_TCP_STATE_CLOSE_WAIT =  8;
    public const int MIB_TCP_STATE_CLOSING    =  9;
    public const int MIB_TCP_STATE_LAST_ACK   = 10;
    public const int MIB_TCP_STATE_TIME_WAIT  = 11;
    public const int MIB_TCP_STATE_DELETE_TCB = 12;

  }

}
