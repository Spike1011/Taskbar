using NUnit.Framework;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using TSU;
using System.Data.SqlClient;
using Modbus.Device;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Web.Services.Description;

namespace TSU.Test
{
    /// <summary>
    /// DBUtils
    /// </summary>
    [TestFixture]
    public class TestDbUtils
    {
        readonly ILogger logger = Substitute.For<ILogger>();
        readonly IModbusTcp holdingRegisters = Substitute.For<IModbusTcp>();

        /// <summary>
        /// Формирование запросов в базу
        /// Хроматограф
        /// </summary>
        [TestCase("data", 3, 7)]
        [TestCase("data", 4, 5)]
        public void FormationScriptHr_StringNameAndListValues_ReturnString(string s, int id, int monitor_id)
        {
            List<string> data = new List<string>() { "0", "1", "2", "3", "4", "5",
            "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20"};
            string equal_to = $"INSERT INTO {s}(data_id, monitor_id, timestamp, " +
                $"c_h2, c_co, c_co2, c_ch4, c_c2h2, c_c2h4, c_c2h6, c_O2, c_n2, roc_h2, roc_co, roc_co2, roc_ch4, roc_c2h2, roc_c2h4, roc_c2h6, roc_O2, roc_n2, health) Values (" +
                $"{id}, {monitor_id}, CURRENT_TIMESTAMP, {data[0]}, {data[1]}, {data[2]}, {data[3]}, {data[4]}, {data[5]}, {data[6]}, {data[7]}, {data[8]}, {data[9]}, {data[10]}, {data[11]}, {data[12]}, {data[13]}, {data[14]}, {data[15]}, {data[16]}, {data[17]}, {data[18]}, {data[19]}, {data[20]}, 1)";
            var sut = new DBUtils();

            string ans = sut.FormatScript(s, id, data, monitor_id);

            Assert.That(ans, Is.EqualTo(equal_to));
        }


        /// <summary>
        /// Формирование запроса в базу
        /// ТОР
        /// </summary>
        /// <param name="s"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        [TestCase("oper_data", 3, 5)]
        [TestCase("oper_data", 5, 7)]
        public void FormationScriptTor_StringNameAndListValues_ReturnString(string s, int id, int monitor_id)
        {
            List<string> data = new List<string>() { "0", "1", "2", "3", "4", "5",
            "6", "7", "8", "9", "10", "11"};
            string equal_to = $"INSERT INTO {s}(data_id, monitor_id, timestamp, " +
                $"ia, ib, ic, ua, ub, uc, t_env, t_top_oil, hum_env, hum_oil_rel, hum_oil_abs) Values (" +
                $"{id}, {monitor_id}, CURRENT_TIMESTAMP, {data[0]}, {data[1]}, {data[2]}, {data[3]}, {data[4]}, {data[5]}, {data[6]}, {data[7]}, {data[8]}, {data[9]}, {data[10]}," +
                $" {data[11]})";
            var sut = new DBUtils();

            string ans = sut.FormatScript(s, id, data, monitor_id);

            Assert.That(ans, Is.EqualTo(equal_to));
        }

        /// <summary>
        /// Получение листа значений изменения скоростей концентраций
        /// </summary>
        [Test]
        public void Poluminal_DataRowAndList_ReturnList()
        {
            List<string> ans = new List<string>();
            string[] data = new string[] { "111.111.111.111", "9", "1", "9", "1", "9", "1", "9", "1", "9", "", "9", "", "9", "", "9", "", "9", "" };

            holdingRegisters.ReadHoldingRegisters(Arg.Any<ModbusIpMaster>(), 1).ReturnsForAnyArgs("1000");
            holdingRegisters.CreateModBus(Arg.Any<TcpClient>(), out Arg.Any<ModbusIpMaster>()).Returns(true);

            Port port = new Port(logger, holdingRegisters, 1);
            ans = port.ReadDataTcp(data);

            Assert.IsNotNull(ans);
        }


        /// <summary>
        /// Получение листа значений изменения скоростей концентраций (ошибка)
        /// </summary>
        [Test]
        public void Poluminal_DataRowAndList_ReturnError()
        {
            int ans = 0;
            List<string> list = new List<string>();
            string[] data = new string[] { "111.111.111.111", "9", "", "9", "", "9", "", "9", "", "9", "", "9", "", "9", "", "9", "", "9", "" };
            string[] setting = new string[] { "111.111.111.111", "test", "login", "pass", "False" };

            holdingRegisters.ReadHoldingRegisters(Arg.Any<ModbusIpMaster>(), Arg.Any<ushort>()).Throws(new Exception()); ;
            holdingRegisters.CreateModBus(Arg.Any<TcpClient>(), out Arg.Any<ModbusIpMaster>()).Returns(true);

            Port port = new Port(logger, holdingRegisters, 1);
            ans = port.Start_Processing(data, 1, setting);

            Assert.That(ans, Is.EqualTo(1));
        }

    }

    /// <summary>
    /// Port
    /// </summary>
    [TestFixture]
    public class TestPort
    {
        readonly ILogger logger = Substitute.For<ILogger>();
        readonly IModbusTcp holdingRegisters = Substitute.For<IModbusTcp>();


        [Test]
        public void DisconnectTcp_MasterClient_ReturnNull()
        {
            TcpClient tcpClient = new TcpClient();

            var port = Substitute.For<Port>();
            port.master_tcp = ModbusIpMaster.CreateIp(tcpClient);
            port.Disconnect();

            Assert.IsNull(port.master_tcp.Transport);
        }

        #region тесты на подключения
        /// <summary>
        /// Ошибка при подключении к базе
        /// </summary>
        [Test]
        public void ExWriteToSqlDB_StringData_Return2()
        {
            string[] data = new string[] { "111.111.111.111", "9","", "9", "", "9", "", "9", "", "9", "", "9", "", "9", "", "9", "", "9", "" };
            string[] setting = new string[] { "111.111.111.111", "test", "login", "pass", "False" };
            int monitor_id = 2;

            holdingRegisters.ReadHoldingRegisters(Arg.Any<ModbusIpMaster>(), 1).ReturnsForAnyArgs("1");
            holdingRegisters.CreateModBus(Arg.Any<TcpClient>(), out Arg.Any<ModbusIpMaster>()).Returns(true);

            Port port = new Port(logger, holdingRegisters, 1);
            int actual = port.Start_Processing(data, monitor_id , setting);

            Assert.That(actual, Is.EqualTo(2));
        }

        /// <summary>
        /// Ошибка подключения
        /// </summary>
        [Test]
        public void ConnectTcpFalse_IpAdress_Return0()
        {

            var sut = new Port(logger, holdingRegisters, 1);
            int actual = sut.ConnectTcp(string.Empty);

            Assert.That(actual, Is.EqualTo(0));
        }

        /// <summary>
        /// Ошибка прибора
        /// </summary>
        [Test]
        public void ConnectTcpModbusFalse_IpAdress_Return1()
        {
            string IpAdress = "010.000.000.000";
            holdingRegisters.CreateModBus(Arg.Any<TcpClient>(), out Arg.Any<ModbusIpMaster>()).Returns(false);

            var sut = new Port(logger, holdingRegisters, 1);
            int actual = sut.ConnectTcp(IpAdress);

            Assert.That(actual, Is.EqualTo(1));
        }

        /// <summary>
        /// Нормальное состояние
        /// </summary>
        [Test]
        public void ConnectTcpTrue_IpAdress_Return2()
        {
            string IpAdress = "010.000.000.000";
            int monitor = 2;

            holdingRegisters.CreateModBus(Arg.Any<TcpClient>(), out Arg.Any<ModbusIpMaster>()).Returns(true);

            var sut = new Port(logger, holdingRegisters, 1);
            int actual = sut.ConnectTcp(IpAdress);

            Assert.That(actual, Is.EqualTo(11));
        }

        /// <summary>
        /// Ошибка файла конфигурации
        /// </summary>
        [Test]
        public void ErrorReadConfig_IpAdress_Return4()
        {
            int monitor = 3;
            IModbusTcp holdingRegisters = Substitute.For<IModbusTcp>();

            var sut = new Port(logger, holdingRegisters, 1);
            int actual = sut.ConnectTcp(null);

            Assert.That(actual, Is.EqualTo(4));
        }
        #endregion
    }

    /// <summary>
    /// ReadData
    /// </summary>
    [TestFixture]
    public class ReadData
    {

        readonly ILogger logger = Substitute.For<ILogger>();
        readonly IModbusTcp holdingRegisters = Substitute.For<IModbusTcp>();

        /// <summary>
        /// Тест на пустые значения регистров
        /// </summary>
        [Test]
        public void ReadData_StringNull_Result()
        {
            int monitor_id = 1;
            string[] data = new string[] { "", "", "" };
            string[] settings = new string[] { "3" };

            var sut = new Port(logger, holdingRegisters, 1);
            int actual_tcp = sut.Start_Processing(data, monitor_id, settings);

            Assert.That(actual_tcp, Is.EqualTo(5));
        }

        /// <summary>
        /// Тест исключения при ошибки чтения регистров Tcp
        /// </summary>
        [Test]
        public void ReadDataExcept1_StringData_Returt1()
        {
            string[] data = new string[] { "1", "3", "4" };
            string[] settings = new string[] { "3" };
            int actual;
            int monitor_id = 1;

            holdingRegisters.ReadHoldingRegisters(Arg.Any<ModbusIpMaster>(), Arg.Any<ushort>()).Throws(new Exception());
            holdingRegisters.CreateModBus(Arg.Any<TcpClient>(), out Arg.Any<ModbusIpMaster>()).Returns(true);

            var sut = new Port(logger, holdingRegisters, 1);
            actual = sut.Start_Processing(data, monitor_id, settings);

            Assert.That(actual, Is.EqualTo(1));
        }

    }
}
