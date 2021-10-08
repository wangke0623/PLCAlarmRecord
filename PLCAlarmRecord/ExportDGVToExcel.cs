using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Office.Interop.Excel;

//安装Microsoft.Office.Interop.Excel 。这里我是按照Nuget 安装的 （引用->管理NuGet程序包->浏览-> 安装Microsoft.Office.Interop.Excel）。
//这里需要注意的是：创建Excel对象的时候，Microsoft.Office.Interop.Excel.Application xlApp = new ApplicationClass(); 这里会报错，解决方
//式是：找到引用。然后右键属性，设置嵌入互操作类型为false！

namespace Excel
{
    class ExportDGVToExcel
    {
        private const int OLDOFFICEVESION = -4143;

        private const int NEWOFFICEVESION = 56;
        /// <summary>
        /// 将DataGridView数据导出到excel
        /// </summary>
        /// <param name="StartCell">开始写入的单元格</param>
        /// <param name="myDGV">DataGridView表格</param>
        /// <param name="Path">写入的路径</param>
        /// <returns>0:成功;1:DataGridView中无记录;2:Excel无法启动;100:Cancel;9999:异常错误</returns>
        public int ExportExcel(string StartCell, DataGridView myDGV, string Path, bool WriteColumns=false)

        {
            if (Path == "")
            {
                MessageBox.Show("请输入保存文件名！");
            }
            else
            {
                // 列索引，行索引，总列数，总行数
                int ColIndex = 0, RowIndex = 0;
                int ColCount = myDGV.ColumnCount, RowCount = myDGV.RowCount;
                if (myDGV.RowCount == 0)
                {
                    return 1;
                }

                // 创建Excel对象

                Microsoft.Office.Interop.Excel.Application xlApp = new ApplicationClass();
                if (xlApp == null)
                {
                    return 2;
                }

                try
                {
                    // 创建Excel工作薄
                    Workbook xlBook = xlApp.Workbooks.Add(true);
                    Worksheet xlSheet = (Worksheet)xlBook.Worksheets[1];
                    ////Get excel Version
                    string Version = xlApp.Version;
                    //保存excel文件的格式
                    int FormatNum;
                    if (Convert.ToDouble(Version) < 12)
                    {
                        //使用Excel 97-2003
                        FormatNum = OLDOFFICEVESION;
                    }
                    else
                    {
                        //使用 excel 2007或更新
                        FormatNum = NEWOFFICEVESION;
                    }
                    // 设置标题

                    //标题所占的单元格数与DataGridView中的列数相同
                    Range range;// = xlSheet.get_Range(xlApp.Cells[1, 1], xlApp.Cells[1, ColCount]);
                    //range.MergeCells = true;//标题行的启用                 
                    //range.FormulaR1C1 = strCaption;
                    //xlApp.ActiveCell.Font.Size = 15;
                    //xlApp.ActiveCell.Font.Bold = true;
                    //xlApp.ActiveCell.HorizontalAlignment = Constants.xlCenter;

                    // 创建缓存数据
                    object[,] objData; 
                    //获取列标题
                    if (WriteColumns)
                    {
                        objData = new object[RowCount + 1, ColCount];
                        foreach (DataGridViewColumn col in myDGV.Columns)
                        {
                            objData[RowIndex, ColIndex++] = col.HeaderText;
                        }
                        // 获取数据
                        for (RowIndex = 1; RowIndex < RowCount + 1; RowIndex++)
                        {
                            for (ColIndex = 0; ColIndex < ColCount; ColIndex++)
                            {
                                //这里就是验证DataGridView单元格中的类型,如果是string或是DataTime类型,则在放入缓存时在该内容前加入" ";
                                if (myDGV[ColIndex, RowIndex - 1].ValueType == typeof(string)
                                    || myDGV[ColIndex, RowIndex - 1].ValueType == typeof(DateTime))
                                {
                                    objData[RowIndex, ColIndex] = "";
                                    if (myDGV[ColIndex, RowIndex - 1].Value != null)
                                    {
                                        objData[RowIndex, ColIndex] = "" + myDGV[ColIndex, RowIndex - 1].Value.ToString();
                                    }
                                }

                                else

                                {
                                    objData[RowIndex, ColIndex] = myDGV[ColIndex, RowIndex - 1].Value;
                                }
                            }

                            System.Windows.Forms.Application.DoEvents();
                        }
                        // 写入Excel位置              
                        range = xlSheet.get_Range(StartCell, xlApp.Cells[RowCount + 1, ColCount]);
                    }
                    else
                    {
                        objData = new object[RowCount, ColCount];
                        // 获取数据
                        for (RowIndex = 0; RowIndex < RowCount; RowIndex++)
                        {
                            for (ColIndex = 0; ColIndex < ColCount; ColIndex++)
                            {
                                //这里就是验证DataGridView单元格中的类型,如果是string或是DataTime类型,则在放入缓存时在该内容前加入" ";
                                if (myDGV[ColIndex, RowIndex].ValueType == typeof(string)
                                    || myDGV[ColIndex, RowIndex].ValueType == typeof(DateTime))
                                {
                                    objData[RowIndex, ColIndex] = "";
                                    if (myDGV[ColIndex, RowIndex].Value != null)
                                    {
                                        objData[RowIndex, ColIndex] = "" + myDGV[ColIndex, RowIndex].Value.ToString();
                                    }
                                }

                                else

                                {
                                    objData[RowIndex, ColIndex] = myDGV[ColIndex, RowIndex].Value;
                                }
                            }

                            System.Windows.Forms.Application.DoEvents();
                        }
                        // 写入Excel位置                                  
                        range = xlSheet.get_Range(StartCell, xlApp.Cells[RowCount, ColCount]);
                    }

                    //range = xlSheet.get_Range(xlApp.Cells[1, 1], xlApp.Cells[RowCount + 1, ColCount]);
                    //单元格文本格式
                    range.NumberFormatLocal = "@";
                    range.Value2 = objData;
                    //自动列宽
                    range.Cells.Select();
                    range.Cells.Columns.AutoFit();                                
                    xlBook.Saved = true;
                    xlBook.SaveAs(Path, FormatNum);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Err:" + err.Message);
                    return 9999;
                }
                finally
                {
                    xlApp.Quit();
                    GC.Collect(); //强制回收
                }
            }
            return 100;

        }
    }

}



