using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodWan
{
    public class SQLWorker : IDisposable
    {
        private string connectionString;
        SqlConnection conn;

        public SQLWorker(string connectionString)
        {
            this.connectionString = connectionString;
            conn = new SqlConnection(connectionString);
            conn.Open();
        }

        public void Dispose()
        {
            conn.Close();
        }
        /// <summary>
        /// Чтение всехд анных из таблицы
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <returns></returns>
        public List<string[]> ReadTable(string table)
        {
            SqlCommand cmd = new SqlCommand("select * from " + table, conn);
            var reader = cmd.ExecuteReader();
            List<string[]> result = new List<string[]>();
            while (reader.Read())
            {
                string[] item = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    item[i] = reader.GetSqlValue(i).ToString();
                }
                result.Add(item);
            }
            reader.Close();
            return result;
        }
        private SqlCommand GetCommand(ParseItem item,object param4,object param10,object param9)
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO measure.Metering " +
"(DateCreated, Date, EquipmentOID, IntParam, StrParam, DateParam, BoolParam, NumericParam, NumericParam2,EquipmentCharacterGUID) " +
"VALUES(@param1,@param2,@param3,@param4,@param5,@param6,@param7,@param8,@param9,@param10)", conn);
            cmd.Parameters.AddWithValue("@param1", DateTime.Now);
            cmd.Parameters.AddWithValue("@param2", DateTime.ParseExact(item.TimeStamp, "MM'/'dd'/'yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("@param3", item.Device_ID);
            cmd.Parameters.AddWithValue("@param5", "");
            cmd.Parameters.AddWithValue("@param6", DateTime.Now);
            cmd.Parameters.AddWithValue("@param7", "true");
            cmd.Parameters.AddWithValue("@param8", (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@param4", param4);
            cmd.Parameters.AddWithValue("@param10", param10);
            cmd.Parameters.AddWithValue("@param9", param9);

            return cmd;
        }
        /// <summary>
        /// Загрузка в базу данных
        /// </summary>
        /// <param name="result">Список объектов</param>
        public void UploadToDatabase(IEnumerable<IGrouping<int, ParseItem>> result)
        {
            var characters = ReadTable("device.EquipmentCharacter").ToDictionary(t => t[1], t => t[0]);
            foreach (var group in result)
            {
                foreach (var item in group)
                {
                    if (item.Data.ContainsKey("Air"))
                    {
                        var cmd = GetCommand(item, item.Data["Air"], characters["Воздух"], (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    if (item.Data.ContainsKey("Humidity"))
                    {
                        var cmd = GetCommand(item, item.Data["Humidity"], characters["Влажность"], (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    if (item.Data.ContainsKey("Temperature"))
                    {
                        var cmd = GetCommand(item, (object)DBNull.Value, characters["Температура"], item.Data["Temperature"].Replace(",", "."));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}