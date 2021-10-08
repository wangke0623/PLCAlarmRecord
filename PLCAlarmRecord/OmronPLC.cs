using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication;
using HslCommunication.Profinet.Omron;
using Timer = System.Windows.Forms.Timer;
using log4net;

namespace PLC
{

    public class OmronPLC
    {

        Timer timer1 = new Timer();//PC端心跳定时器声明
        Timer timer2 = new Timer();//PLC端心跳定时器声明
        Timer timer3 = new Timer();//PLC端心跳定时器声明
        Timer timer4 = new Timer();//PLC端心跳状态计时
        Timer timer5 = new Timer();//PLC重新建立连接计时

        public static readonly log4net.ILog log = log4net.LogManager.GetLogger("log");
        const Int16 almLen = 2000;
        public static HslCommunication.Profinet.Omron.OmronFinsNet omronFinsNet;
        OperateResult Connection = new OperateResult();
        static Int32 ReadCount = -1;
      

        private static string PlcIp;
        private static int PlcPort;
        private static int TimeOut;
        private static bool ConnectionStatus;
        private static bool ConnectionStatusTemp;

        private static bool PC心跳VariableValue;
        private static bool PLC心跳VariableValue;
        private static bool[] PLC报警VariableValue;
       
        private static string PC心跳VariableAddress;
        private static string PLC心跳VariableAddress;
        private static string[] PLC报警VariableAddress;

        public string plcIp {set { PlcIp = value; }}
        public Int16 plcPort {set { PlcPort = value; }}
        public int timeOut {set { TimeOut = value; }}
        public bool connectionStatus {get { return ConnectionStatus; }}
        public int readCount {get { return ReadCount; }}

        public string PC心跳variableAddress { set { PC心跳VariableAddress = value; } }
        public string PLC心跳variableAddress { set { PLC心跳VariableAddress = value; } }
        public string[] PLC报警variableAddress { set { PLC报警VariableAddress = value; } }

        public bool PC心跳variableValue { get { return PC心跳VariableValue; } }
        public bool PLC心跳variableValue { get { return PLC心跳VariableValue; } }
        public bool[] PLC报警variableValue { get { return PLC报警VariableValue; } }


        bool[] almTemp = new bool[almLen];
        static Event.CustomerEvent[] almEvent = new Event.CustomerEvent[almLen];
        public Boolean PlcConnect()
        {
            
            timer5.Stop();
            omronFinsNet = new OmronFinsNet();
            omronFinsNet.IpAddress = PlcIp;
            omronFinsNet.Port = PlcPort;
            omronFinsNet.ConnectTimeOut = TimeOut;
            //有返回值的异步调用
            Task<OperateResult> task = Task<string>.Run(() =>
            {
                OperateResult resullt = new OperateResult();
                try
                {
                    Connection = omronFinsNet.ConnectServer();
                    if (!Connection.IsSuccess)
                    {
                       // loginfo.Info(PlcIp + " 连接失败!");
                    }
                }
                catch (Exception ex) { log.Error("PLC连接失败!",ex);}
                return resullt;
            });
            ReadCount = -1;
            if (Connection.IsSuccess)
            {
                ConnectionStatus = true;
                return true;
            }
            else
            {
               ConnectionStatus = false;              
                return false;
            }
        }

        public Boolean DisConnect()
        {
            if (Connection.IsSuccess)
            {
                Task<string> task = Task<string>.Run(() =>
                {
                    try
                    {
                        omronFinsNet.ConnectClose();
                    }
                    catch (Exception ex) { log.Error(ex);}
                    return Thread.CurrentThread.ManagedThreadId.ToString();
                });
            }
            return !Connection.IsSuccess;
        }

        public void writePlc()
        {
            OperateResult write = new OperateResult();

            byte[] writedata = new byte[1];

            Task<string> task = Task<string>.Run(() =>
           {
               try
               {
                   if (ConnectionStatus)
                   {
                       write = omronFinsNet.Write(PC心跳VariableAddress, writedata);
                       if (!write.IsSuccess)
                       {
                        //loginfo.Info(PC心跳VariableAddress + "写入失败!");
                       }
                   }

               }
               catch (Exception ex) { log.Error(ex); ConnectionStatus = false;}
               return Thread.CurrentThread.ManagedThreadId.ToString();
           });
        }

        public void readPlc()
        {
            if (ConnectionStatus)
            {
                try
                {
                    OperateResult<Boolean> Read1 = omronFinsNet.ReadBool(PLC心跳VariableAddress);
                    if (!Read1.IsSuccess)
                    {
                    // loginfo.Info(PLC心跳VariableAddress + "读取失败!");
                    }
                    else { ConnectionStatus = false; }
                    OperateResult<Boolean> Read2;
                    int index = 0;   
                    PLC报警VariableValue = new bool[almLen];
                    foreach (string arr in PLC报警VariableAddress)
                    {
                        Read2= omronFinsNet.ReadBool(arr);
                        PLC报警VariableValue[index] = Read2.Content;
                        if (!Read2.IsSuccess)
                        {
                         //loginfo.Info(PLC报警VariableAddress +"读取失败!");
                        }
                    }                  
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    ConnectionStatus = false;
                    throw;
                }                              
            }
        }

        //主定时线程
        private void timer1_Tick(object sender, EventArgs e)
        {
            log.Error("这是一条错误信息");
            log.Warn("这是一条警告");
            log.Info("来自于APP的info");
            log.Debug("这是一条调试信息");
            
            readPlc();

            if (ReadCount > 20)
            {
                for (int i = 0; i < almLen; i++)
                {
                    if (PLC报警VariableValue[i] != almTemp[i])
                    { almEvent[i].customerEvent(); }
                }                            
            }          
        }

        //PC端心跳生成
        private void timer2_Tick(object sender, EventArgs e)
        {
            PC心跳VariableValue = !PC心跳VariableValue;
        }

        //PLC端心跳检测
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (ConnectionStatus)
            {
                if (ReadCount >= 20)
                {
                    if (ConnectionStatusTemp == PLC心跳VariableValue)
                    {
                        timer4.Start();
                        timer4.Enabled = true;
                    }
                    else
                    { 
                        timer4.Stop();
                        timer4.Enabled = false;
                    }
                    ConnectionStatusTemp = PLC心跳VariableValue;
                }
            }
            else
            {
                timer4.Start();
                timer4.Enabled = true;
            }
        }

        //PLC端心跳状态超时
        private void timer4_Tick(object sender, EventArgs e)
        {
            DisConnect();
            timer5.Start();
            timer5.Enabled = true;
        }

        //PLC重新建立连接
        private void timer5_Tick(object sender, EventArgs e)
        {
            timer5.Stop();
            timer5.Enabled = false;
            PlcConnect();
        }


        public void Init()
        {
            //开启定时线程
            timer1.Interval = 300;
            timer1.Enabled = true;
            timer1.Start();
            timer1.Tick += new System.EventHandler(this.timer1_Tick);

            //启用PC心跳
            timer2.Interval = 350;
            timer2.Enabled = true;
            timer2.Start();
            timer2.Tick += new System.EventHandler(this.timer2_Tick);
            PLC心跳VariableValue = false;

            //PLC心跳侦测
            timer3.Interval = 500;
            timer3.Enabled = true;
            timer3.Start();
            timer3.Tick += new System.EventHandler(this.timer3_Tick);


            //PLC心跳状态计时
            timer4.Interval = 10000;
            timer4.Enabled = true;
            timer4.Tick += new System.EventHandler(this.timer4_Tick);

            //PLC重新建立连接计时
            timer5.Stop();
            timer5.Interval = 5000;
            timer5.Enabled = true;
            timer5.Tick += new System.EventHandler(this.timer5_Tick);

            log4net.Config.XmlConfigurator.Configure();
    }


    }
}

