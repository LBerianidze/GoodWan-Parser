using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GoodWan
{
    public class Parser
    {
        private const string APIURL = "https://api.goodwan.ru/v1/";//Ссылка на API
        private readonly string login;
        private readonly string password;

        public Parser(string login, string password)
        {
            this.login = login;
            this.password = password;
        }
        /// <summary>
        /// Парсинг по дате
        /// </summary>
        /// <param name="from_datetime">Начало</param>
        /// <param name="to_datetime">Конец</param>
        /// <returns></returns>
        public IEnumerable<IGrouping<int, ParseItem>> Start(DateTime from_datetime, DateTime to_datetime)
        {
            //Конвертирование даты в подходящий формат
            string from = from_datetime.ToString("yyyy-MM-ddTHH:mm:ss");
            string to = to_datetime.ToString("yyyy-MM-ddTHH:mm:ss");
            //Подключение к АПИ
            WebClient wb = new WebClient
            {
                Credentials = new NetworkCredential(login, password)
            };
            string parseurl = APIURL + $"events/ex?from={from}&to={to}";
            //Получение результата запроса
            var result = wb.DownloadString(parseurl);
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(result); //Преобразование результата в динамический объект json
            List<ParseItem> items = new List<ParseItem>();//список спарсенных событий
            foreach (var item in json)//итерация по списку результата
            {
                int event_id = item["event_id"];//Event id
                int device_id = item["device_id"];//Device ID
                string timestamp_utc = item["timestamp_utc"];//Date
                Dictionary<int, string> components = new Dictionary<int, string>();
                if (item.components != null)//Компоненты
                {
                    foreach (var component in item.components)
                    {
                        components.Add(Convert.ToInt32(component["component"]), component["value_decimal"].ToString());
                    }
                }

                Dictionary<string, string> data = new Dictionary<string, string>();//Воздух,влажность и прочее
                foreach (var data_item in item.data)
                {
                    data.Add(data_item.Name.ToString(), data_item.Value.ToString());
                }
                ParseItem parsed = new ParseItem(event_id, device_id, timestamp_utc, components, data);
                items.Add(parsed);
            }
            var grouped = items.GroupBy(t => t.Device_ID);//Группировка по ID устройств
            return grouped;
        }
    }
}
