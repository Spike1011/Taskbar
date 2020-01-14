using System;
using Modbus.Device;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSU
{
    public class Port
    {
        private TcpClient tcpClient;
        public ModbusIpMaster master_tcp;
        private string error;
        readonly int monitor;
        readonly string[] name_monitor = new string[]
        {
            "[data]",
            "[oper_data]"
        };

        private readonly ILogger _logger;
        private readonly IModbusTcp _modbustcp;
        delegate string ReadRtu(IModbusSerialMaster m, ushort u);
        delegate string ReadTcp(ModbusIpMaster m, ushort u);


        /// <summary>
        /// Конструктор класса Port
        /// </summary>
        /// <param name="data">данные из прибора</param>
        /// <param name="monitor">прибор</param>
        public Port(ILogger logger, IModbusTcp modbustcp,  int monitor)
        {
            _logger = logger;
            _modbustcp = modbustcp;
            this.monitor = monitor;
        }

        /// <summary>
        /// Чтение регистров 
        /// </summary>
        public int Start_Processing(string[] data, int monitor_id, string[] settings)
        {
            if (ConnectTcp(data[0]) == 11)
            {
                List<string> data_print;
                try
                {
                    data_print = ReadDataTcp(data);
                }
                catch (Exception ex)
                {
                    error = $"{name_monitor[monitor]} Ошибка при чтении данных из прибора";
                    Debug.WriteLine(error);
                    _logger.Error(error, ex);
                    return 1;
                }

                try
                {
                    DBUtils db = new DBUtils();
                    string s = "";
                    switch (name_monitor[monitor])
                    {
                        case "[data]":
                            s = "data";
                            break;
                        case "[oper_data]":
                            s = "oper_data";
                            break;
                    }

                    db.WriteToSql(data_print, s, monitor_id, settings);
                }
                catch (SqlException ex)
                {
                    error = "Ошибка подключения к базе данных," +
                        "  проверьте параметры подключения в файле конфигураций.";
                    _logger.Info(error);
                    Debug.WriteLine(error);
                    return 2;
                }
                catch (ThreadAbortException)
                {
                    Environment.Exit(0);
                    Application.Exit();
                    return 3;
                }
                catch (Exception ex)
                {
                    error = "Неизвестная ошибка";
                    _logger.Error(error, ex);
                    Debug.WriteLine(error);
                    return 4;
                }
                Disconnect();
                return 11;
            }
            else
                return 5;
        }

                
        /// <summary>
        /// Чтение регистров по TCP
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<string> ReadDataTcp(string[] data)
        {
            UInt32 t1 = 0, t2 = 0;
            Int32 o;
            float r;

            List<string> data_print = new List<string>();

            for (int i = 1; i < data.Length; i+=2)
            {
                if (data[i] == "na" || data[i] == "")
                    data_print.Add("NULL");
                else
                {
                    if (data[i + 1] == "na" || data[i + 1] == "")
                        data_print.Add(_modbustcp.ReadHoldingRegisters(master_tcp, Convert.ToUInt16(data[i])));
                    else
                    {
                        t1 = Convert.ToUInt32(_modbustcp.ReadHoldingRegisters(master_tcp, Convert.ToUInt16(data[i+1])), 10);
                        t2 = Convert.ToUInt32(_modbustcp.ReadHoldingRegisters(master_tcp, Convert.ToUInt16(data[i])), 10);

                        // конвертировать по стандарту IEEE, смещение регистров 
                        o = Convert.ToInt32((t2 << 16) | t1);
                        r = BitConverter.ToSingle(BitConverter.GetBytes(o), 0);
                        data_print.Add(Convert.ToString(r).Replace(",", "."));
                    }
                }
            }
            return data_print;
        }
        

        /// <summary>
        /// Подключение к прибору по протоколу TCP
        /// </summary>
        /// <returns></returns>
        public int ConnectTcp(string ipAddress)
        {
            if (master_tcp != null)
                master_tcp.Dispose();
            if (tcpClient != null)
                tcpClient.Close();
            try
            {
                tcpClient = new TcpClient();
                IAsyncResult asyncResult = tcpClient.BeginConnect(ipAddress, 502, null, null);
                asyncResult.AsyncWaitHandle.WaitOne(3000, true); //ожидание 3 секунды
                if (!asyncResult.IsCompleted)
                {
                    tcpClient.Close();
                    error = $"{name_monitor[monitor]}. Ошибка подключения, проверьте параметры подключения в файле конфигурации";
                    Debug.WriteLine(error);
                    _logger.Error(error);
                    return 0;
                }
                // Создание ModbusTCP соединения
                try
                {
                    bool master_flag = _modbustcp.CreateModBus(tcpClient, out master_tcp);
                    if (!master_flag)
                        throw new InvalidOperationException();
                }
                catch (InvalidOperationException ex)
                {
                    error = $"{name_monitor[monitor]}. Прибор недоступен. Проверьте статус прибора или соединения";
                    Debug.WriteLine(error);
                    _logger.Error(error, ex);
                    return 1;
                }
                return 11;
            }
            catch (ArgumentNullException ex)
            {
                error = $"{name_monitor[monitor]}. Файл конфигурации недоступен. Выполните конфигурирование программы";
                _logger.Error(error, ex);
                Debug.WriteLine(error);
                return 4;
            }
        }

        
        /// <summary>
        /// Разъединение потока с Tcp
        /// </summary>
        public void Disconnect()
        {
            if (master_tcp != null)
                master_tcp.Dispose();
            if (tcpClient != null)
                tcpClient.Close();
        }
    }
}
