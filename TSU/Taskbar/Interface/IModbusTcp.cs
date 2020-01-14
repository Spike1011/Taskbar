using Modbus.Device;
using System.Net.Sockets;

namespace TSU
{
    public interface IModbusTcp
    {
        bool CreateModBus(TcpClient tcpClient, out ModbusIpMaster master);
        string ReadHoldingRegisters(ModbusIpMaster master, ushort reg);
        string ReadInputRegisters(ModbusIpMaster master, ushort reg);
    }
}
