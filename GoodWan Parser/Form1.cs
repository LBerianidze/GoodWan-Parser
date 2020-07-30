using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Globalization;
using System.IO;
using GoodWan;
using System.Xml;

namespace GoodWan_Parser
{
    public partial class Form1 : Form
    {
        string connectionString = "";
        public Form1()
        {
            InitializeComponent();
            LoadXML();
        }
        /// <summary>
        /// Загрузка конфига
        /// </summary>
        void LoadXML()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(File.ReadAllText("configs.xml"));

            XmlNode node = document.SelectSingleNode("/Config/SQLConnectionString");
            connectionString = node.ChildNodes[0].InnerText;

        }
        IEnumerable<IGrouping<int, ParseItem>> result;
        private void start_Click(object sender, EventArgs e)
        {
            Parser parser = new Parser(loginTextBox.Text, passwordTextBox.Text);
            DateTime from = dateTimePicker1.Value;
            DateTime to = dateTimePicker2.Value;
            //Для показание данных в dataGridView1 
            result = parser.Start(from, to);
            dataGridView1.Rows.Clear();
            foreach(var group in result)
            {
                foreach(var item in group)
                {
                    dataGridView1.Rows.Add(item.Event_ID, item.Device_ID, item.TimeStamp, item.Data.FirstOrDefault(t => t.Key == "Air").Value,
                        item.Data.FirstOrDefault(t => t.Key == "Humidity").Value,
                        item.Data.FirstOrDefault(t => t.Key == "Temperature").Value
                        );
                }
            }
            MessageBox.Show("Парсинг завершен!");
        }

        private void save_Click(object sender, EventArgs e)
        {
            if (result != null)
            {
                SQLWorker worker = new SQLWorker(connectionString);
                worker.UploadToDatabase(result);
                MessageBox.Show("Результат сохранен в БД!");
            }
        }
    }
}
