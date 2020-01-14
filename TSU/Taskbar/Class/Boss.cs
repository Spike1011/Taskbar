using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TSU
{
    public class Boss
    {
        IModbusTcp modbusTcp = new ModbusTcp();
        LoggerProj logger = new LoggerProj();
        public static object locker = new object();

        /// <summary>
        /// Запуск цикла программы
        /// </summary>
        /// <param name="_data"></param>
        public void Mod_TCP()
        {
            string[] settings = new string[5];

            // чтение xml
            XDocument xdoc = null;
            XElement root = null;
            XElement Setting = null;
            try
            {
                xdoc = XDocument.Load(@"..\DocXML.xml");
                root = xdoc.Element("Root");
                Setting = xdoc.Root.Element("Setting");
            }
            catch
            {
                MessageBox.Show("Файл конфигурации не найден.");
                Application.Exit();
            }

            // настройки подключения
            settings[0] = Setting.Element("ip").Value.ToString();
            settings[1] = Setting.Element("name").Value.ToString();
            settings[2] = Setting.Element("login").Value.ToString();
            settings[3] = Setting.Element("pass").Value.ToString();
            settings[4] = Setting.Element("security").Value.ToString();

            foreach (XElement item in root.Elements("monitor").ToList())
            {
                if(item.Element("check").Value == "True" || item.Element("check").Value == "true")
                    NewThread(item, settings);
            }
        }

        /// <summary>
        /// Создание нового потока
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="settings"></param>
        private async void NewThread(XElement xElement, string[] settings)
        {
            string[] data = null;
            Port port = null;

            // Определение типа данных (Концентрации, Рабочие параметры)
            if (xElement.Element("type").Value.ToString() == "Концентрации")
            {
                Concentration(xElement, out data, out port);
            }
            else
            {
                OperatingParams(xElement, out data, out port);
            }
            data[0] = xElement.Element("ip").Value.ToString();            
            int monitor_id = Convert.ToInt32(xElement.Element("monitor_id").Value);

            // создание потока
            await Task.Run(() =>
            {
                while(true)
                {
                    //при нажатой кнопки паузы - пропуск
                    if (Main.pause)
                        continue;
                    port.Start_Processing( data,monitor_id, settings);
                    GC.Collect();
                    Thread.Sleep(Convert.ToInt32(xElement.Element("period").Value));
                }
            });
        }

        /// <summary>
        /// Рабочие параметры
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="data"></param>
        /// <param name="port"></param>
        private void OperatingParams(XElement xElement, out string[] data, out Port port)
        {
            port = new Port(logger, modbusTcp, 1);
            data = new string[23];
            data[1] = xElement.Element("ia_1").Value.ToString();
            data[2] = xElement.Element("ia_2").Value.ToString();
            data[3] = xElement.Element("ib_1").Value.ToString();
            data[4] = xElement.Element("ib_2").Value.ToString();
            data[5] = xElement.Element("ic_1").Value.ToString();
            data[6] = xElement.Element("ic_2").Value.ToString();
            data[7] = xElement.Element("ua_1").Value.ToString();
            data[8] = xElement.Element("ua_2").Value.ToString();
            data[9] = xElement.Element("ub_1").Value.ToString();
            data[10] = xElement.Element("ub_2").Value.ToString();
            data[11] = xElement.Element("uc_1").Value.ToString();
            data[12] = xElement.Element("uc_2").Value.ToString();
            data[13] = xElement.Element("t_env_1").Value.ToString();
            data[14] = xElement.Element("t_env_2").Value.ToString();
            data[15] = xElement.Element("t_top_oil_1").Value.ToString();
            data[16] = xElement.Element("t_top_oil_2").Value.ToString();
            data[17] = xElement.Element("hum_env_1").Value.ToString();
            data[18] = xElement.Element("hum_env_2").Value.ToString();
            data[19] = xElement.Element("hum_oil_rel_1").Value.ToString();
            data[20] = xElement.Element("hum_oil_rel_2").Value.ToString();
            data[21] = xElement.Element("hum_oil_abs_1").Value.ToString();
            data[22] = xElement.Element("hum_oil_abs_2").Value.ToString();
        }

        /// <summary>
        /// Концентрации
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="data"></param>
        /// <param name="port"></param>
        private void Concentration(XElement xElement, out string[] data, out Port port)
        {
            port = new Port(logger, modbusTcp, 0);
            data = new string[19];
            data[1] = xElement.Element("h2_1").Value.ToString();
            data[2] = xElement.Element("h2_2").Value.ToString();
            data[3] = xElement.Element("co_1").Value.ToString();
            data[4] = xElement.Element("co_2").Value.ToString();
            data[5] = xElement.Element("co2_1").Value.ToString();
            data[6] = xElement.Element("co2_2").Value.ToString();
            data[7] = xElement.Element("ch4_1").Value.ToString();
            data[8] = xElement.Element("ch4_2").Value.ToString();
            data[9] = xElement.Element("c2h2_1").Value.ToString();
            data[10] = xElement.Element("c2h2_2").Value.ToString();
            data[11] = xElement.Element("c2h4_1").Value.ToString();
            data[12] = xElement.Element("c2h4_2").Value.ToString();
            data[13] = xElement.Element("c2h6_1").Value.ToString();
            data[14] = xElement.Element("c2h6_2").Value.ToString();
            data[15] = xElement.Element("o2_1").Value.ToString();
            data[16] = xElement.Element("o2_2").Value.ToString();
            data[17] = xElement.Element("n2_1").Value.ToString();
            data[18] = xElement.Element("n2_2").Value.ToString();
        }
    }
}
