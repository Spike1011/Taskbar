using System;
using Modbus.Device;
using System.Net.Sockets;

namespace TSU
{
    public class ModbusTcp : IModbusTcp
    {
        /// <summary>
        /// Чтение по 3 функции ReadHolding
        /// </summary>
        /// <param name="master"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        public string ReadHoldingRegisters(ModbusIpMaster master, ushort reg)
        {
            return Convert.ToString(master.ReadHoldingRegisters(1, reg, 1)[0], 10);
        }

        /// <summary>
        /// Чтение по 4 функции ReadInput
        /// </summary>
        /// <param name="master"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        public string ReadInputRegisters(ModbusIpMaster master, ushort reg)
        {
            return Convert.ToString(master.ReadInputRegisters(1, reg, 1)[0]);
        }

        /// <summary>
        /// Создание подключения
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="master"></param>
        /// <returns></returns>
        public bool CreateModBus(TcpClient tcpClient, out ModbusIpMaster master)
        {
            try
            {
                master = ModbusIpMaster.CreateIp(tcpClient);
                master.Transport.Retries = 0;
                master.Transport.ReadTimeout = 2500;
                return true;
            }
            catch
            {
                master = null;
                return false;
            }
        }
    }
}
