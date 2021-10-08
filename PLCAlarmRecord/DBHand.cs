using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;  //引入OLE命名空间
using MySql.Data.MySqlClient; //安装Connector/Net 8.0.11驱动后，在C:\Program Files (x86)\MySQL\ MySQL Connector Net 8.0.11\Assemblies\v4.5.2\MySql.Data.dll添加这个引用
using System.Data;
using System.Data.SqlClient;

namespace DB
{
    class DBHand
    {
        private SqlConnection MsCon = new SqlConnection();
        private SqlCommand MsCommand = new SqlCommand();
        private SqlDataAdapter MsdadaAdaper = new SqlDataAdapter();

        private MySqlConnection MyCon = new MySqlConnection();
        private MySqlCommand MyCommand = new MySqlCommand();
        private MySqlDataAdapter MydadaAdaper = new MySqlDataAdapter();

        private String ConnectionCString;
        private String CommandCString;
        private String _Type;
        private String _ServerName;
        private String _DataBaseName;
        private String _TableName;
        private String _User;
        private String _Passwords;
        private String _ConError;
        public enum DBType
        {
            mssql = 0,
            mysql = 1,
        };

        #region "属性"

        /// <summary>
        /// 数据库的数据源
        /// </summary>    
        public DBType Type
        {
            set
            {
                switch (value)
                {
                    case DBType.mssql:
                        _Type = "mssql";
                        break;
                    case DBType.mysql:
                        _Type = "mysql";
                        break;
                    default:
                        _Type = "mssql";
                        break;
                }
            }
            get
            {
                if (_Type == "mssql")
                {
                    return DBType.mssql;
                }
                else if (_Type == "mysql")
                {
                    return DBType.mysql;
                }
                else
                {
                    return DBType.mssql;
                }
            }
        }

        /// <summary>
        /// 数服务器名称
        /// </summary>
        public String ServerName
        {
            set { _ServerName = value; }
            get { return _ServerName; }
        }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public String DataBaseName
        {
            set { _DataBaseName = value; }
            get { return _DataBaseName; }
        }

        /// <summary>
        /// 表名
        /// </summary>
        public String TableName
        {
            set { _TableName = value; }
            get { return _TableName; }
        }

        /// <summary>
        /// 用户
        /// </summary>
        public String User
        {
            set { _User = value; }
            get { return _User; }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public String Passwords
        {
            set { _Passwords = value; }
            get { return _Passwords; }
        }

        /// <summary>
        /// 错误
        /// </summary>
        public String ConError
        {
            get { return _ConError; }
        }

        #endregion

        #region 数据库操作
        /// <summary>
        /// 连接数据库
        /// </summary>
        public void Connection()
        {
            try
            {
                if (_Type == "mssql")
                {
                    ConnectionCString = "Data Source=" + _ServerName + ";Integrated Security=SSPI;Initial Catalog=" + _DataBaseName + "; User ID= " + _User + "; Password= " + _Passwords;   //SQLNCLI11
                    //ConnectionCString = @"Persist Security Info = True; User ID = sa; Data Source = DESKTOP - S7EGHL6\CITADEL; Initial Catalog = TZ20093DataBase";
                    MsCon.ConnectionString = ConnectionCString;
                    MsCon.Open();
                }
                else if (_Type == "mysql")
                {
                    //关键点在于SslMode=None
                    ConnectionCString = "SslMode=None;Server = " + _ServerName + ";Database= " + _DataBaseName + "; User= " + _User + "; Password= " + _Passwords;
                    MyCon.ConnectionString = ConnectionCString;
                    MyCon.Open();

                }
                _ConError = "";
            }
            catch (Exception ex)
            { _ConError = ex.Message; }
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void UnConnection()
        {
            if (_Type == "mssql")
            {
                MsCommand.Connection.Close();
                MsCon.ConnectionString = null;
                MsCon.Close();
            }
            else if (_Type == "mysql")
            {
                MyCommand.Connection.Close();
                MyCon.ConnectionString = null;
                MyCon.Close();
            }
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        public void CreateDataBase()
        {

            CommandCString = "CREATE DATABASE" + _DataBaseName;
            try
            {
                if (_Type == "mssql")
                {
                    MsCommand.Connection = MsCon;
                    MsCommand.CommandType = CommandType.Text;
                    MsCommand.CommandText = CommandCString;
                }
                else if (_Type == "mysql")
                {
                    MyCommand.Connection = MyCon;
                    MyCommand.CommandType = CommandType.Text;
                    MyCommand.CommandText = CommandCString;
                }
                _ConError = "";
            }
            catch (Exception ex)
            { _ConError = ex.Message; }
        }


        /// <summary>
        /// 新建数据表
        /// </summary>
        /// <param name="ColumnName"></param>
        public void CreateTable(string[] ColumnName)
        {
            string Str = string.Empty;
            for (int I = 0; I < ColumnName.Length; I++)
                if (I < ColumnName.Length - 1)
                {
                    Str = Str + ColumnName[I] + " varchar(255),";
                }
                else
                {
                    Str = Str + ColumnName[I] + " varchar(255)";
                }
            String[,] temp = ExecuteQuery();
           
            try
            {
                if (temp == null)
                {
                    if (_Type == "mssql")
                    {
                        CommandCString = "CREATE TABLE "  + _TableName + "(" + Str + ")";
                        MsCommand.Connection = MsCon;
                        MsCommand.CommandType = CommandType.Text;
                        MsCommand.CommandText = CommandCString;
                        MsCommand.ExecuteNonQuery();
                    }
                    else if (_Type == "mysql")
                    {
                        CommandCString = "CREATE TABLE" + " " + _DataBaseName + "." + _TableName + "(" + Str + ")" + "DEFAULT CHARSET = utf8";
                        MyCommand.Connection = MyCon;
                        MyCommand.CommandType = CommandType.Text;
                        MyCommand.CommandText = CommandCString;
                        MyCommand.ExecuteNonQuery();
                    }
                    _ConError = "";

                }
                else
                {
                    if (temp.GetLength(0)==0)
                    {
                        if (_Type == "mssql")
                        {
                            CommandCString = "CREATE TABLE " + _TableName + "(" + Str + ")";
                            MsCommand.Connection = MsCon;
                            MsCommand.CommandType = CommandType.Text;
                            MsCommand.CommandText = CommandCString;
                            MsCommand.ExecuteNonQuery();
                        }
                        else if (_Type == "mysql")
                        {
                            CommandCString = "CREATE TABLE" + " " + _DataBaseName + "." + _TableName + "(" + Str + ")" + "DEFAULT CHARSET = utf8";
                            MyCommand.Connection = MyCon;
                            MyCommand.CommandType = CommandType.Text;
                            MyCommand.CommandText = CommandCString;
                            MyCommand.ExecuteNonQuery();
                        }
                        _ConError = "";
                    }
       
                }
            }
            catch (Exception ex)
            {
                _ConError = ex.Message;
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="InserstDataIn">插入数据库的数据</param>
        /// <returns></returns>
        public object InserstData(string[] InserstDataIn)
        {
            string Str = string.Empty;

            for (int I = 0; I < InserstDataIn.Length; I++)
            {
                Str = Str + "'" + InserstDataIn[I] + "',";
            }
            Str = Str.Remove(Str.Length - 1, 1);           
            try
            {
                if (_Type.ToLower() == "mssql")
                {
                    CommandCString = "INSERT INTO " + _TableName.ToLower() + " VALUES" + "(" + Str + ")";
                    MsCommand.Connection = MsCon;
                    MsCommand.CommandType = CommandType.Text;
                    MsCommand.CommandText = CommandCString;
                    MsCommand.ExecuteNonQuery();
                }
                else if (_Type.ToLower() == "mysql")
                {
                    CommandCString = "INSERT INTO " + _DataBaseName + "." + _TableName.ToLower() + " VALUES" + "(" + Str + ")";
                    MyCommand.Connection = MyCon;
                    MyCommand.CommandType = CommandType.Text;
                    MyCommand.CommandText = CommandCString;
                    MyCommand.ExecuteNonQuery();
                }
                _ConError = "";
            }
            catch (Exception ex)
            {
                _ConError = ex.Message;
            }
            return true;
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="LastName">索引列名</param>
        /// <param name="Index">索引列值</param>
        /// <param name="ColumnName">列名</param>
        /// <param name="NewData">新的数据</param>
        /// <returns></returns>
        public object UpdateData(string LastName, string Index, string[] ColumnName, string[] NewData)
        {
            string Str1 = "";
            string Str3 = "";
            string Str2 = "";                   
            for (int I = 0; I < ColumnName.Length; I++)
            {
                Str2 += ColumnName[I] + " = " + "'" + NewData[I] + "',";
            }
            Str2 = Str2.Remove(Str2.Length - 1, 1);
            Str3 = " WHERE " + LastName + " = " + Index;          
            try
            {
                if (_Type.ToLower() == "mssql")
                {
                    Str1 = "UPDATE " + _TableName.ToLower() + " Set ";
                    CommandCString = Str1 + Str2 + Str3;
                    MsCommand.Connection = MsCon;
                    MsCommand.CommandType = CommandType.Text;
                    MsCommand.CommandText = CommandCString;
                    MsCommand.ExecuteNonQuery();
                }
                else if (_Type.ToLower() == "mysql")
                {
                    Str1 = "UPDATE " + _DataBaseName + "." + _TableName.ToLower() + " Set ";
                    CommandCString = Str1 + Str2 + Str3;
                    MyCommand.Connection = MyCon;
                    MyCommand.CommandType = CommandType.Text;
                    MyCommand.CommandText = CommandCString;
                    MyCommand.ExecuteNonQuery();
                }
                _ConError = "";
            }
            catch (Exception ex)
            {
                _ConError = ex.Message;
            }
            return true;
        }

        /// <summary>
        /// 执行语句
        /// </summary>
        /// <param name="OrderStr"></param>
        /// <returns></returns>
        public string[,] ExecuteQuery(string OrderStr = "")
        {
            string[,] DataOut;
            
            using (DataSet DataSet1 = new DataSet())
            {
                try
                {
                    //CommandCString = "select count(*)  from information_schema.TABLES t where t.TABLE_SCHEMA = '" + _DataBaseName + "' and t.TABLE_NAME = '" + _TableName + "'";
                    if (_Type == "mssql")
                    {
                        if (OrderStr == string.Empty)
                        {
                            CommandCString = "SELECT Name FROM " + _DataBaseName + ".sys.Objects where name like '%" + _TableName + "%'";
                        }
                        else
                        {
                            CommandCString = OrderStr;
                        }
                        MsCommand.Connection = MsCon;
                        MsdadaAdaper.SelectCommand = MsCommand;
                        MsCommand.CommandType = CommandType.Text;
                        MsCommand.CommandText = CommandCString;
                        SqlDataReader read = MsCommand.ExecuteReader();
                        read.Close();
                        DataSet1.Clear();
                        MsdadaAdaper.Fill(DataSet1, "read");
                    }
                    else if (_Type == "mysql")
                    {
                        if (OrderStr == string.Empty)
                        {
                            CommandCString = "SELECT table_name FROM information_schema.tables WHERE table_schema = '" + _DataBaseName + "' AND table_type = 'base table' and table_name like '%" + _TableName + "%'";
                        }
                        else
                        {
                            CommandCString = OrderStr;
                        }
                        MyCommand.Connection = MyCon;
                        MydadaAdaper.SelectCommand = MyCommand;
                        MyCommand.CommandType = CommandType.Text;
                        MyCommand.CommandText = CommandCString;
                        MySqlDataReader read = MyCommand.ExecuteReader();
                        read.Close();
                        DataSet1.Clear();
                        MydadaAdaper.Fill(DataSet1, "read");
                    }                  
                    DataOut = new string[DataSet1.Tables[0].Rows.Count, DataSet1.Tables[0].Columns.Count];
                    if (DataSet1.Tables[0].Rows.Count > 0 && DataSet1.Tables[0].Columns.Count > 0)
                    {
                        for (int i = 0; i < DataSet1.Tables[0].Rows.Count; i++)
                        {
                            for (int j = 0; j < DataSet1.Tables[0].Columns.Count; j++)
                            {
                                DataOut[i, j] = DataSet1.Tables[0].Rows[i].ItemArray[j].ToString();
                            }
                        }
                    }
                    _ConError = "";
                    return DataOut;
                }
                catch (Exception ex) { _ConError = ex.Message; return null; }
            }
        }
        #endregion
    }
}
