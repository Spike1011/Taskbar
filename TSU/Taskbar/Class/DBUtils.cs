using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using MathNet.Numerics;


namespace TSU
{
    public class DBUtils
    {
        readonly SqlCommand myCommand = new SqlCommand();
        
        /// <summary>
        /// Запись в БД, таблица data
        /// </summary>
        /// <param name="_vf">Массив строк, записываемый в базу </param>
        /// <param name="name_monitor">Имя столбцов</param>
        public void WriteToSql(List<string> _vf, string name_monitor, int monitor_id, string[] set)
        {
            /// Синхронизация потоков
            ///  - исключает вероятность обращения к базе в одно и тоже время
            lock (Boss.locker)
            {
                SqlConnection conn = DBSQLServerUtils.GetDBConnection(set[0], set[1], set[2], set[3], set[4]);
                DataTable from_data = new DataTable();
                int data_id = 0;
                conn.Open();
                myCommand.Connection = conn;


                if (name_monitor == "data")
                {
                    SqlDataAdapter adapter1 = new SqlDataAdapter("" +
                        "SELECT * " +
                        "FROM data " +
                        "WHERE timestamp >= DATEADD(day, -1, GETDATE())", conn);
                    adapter1.Fill(from_data);
                    var f_d_s = from_data.Select("monitor_id = " + monitor_id);
                    _vf.AddRange(Polunminal(f_d_s, _vf));
                }
            
                try
                {
                    //  запрос для вывода data 
                    from_data.Clear();
                    SqlDataAdapter adapter = new SqlDataAdapter("" +
                        "select *" +
                        "from " + name_monitor, conn);
                    adapter.Fill(from_data);
                    foreach (DataRow dataRow in from_data.Select(" data_id =  MAX(data_id)"))
                        data_id = Convert.ToInt32(dataRow["data_id"]) + 1;
                    adapter.Dispose();
                }
                catch(Exception ex)
                {
                    data_id = 0;
                }
            
            
                // вывод в базу
                string result = FormatScript(name_monitor, data_id, _vf, monitor_id);
                if(result != "")
                {
                    myCommand.CommandText = result;
                    myCommand.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Формирование запроса в базу
        /// </summary>
        public string FormatScript(string stringSql, int id, List<string> data, int monitor_id)
        {
            bool flag = false;
            string[] named_column = new string[1];
            string table = stringSql;

            switch (stringSql)
            {
                case "data":
                    named_column = new string[]
                    {
                        "data_id",
                        "monitor_id",
                        "timestamp",
                        "c_h2",
                        "c_co",
                        "c_co2",
                        "c_ch4",
                        "c_c2h2",
                        "c_c2h4",
                        "c_c2h6",
                        "c_O2",
                        "c_n2",
                        "roc_h2",
                        "roc_co",
                        "roc_co2",
                        "roc_ch4",
                        "roc_c2h2",
                        "roc_c2h4",
                        "roc_c2h6",
                        "roc_O2",
                        "roc_n2",
                        "health"
                    };
                    break;
                case "oper_data":
                    named_column = new string[]
                    {
                        "data_id",
                        "monitor_id",
                        "timestamp",
                        "ia",
                        "ib",
                        "ic",
                        "ua",
                        "ub",
                        "uc",
                        "t_env",
                        "t_top_oil",
                        "hum_env",
                        "hum_oil_rel",
                        "hum_oil_abs"
                    };
                    break;
            }

            // заголовки столбцов
            stringSql = "INSERT INTO " + stringSql + "(";
            stringSql += named_column[0];
            for (int i = 1; i < named_column.Length; i++)
            {
                if (i == named_column.Length - 1)
                    stringSql = stringSql + ", " + named_column[i] + ") Values (";
                else
                    stringSql = stringSql + ", " + named_column[i];
            }
            stringSql += id.ToString() + ", ";
            stringSql += monitor_id.ToString();

            // значения
            for (int i = 0; i < data.Count; i++)
            {

                if (i == 0 && flag == false)
                {
                    stringSql += ", CURRENT_TIMESTAMP";
                    i--;
                    flag = true;
                }
                else
                {
                    if (i == data.Count - 1)
                    {
                        if (table == "data")
                            stringSql = stringSql + ", " + data[i] + ", 1)";
                        else
                            stringSql = stringSql + ", " + data[i] + ")";
                    }
                    else
                    {
                        stringSql = stringSql + ", " + data[i];
                    }
                }
            }
            return stringSql;
        }

        /// <summary>
        /// Вычисление скоростей изменения
        /// </summary>
        /// <param name="dataInRangetime"></param>
        /// <param name="dataValueInRegisters"></param>
        /// <returns></returns>
        private List<string> Polunminal(DataRow[] dataInRangetime, List<string> dataValueInRegisters)
        {
            double[][] for_pol1 = new double[][] { };
            List<string> polunominal = new List<string>();
            double[] ArrayTime = new double[] { };
            //DateTime dateTime = new DateTime();
            int i = 0;
            bool flag = false;
            string[] columns =
            {
                "c_h2",
                "c_co",
                "c_co2",
                "c_ch4",
                "c_c2h2",
                "c_c2h4",
                "c_c2h6",
                "c_o2",
                "c_n2",
                "timestamp",
            };

            ReformPolynom(dataInRangetime, ref for_pol1, ref ArrayTime, ref i, ref flag, columns);

            // флаг для идентификации первой записи за день
            if (flag)
            {
                double[][] for_pol2 = new double[9][];
                // циклы для транспонирования матрицы
                for (i = 0; i < 9; i++)
                {
                    int TR = 0;
                    List<double> vs = new List<double>();
                    for (int j = 0; j < for_pol1.Count(); j++)
                    {
                        vs.Add(for_pol1[j][i]);
                        TR = j;
                    }

                    try
                    {
                        Array.Resize(ref ArrayTime, TR + 2);

                        var dateTime = Convert.ToDateTime(DateTime.Now);
                        ArrayTime[TR + 1] = dateTime.ToOADate();
                        vs.Add(Convert.ToDouble(dataValueInRegisters[i].Replace(".", ",")));

                        for_pol2[i] = vs.ToArray();
                    }
                    catch (Exception ex)
                    { }
                }

                // вычисления polynomial
                for (int er = 0; er < 9; er++)
                {

                    try
                    {
                        var qwe = Fit.Polynomial(ArrayTime, for_pol2[er], 1);
                        if (!Double.IsInfinity(Fit.Polynomial(ArrayTime, for_pol2[er], 1)[1]))
                        {
                            polunominal.Add(String.Format("{0:0.000000}", Fit.Polynomial(ArrayTime.ToArray(), for_pol2[er], 1)[1]));
                            polunominal[er] = (polunominal[er].Replace(",", "."));
                        }
                        else
                            polunominal.Add("0");
                    }
                    catch (Exception ex)
                    {
                        polunominal.Add("0");
                    }
                }
                flag = false;
            }
            else
                for (int er = 0; er < 9; er++)
                    polunominal.Add("NULL");
            return polunominal;
        }

        /// <summary>
        /// Формирование данных 
        /// </summary>
        /// <param name="dataInRangetime"></param>
        /// <param name="for_pol1"></param>
        /// <param name="ArrayTime"></param>
        /// <param name="i"></param>
        /// <param name="flag"></param>
        /// <param name="columns"></param>
        private void ReformPolynom(DataRow[] dataInRangetime, ref double[][] for_pol1, ref double[] ArrayTime, ref int i, ref bool flag, string[] columns)
        {
            foreach (DataRow dataRow in dataInRangetime)
            {
                // проверка (если один, то roc_газ null)
                if (dataInRangetime.Count() == 0)
                    break;


                try
                {
                    // цикл для перебора всех значений из DataTable в ступенчатый массив
                    Array.Resize(ref for_pol1, i + 1);
                    for (int j = 0; j < 9; j++)
                    {
                        Array.Resize(ref for_pol1[i], j + 1);
                        for_pol1[i][j] = Convert.ToDouble(dataRow[columns[j]]);
                    }
                }
                catch (Exception ex)
                { }

                // преобразование времени в double
                Array.Resize(ref ArrayTime, i + 1);
                var dateTime = Convert.ToDateTime(dataRow[columns[9]]);
                ArrayTime[i] = dateTime.ToOADate();
                i += 1;
                flag = true;
            }
        }
    } 

}
