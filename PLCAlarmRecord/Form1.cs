using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using ClosedXML;
using ClosedXML.Excel;

namespace PLCAlarmRecord
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Process[] pro1 = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            Process[] pro2 = Process.GetProcessesByName("PLCAlarmRecord");
            if (pro1.Length > 1 || pro2.Length > 1)
            {
                MessageBox.Show("目前已有一个实例在运行，请勿重复运行程序!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
            this.StartPosition = FormStartPosition.CenterScreen;
            //this.WindowState = FormWindowState.Maximized;
            InitializeComponent();
        }

        //变量定义
        PLC.OmronPLC OmronPLC = new PLC.OmronPLC();
        DB.DBHand dBHand = new DB.DBHand();
        public struct SystemConfig { public string PLC_IP; public string PLC_Port; public string ServerName; public string DataBaseName; public string DataBaseUser; public string DataBasePassword; }
        public static SystemConfig systemConfig;
        private void Form1_Load(object sender, EventArgs e)
        {
            SystemConfigRead();
            OmronPLC.plcIp = systemConfig.PLC_IP;
            OmronPLC.plcPort = Convert.ToInt16(systemConfig.PLC_Port);
            OmronPLC.timeOut = 2000;
            OmronPLC.Init();
            timer1.Enabled = true;
            try
            {
                dBHand.Type = DB.DBHand.DBType.mssql;
                dBHand.ServerName = systemConfig.ServerName;
                dBHand.User = systemConfig.DataBaseUser;
                dBHand.Passwords = systemConfig.DataBasePassword;
                dBHand.DataBaseName = systemConfig.DataBaseName;
                dBHand.Connection();
                dBHand.TableName = "PLCAlarmRecord";
                dBHand.CreateTable(new string[] { "Time", "Variable", "AlarmContent" });
                if (dBHand.ConError != "")
                {
                    MessageBox.Show("创建数据库记录表失败");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                dBHand.UnConnection();
            }
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add("工位列表");
            treeView1.Nodes[0].Nodes.Add("OP10");
            treeView1.Nodes[0].Nodes.Add("OP20");
            treeView1.Nodes[0].Nodes[0].ImageIndex = 1;
            treeView1.Nodes[0].Nodes[1].ImageIndex = 1;

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show((DateTime.Now - Starttime).TotalSeconds.ToString());                   
            PLC_Connect.Image = OmronPLC.connectionStatus ? BoolStatus_imageList.Images[1] : BoolStatus_imageList.Images[2];
        }


        private void SystemConfigRead()
        {
            XDocument myXDoc = XDocument.Load(@"..\..\SystemConfig.xml");
            string[] value = new string[5];
            try
            {
                XElement root = myXDoc.Root;
                var elements = root.Elements();
                systemConfig.PLC_IP = root.Element("PLC").Element("PLC_IP").Value;
                systemConfig.PLC_Port = root.Element("PLC").Element("PLC_Port").Value;
                systemConfig.ServerName = root.Element("Database").Element("ServerName").Value;
                systemConfig.DataBaseName = root.Element("Database").Element("DataBaseName").Value;
                systemConfig.DataBaseUser = root.Element("Database").Element("DataBaseUser").Value;
                systemConfig.DataBasePassword = root.Element("Database").Element("DataBasePassword").Value;
            }
            catch
            {
                MessageBox.Show("系统配置文件读取失败");
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.ToString() != treeView1.Nodes[0].ToString())
            {
                treeView1.SelectedImageIndex = 1;
            } else
            {
                treeView1.SelectedImageIndex = 0;
            }

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.Clear(Color.FromArgb(85, 93, 137));
            //e.Graphics.Clear(Color.SteelBlue);
            e.Graphics.Clear(SystemColors.Control);
            e.Graphics.DrawRectangle(new Pen(Color.Black, 1), 0, 0, panel1.Width, panel1.Height);
            //e.Graphics.FillRectangle(new SolidBrush(SystemColors.Control), 0, 0, panel1.Width, panel1.Height);
            e.Graphics.DrawString("输出", new Font("Arial", 9), new SolidBrush(Color.Black), 10, 7);
            e.Graphics.Dispose();
        }
        public struct StoreData
        {
            public string 数据地址;
            public string 数据类型;
            public string 数据名;
        }
        public struct ErrorData
        {
            public string 报警地址;
            public string 数据类型;
            public string 报警内容;
        }
        public struct StationVariableAddress
        {
            public string 工位名;
            public string PLC心跳地址;
            public string 数据存储触发地址;
            public string 设备状态地址;
            public List<StoreData> 存表数据地址;
            public string 数据存储完成地址;
            public List<ErrorData> 报警数据地址;
        }
        public StationVariableAddress stationVariableAddress = new StationVariableAddress();
        private void 插入工位ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            String importExcelPath = @"..\..\工位配置.xlsx";
            var workBook = new XLWorkbook(importExcelPath);
            IXLWorksheet worksheet = workBook.Worksheet(1);
            stationVariableAddress.工位名 = worksheet.Name;
            stationVariableAddress.PLC心跳地址 = worksheet.Cell(1, 2).Value.ToString();
            stationVariableAddress.数据存储触发地址 = worksheet.Cell(3, 2).Value.ToString();
            stationVariableAddress.设备状态地址 = worksheet.Cell(5, 2).Value.ToString();
            stationVariableAddress.数据存储完成地址= worksheet.Cell(3, 6).Value.ToString();
            stationVariableAddress.存表数据地址 = new List<StoreData>();
            stationVariableAddress.报警数据地址 = new List<ErrorData>();
            stationVariableAddress.存表数据地址.Clear();
            stationVariableAddress.报警数据地址.Clear();
            for (int i=0;i<256;i++)
            {
                if (worksheet.Cell(12+i, 1).Value.ToString()=="" || worksheet.Cell(12+i, 2).Value.ToString() == "" || worksheet.Cell(12+i, 3).Value.ToString() == "")
                {
                    break;
                }else
                {
                    StoreData storeData = new StoreData();
                    storeData.数据地址 = worksheet.Cell(12+i, 1).Value.ToString();
                    storeData.数据类型 = worksheet.Cell(12+i, 2).Value.ToString();
                    storeData.数据名 = worksheet.Cell(12+i, 3).Value.ToString();
                    stationVariableAddress.存表数据地址.Add(storeData);
                }
            }

            for (int i = 0; i < 256; i++)
            {
                if (worksheet.Cell(12 + i, 5).Value.ToString() == "" || worksheet.Cell(12 + i, 6).Value.ToString() == "" || worksheet.Cell(12 + i, 7).Value.ToString() == "")
                {
                    break;
                }
                else
                {
                    ErrorData errorData = new ErrorData();
                    errorData.报警地址 = worksheet.Cell(12 + i, 5).Value.ToString();
                    errorData.数据类型 = worksheet.Cell(12 + i, 6).Value.ToString();
                    errorData.报警内容 = worksheet.Cell(12 + i, 7).Value.ToString();
                    stationVariableAddress.报警数据地址.Add(errorData);
                }
            }
            var temp = false;
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
            treeView1.ContextMenuStrip = treeView1.Nodes[0].IsSelected ?  contextMenuStrip1: null;
        }

        private void 删除选中的当前行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView3.Rows.RemoveAt(dataGridView3.CurrentRow.Index);
        }

        private void 删除所有行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView3.Rows.Clear();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
