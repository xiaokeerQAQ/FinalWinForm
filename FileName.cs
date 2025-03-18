/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1321
{
    internal class FileName
    {
    }
    *//*  private async void button1_Click(object sender, EventArgs e)
          {
              // 判断当前状态
              if (!isOn)
              {
                  Console.WriteLine("尝试启动自校准模式...");

                  // 寄存器写入 3，表示启动自校准模式
                  bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 3);

                  if (writeSuccess)
                  {
                      // 写入成功，进入自校准模式，弹出文件选择窗口
                      SelectionForm selectionForm = new SelectionForm();
                      selectionForm.ShowDialog();

                      if (selectionForm.DialogResult == DialogResult.OK)
                      {
                          // 放入样棒框
                          DialogResult result = MessageBox.Show(
                              $"系统文件：C:\\system\\system.ini\n" +
                              $"标样文件：{selectionForm.StandardFilePath}\n" +
                              $"标定循环次数：{selectionForm.CalibrationCount}\n" +
                              $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                              "放入样棒后点击确认？",
                              "放入样棒",
                              MessageBoxButtons.OKCancel,
                              MessageBoxIcon.Question
                          );

                          if (result == DialogResult.Cancel)
                          {
                              MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                              return;
                          }

                          int[] response = await _plcClient.ReadDRegisterAsync(2132,1);

                          if (response != null && response.Length >= 15)
                          {
                              int scanAreaStatus = response[0];

                              // 判断扫码区是否存在样棒或待检棒
                              if (scanAreaStatus == 1)
                              {
                                  MessageBox.Show("扫码区存在样棒或待检棒，发送扫码成功", "扫码成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                  // 发送扫码成功信号给 PLC

                                  bool confirmWriteSuccess = await _plcClient.WriteDRegisterAsync(2132, 3);
                                  if (!confirmWriteSuccess)
                                  {
                                      MessageBox.Show("无法通知 PLC 开始循环（D2132 = 3 失败）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                      return;
                                  }
                                  string selectedStandardFile = selectionForm.StandardFilePath;
                                  totalCycles = selectionForm.CalibrationCount;
                                  currentCycle = 0;

                                  isOn = true;
                                  button1.Text = "自校准模式已开启";
                                  label1.Text = "当前状态：自校准模式";
                                  button2.Enabled = false;

                                  // 启动循环任务
                                  cancellationTokenSource = new CancellationTokenSource();
                                  CancellationToken token = cancellationTokenSource.Token;
                                  Task.Run(() => RunCalibrationLoop(selectedStandardFile, token));

                              }
                              else
                              {
                                  MessageBox.Show("扫码区没有样棒或待检棒", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                  return;
                              }
                          }


                      }
                  }
                  else
                  {
                      bool errorReportSuccess = await _plcClient.WriteDRegisterAsync(2135, 1);
                      if (errorReportSuccess)
                      {
                          MessageBox.Show("无法向 D2135 发送异常报告！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                      }

                      //  MessageBox.Show("无法写入 D 寄存器！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                      StopCalibration(true);
                  }
              }
              else
              {
                  Console.WriteLine("尝试停止自校准模式...");


                  bool writeSuccess = await _plcClient.WriteDRegisterAsync(2133, 1);

                  if (writeSuccess)
                  {

                      StopCalibration(false);


                      isOn = false;
                      button1.Text = "启动自校准模式";
                      label1.Text = "当前状态：待机状态";
                      button2.Enabled = false;
                  }
                  else
                  {
                      // 写入 D 寄存器失败时，弹出错误提示
                      MessageBox.Show("无法停止自校准模式，写入 D 寄存器失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  }
              }
          }
  */
    /* private async Task<bool> StartCalibrationMode()
     {
         Console.WriteLine("尝试启动自校准模式...");

         bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 3);  // 写入启动自校准模式信号

         return writeSuccess;
     }
     private SelectionForm ShowSelectionForm()
     {
         SelectionForm selectionForm = new SelectionForm();
         selectionForm.ShowDialog();
         return selectionForm;
     }

     private bool ShowConfirmationDialog(SelectionForm selectionForm)
     {
         DialogResult result = MessageBox.Show(
             $"系统文件：C:\\system\\system.ini\n" +
             $"标样文件：{selectionForm.StandardFilePath}\n" +
             $"标定循环次数：{selectionForm.CalibrationCount}\n" +
             $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
             "放入样棒后点击确认？",
             "放入样棒",
             MessageBoxButtons.OKCancel,
             MessageBoxIcon.Question
         );

         return result == DialogResult.OK;
     }

     private async Task<bool> CheckScanArea()
     {
         // 读取 D2132 寄存器的值
         byte[] response = await _plcClient.ReadDRegisterAsync(2132);  // 从 D2132 读取数据

         if (response != null && response.Length >= 15)
         {
             byte scanAreaStatus = response[13];  

             if (scanAreaStatus == 1)
             {
                 return true;  // 扫码区有试件
             }
             else
             {
                 MessageBox.Show("扫码区没有试件", "无试件", MessageBoxButtons.OK, MessageBoxIcon.Information);
                 return false;  // 没有试件
             }
         }
         else
         {
             MessageBox.Show("无法读取 D2132 寄存器", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             return false;
         }
     }



     private void StartCalibrationLoop(SelectionForm selectionForm)
     {
         string selectedStandardFile = selectionForm.StandardFilePath;
         totalCycles = selectionForm.CalibrationCount;
         currentCycle = 0;

         isOn = true;
         button1.Text = "自校准模式已开启";
         label1.Text = "当前状态：自校准模式";
         button2.Enabled = false;

         // 启动循环任务
         cancellationTokenSource = new CancellationTokenSource();
         CancellationToken token = cancellationTokenSource.Token;
         Task.Run(() => RunCalibrationLoop(selectedStandardFile, token));
     }
     private async Task<bool> StopCalibrationMode()
     {
         Console.WriteLine("尝试停止自校准模式...");

         bool writeSuccess = await _plcClient.WriteDRegisterAsync(2133, 1);  // 写入停止自校准模式信号

         return writeSuccess;
     }
     private async void button1_Click(object sender, EventArgs e)
     {
         if (!isOn)
         {
             // 启动自校准模式
             bool startSuccess = await StartCalibrationMode();

             if (startSuccess)
             {
                 // 弹出选择框并显示确认提示
                 SelectionForm selectionForm = ShowSelectionForm();
                 if (selectionForm.DialogResult == DialogResult.OK && ShowConfirmationDialog(selectionForm))
                 {
                     // 检查扫码区
                     bool scanSuccess = await CheckScanArea();
                     if (scanSuccess)
                     {
                         MessageBox.Show("扫码区存在样棒或待检棒，发送扫码成功", "扫码成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                         // 发送扫码成功信号
                         bool confirmWriteSuccess = await _plcClient.WriteDRegisterAsync(2132, 3); // 启动循环信号
                         if (!confirmWriteSuccess)
                         {
                             MessageBox.Show("无法通知 PLC 开始循环（D2132 = 3 失败）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                             return;
                         }

                         // 启动校准循环
                         StartCalibrationLoop(selectionForm);
                     }
                     else
                     {
                         MessageBox.Show("扫码区没有样棒或待检棒", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     }
                 }
             }
             else
             {
                 MessageBox.Show("无法启动自校准模式，写入 D 寄存器失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 StopCalibration(true);
             }
         }
         else
         {
             // 停止自校准模式
             bool stopSuccess = await StopCalibrationMode();

             if (stopSuccess)
             {
                 StopCalibration(false);
                 isOn = false;
                 button1.Text = "启动自校准模式";
                 label1.Text = "当前状态：待机状态";
                 button2.Enabled = false;
             }
             else
             {
                 MessageBox.Show("无法停止自校准模式，写入 D 寄存器失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }
     }*/

    /* private async void button2_Click(object sender, EventArgs e)
     {
         if (!isOn)  // 当前未开启检测模式，点击后开启
         {
             Console.WriteLine("尝试进入检测模式...");

             // 向 D2130 发送 1，通知 PLC 开启检测模式
             bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 1);

             if (writeSuccess)
             {
                 Console.WriteLine("✅ 检测模式已开启");

                 // 更新状态
                 isOn = true;
                 button2.Text = "退出检测模式"; // 按钮显示为“退出检测模式”
                 label1.Text = "当前状态：检测模式";

                 // 禁用自校准按钮
                 button1.Enabled = false;

                 // 显示检测模式窗口 Form2
                 Form2 form2 = new Form2();
                 form2.Show();
             }
             else
             {
                 MessageBox.Show("无法进入检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }
         else  // 当前处于检测模式，点击后退出
         {
             Console.WriteLine("尝试退出检测模式...");

             // 向 D2134 发送 1，通知 PLC 关闭检测模式
             bool writeSuccess = await _plcClient.WriteDRegisterAsync(2134, 1);

             if (writeSuccess)
             {
                 Console.WriteLine("✅ 检测模式已关闭");

                 // 调用停止检测方法，关闭 Form2 并恢复 UI
                 StopDetection();
             }
             else
             {
                 MessageBox.Show("无法退出检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }
     }*/


    /* private async void button2_Click(object sender, EventArgs e)
     {
         if (!isOn)  // 当前未开启检测模式，点击后开启
         {
             Console.WriteLine("尝试进入检测模式...");

             // 向 D2130 发送 1，通知 PLC 开启检测模式
             bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 1);

             if (writeSuccess)
             {


                 // 更新状态
                 isOn = true;
                 button2.Text = "退出检测模式"; 
                 label1.Text = "当前状态：检测模式";


                 button1.Enabled = false;


                 Form2 form2 = new Form2();
                 var result = form2.ShowDialog(); 

                 if (result == DialogResult.Cancel)
                 {
                     Console.WriteLine("用户取消检测模式，恢复待机状态...");
                     await StopDetectionAsync();
                 }
             }
             else
             {
                 MessageBox.Show("无法进入检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }
         else  
         {
             Console.WriteLine("尝试退出检测模式...");


             bool writeSuccess = await _plcClient.WriteDRegisterAsync(2134, 1);

             if (writeSuccess)
             {

                 await StopDetectionAsync();
             }
             else
             {
                 MessageBox.Show("无法退出检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }
     }*//*  DateTime lastCycleEndTime = DateTime.Now;
              string iniPath = "C:\\system\\system.ini";
              //  string sampleFolder = "D:\\标样\\yangguang"; // 样管文件夹路径
              //bool isFirstCycle = true;
              //int fileIndex = 1; // 样管文件索引


              while (currentCycle < totalCycles && !token.IsCancellationRequested)
              {
                  bool isTestCompleted = _tcpServer.IsTestCompleted(); 

                  if (isTestCompleted)
                  {
                      MessageBox.Show($"第 {currentCycle + 1} 次校准合格！", "测试合格", MessageBoxButtons.OK, MessageBoxIcon.Information);

                      // 合格后，向 D2142 发送 2
                      bool writeSuccess = await _plcClient.WriteDRegisterAsync(2142, 2);
                      if (!writeSuccess)
                      {
                          MessageBox.Show("写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                          StopCalibration(true);
                          break;
                      }

                      //D2321 变为 1，表示样棒回到待检区**
                      int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);

                      if (response != null && response.Length >= 15)
                      {
                          int scanAreaStatus = response[0];

                          // 判断扫码区是否存在样棒或待检棒
                          if (scanAreaStatus == 1)
                          {
                              MessageBox.Show("扫码区存在样棒或待检棒，发送扫码成功", "扫码成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                              // 发送扫码成功信号给 PLC

                              bool confirmWriteSuccess = await _plcClient.WriteDRegisterAsync(2132, 3);
                              if (!confirmWriteSuccess)
                              {
                                  MessageBox.Show("无法通知 PLC 开始循环（D2132 = 3 失败）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                  return;
                              }
                          }
                      }
                              // 确认样棒回到待检区后，进入下一轮
                              currentCycle++;
                      if (currentCycle >= totalCycles)
                      {
                          StopCalibration(true); 
                          break;
                      }
                  }
                  else
                  {

                      bool writeSuccess = await _plcClient.WriteDRegisterAsync(2142, 1);
                      if (!writeSuccess)
                      {
                          MessageBox.Show("无法发送失败信号到 PLC，写入 D2142 失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                          return;
                      }


                      MessageBox.Show($"第 {currentCycle + 1} 次校准未合格，校准终止！", "测试未合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                      StopCalibration(true);
                      break;
                  }
              }

              currentCycle++;
              UpdateCycleLabel();*/
    /* private bool ConfirmSampleInsertion(SelectionForm selectionForm)
      {
          DialogResult result = MessageBox.Show(
              $"系统文件：C:\\system\\system.ini\n" +
              $"标样文件：{selectionForm.StandardFilePath}\n" +
              $"标定循环次数：{selectionForm.CalibrationCount}\n" +
              $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
              "放入样棒后点击确认？",
              "放入样棒",
              MessageBoxButtons.OKCancel,
              MessageBoxIcon.Question
          );

          return result == DialogResult.OK;
      }
*/
    /* private async Task<bool> ConfirmSampleInsertion(SelectionForm selectionForm)
     {
         // 弹出提示框
         DialogResult result = MessageBox.Show(
             $"系统文件：C:\\system\\system.ini\n" +
             $"标样文件：{selectionForm.StandardFilePath}\n" +
             $"标定循环次数：{selectionForm.CalibrationCount}\n" +
             $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
             "放入样棒后点击确认？",
             "放入样棒",
             MessageBoxButtons.OKCancel,
             MessageBoxIcon.Question
         );

         if (result != DialogResult.OK)
         {
             MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
             return false; 
         }

         // 用户点击确认后，检查扫码区的状态
         int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);

         if (response != null && response.Length > 0)
         {
             int scanAreaStatus = response[0];

             // 判断扫码区是否存在样棒或待检棒
             if (scanAreaStatus == 1)
             {
                 // 如果扫码区有样棒，继续执行
                 return true;
             }
             else
             {
                 // 如果扫码区没有样棒或待检棒，提示用户放入样棒
                 MessageBox.Show("扫码区没有检测到样棒或待检棒，请放入样棒。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 return false; // 返回 false，终止后续流程
             }
         }
         else
         {
             MessageBox.Show("无法读取 D2132 寄存器的值，检查 PLC 连接。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             return false; 
         }
     }*/
    /*  private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
         {
             DateTime lastCycleEndTime = DateTime.Now;
             string iniPath = "C:\\system\\system.ini";

             while (currentCycle < totalCycles && !token.IsCancellationRequested)
             {
                 // **提示当前循环开始**
                 MessageBox.Show($"第 {currentCycle + 1} 次校准开始！", "开始校准", MessageBoxButtons.OK, MessageBoxIcon.Information);

                 if (currentCycle == 0) // **第一次循环**
                 {
                     // 发送 D2132 = 3 后，等待 PLC 处理并进入校准状态
                     while (true)
                     {
                         int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);
                         if (response != null && response.Length > 0 && response[0] == 2)
                         {
                             break; // **PLC 进入校准状态，继续流程**
                         }

                         DialogResult result = MessageBox.Show("PLC 未准备好，请检查设备状态，点击确认继续等待。",
                             "等待 PLC", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                         if (result == DialogResult.Cancel)
                         {
                             MessageBox.Show("操作已取消，校准终止！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                             StopCalibration(true);
                             return;
                         }
                     }
                 }

                 // **如果不是第一次循环，等待前一次测试是否合格**
                 if (currentCycle > 0)
                 {
                     //  判断前一次测试是否合格
                     bool isTestCompleted = _tcpServer.IsTestCompleted();
                     if (!isTestCompleted) // 前一次测试不合格，终止校准
                     {
                         MessageBox.Show($"第 {currentCycle} 次校准未合格，校准终止！", "测试未合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                         StopCalibration(true);
                         return;
                     }
                 }

                 // 等待测试完成
                 bool isTestPassed = _tcpServer.IsTestCompleted();  // 获取当前测试结果
                 while (!isTestPassed) 
                 {
                     await Task.Delay(1000);  
                     isTestPassed = _tcpServer.IsTestCompleted();
                 }

                 // 检测是否合格*
                 if (isTestPassed)
                 {
                     bool testPassWriteSuccess = await _plcClient.WriteDRegisterAsync(2142, 2); // 合格后才写入 D2142 = 2
                     if (!testPassWriteSuccess)
                     {
                         MessageBox.Show("写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                         StopCalibration(true);
                         return;
                     }
                 }
                 else
                 {
                     bool testFailWriteSuccess = await _plcClient.WriteDRegisterAsync(2142, 1); // 失败时写入 D2142 = 1
                     if (!testFailWriteSuccess)
                     {
                         MessageBox.Show("无法发送失败信号到 PLC，写入 D2142 失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                         return;
                     }
                 }


                 currentCycle++;
                 UpdateCycleLabel();

                 if (currentCycle >= totalCycles)
                 {
                     MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                     DateTime validUntil = lastCycleEndTime.AddHours(2);
                     WriteDeadlineToIni(iniPath, validUntil);
                     UpdateValidUntilLabel(validUntil);

                     this.Invoke(new Action(() =>
                     {
                         button2.Enabled = true;  
                     }));

                     StopCalibration(false);
                     return;
                 }
             }
         }*/




    /*       private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
           {
               DateTime lastCycleEndTime = DateTime.Now;
               string iniPath = "C:\\system\\system.ini";

               while (currentCycle < totalCycles && !token.IsCancellationRequested)
               {
                   // 第一次循环直接开始
                   MessageBox.Show($"第 {currentCycle + 1} 次校准开始！", "开始校准", MessageBoxButtons.OK, MessageBoxIcon.Information);


                   bool firstCycleWriteSuccess = await _plcClient.WriteDRegisterAsync(2142, 2); 
                   if (!firstCycleWriteSuccess)
                   {
                       MessageBox.Show("写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                       StopCalibration(true);
                       break;
                   }

                   // 等待 D2132 为 1，表示样棒回到待检区
                   int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);
                   while (response == null || response.Length < 1 || response[0] != 1)
                   {
                       // 提示框，要求用户确认并继续等待
                       DialogResult result = MessageBox.Show("扫码区没有检测到样棒或待检棒，请检查后重新放入样棒，点击确认继续等待。",
                           "等待样棒", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                       if (result == DialogResult.Cancel)
                       {
                           MessageBox.Show("操作已取消，校准终止！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                           StopCalibration(true);
                           break;
                       }


                       response = await _plcClient.ReadDRegisterAsync(2132, 1);
                   }


                   if (response != null && response.Length >= 1 && response[0] == 1)
                   {
                       MessageBox.Show("扫码区存在样棒或待检棒，发送扫码成功", "扫码成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                       // 发送扫码成功信号给 PLC
                       bool secondCycleWriteSuccess = await _plcClient.WriteDRegisterAsync(2132, 3); // 改名为 secondCycleWriteSuccess
                       if (!secondCycleWriteSuccess)
                       {
                           MessageBox.Show("无法通知 PLC 开始循环（D2132 = 3 失败）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                           return;
                       }
                   }

                   // 在第一次循环后进行判断是否合格，并处理
                   bool isTestCompleted = _tcpServer.IsTestCompleted();
                   if (isTestCompleted)
                   {
                       // 合格后，向 D2142 发送 2
                       bool testPassWriteSuccess = await _plcClient.WriteDRegisterAsync(2142, 2); // 改名为 testPassWriteSuccess
                       if (!testPassWriteSuccess)
                       {
                           MessageBox.Show("写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                           StopCalibration(true);
                           break;
                       }

                       // 继续后续操作
                       currentCycle++;
                       if (currentCycle >= totalCycles)
                       {
                           StopCalibration(true);
                           break;
                       }
                   }
                   else
                   {
                       // 如果测试未通过，发送失败信号到 PLC
                       bool testFailWriteSuccess = await _plcClient.WriteDRegisterAsync(2142, 1); // 改名为 testFailWriteSuccess
                       if (!testFailWriteSuccess)
                       {
                           MessageBox.Show("无法发送失败信号到 PLC，写入 D2142 失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                           return;
                       }

                       MessageBox.Show($"第 {currentCycle + 1} 次校准未合格，校准终止！", "测试未合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                       StopCalibration(true);
                       break;
                   }
               }

               // 更新循环次数
               currentCycle++;
               UpdateCycleLabel();

               if (currentCycle >= totalCycles)
               {
                   MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                   DateTime validUntil = lastCycleEndTime.AddHours(2); // 计算有效期限
                   WriteDeadlineToIni(iniPath, validUntil); // 写入 system.ini
                   UpdateValidUntilLabel(validUntil); // 更新 UI

                   this.Invoke(new Action(() =>
                   {
                       button2.Enabled = true;  // 只有成功完成才启用检测模式
                   }));

                   StopCalibration(false);
               }

               // await Task.Delay(10000, token); // 等待 10 秒，进入下一次循环
           }*/
    /*   private void UpdateCycleLabel()
         {
             if (label2.InvokeRequired)
             {
                 // 如果在非UI线程，使用Invoke来回到UI线程更新
                 label2.Invoke(new Action(UpdateCycleLabel));
             }
             else
             {
                 label2.Text = $"当前循环次数：{currentCycle} / {totalCycles}";
             }
         }

         private void StopCalibration(bool isManualStop = false)
         {
             if (cancellationTokenSource != null)
             {
                 cancellationTokenSource.Cancel();
                 cancellationTokenSource.Dispose();
                 cancellationTokenSource = null;
             }

             bool isCalibrationSuccessful = (currentCycle > 0 && currentCycle >= totalCycles);

             currentCycle = 0;
             totalCycles = 0;
             isOn = false;

             this.Invoke(new Action(() =>
             {
                 button1.Text = "自校准模式关闭";
                 label1.Text = "当前状态：待机状态";
                 label2.Text = "当前循环次数：0";

                 // 手动停止 or 异常终止，都应该禁用检测模式
                 button2.Enabled = isCalibrationSuccessful && !isManualStop;
             }));
         }*/


    /*  private void StopCalibration()
      {
          if (cancellationTokenSource != null)
          {
              cancellationTokenSource.Cancel();  // 取消任务
              cancellationTokenSource.Dispose(); // 释放资源
              cancellationTokenSource = null;
          }

          currentCycle = 0;
          totalCycles = 0;
          isOn = false;

          // 在UI线程上更新
          this.Invoke(new Action(() =>
          {
              button1.Text = "自校准模式关闭";
              label1.Text = "当前状态：待机状态";
              label2.Text = "当前循环次数：0";
              button2.Enabled = true;  // 启用按钮2
          }));
      }*/




    /* private Dictionary<string, int> ReadIniValues(string iniPath, string section)
     {
         Dictionary<string, int> values = new Dictionary<string, int>();

         string[] lines = File.ReadAllLines(iniPath);
         bool inSection = false;

         foreach (string line in lines)
         {
             string trimmedLine = line.Trim();

             if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
             {
                 inSection = trimmedLine.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase);
                 continue;
             }

             if (inSection && trimmedLine.Contains("="))
             {
                 string[] parts = trimmedLine.Split('=');
                 if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int value))
                 {
                     values[parts[0].Trim()] = value;
                 }
             }
         }

         return values;
     }*/


    /*  private int ReadIniTolerance(string iniPath)
      {
          string[] lines = File.ReadAllLines(iniPath);
          foreach (string line in lines)
          {
              if (line.StartsWith("Value=") && int.TryParse(line.Split('=')[1].Trim(), out int tolerance))
              {
                  return tolerance;
              }
          }
          return 10; // 默认误差±10
      }
*/
    /*private async void button1_Click(object sender, EventArgs e)
     {
         if (!isOn)
         {
             await StartCalibrationMode();
         }
         else
         {
             await StopCalibrationMode();
         }
     }


     private async Task StartCalibrationMode()
     {
         Console.WriteLine("尝试启动自校准模式...");

         // 通知 PLC 进入自校准模式
         bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 3);
         if (!writeSuccess)
         {
             StopCalibration(true);
             return;
         }

         // 弹出文件选择窗口
         SelectionForm selectionForm = new SelectionForm();
         selectionForm.ShowDialog();

         if (selectionForm.DialogResult != DialogResult.OK)
         {
             MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
             return;
         }

         // 确认放入样棒
         bool isSampleInserted = await ConfirmSampleInsertion(selectionForm);
         if (!isSampleInserted) return; // 如果未放入样棒，则退出

         // 向 PLC 发送扫码到位信号
         await _plcClient.WriteDRegisterAsync(2132, 3);

         *//* if (!confirmWriteSuccess)
          {
              MessageBox.Show("无法通知 PLC 开始循环（D2132 = 3 失败）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return;
          }*//*
         await Task.Delay(500);

         // **检查 PLC 是否正确接收到 `D2132 = 3`**
         int[] confirmResponse = await _plcClient.ReadDRegisterAsync(2132, 1);
         if (confirmResponse == null || confirmResponse.Length < 1 || confirmResponse[0] != 3)
         {
             MessageBox.Show("PLC 没有正确收到扫码成功信号，校准无法开始！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             return;
         }
         // 启动自校准任务
         StartCalibrationTask(selectionForm);
     }

     private async Task StopCalibrationMode()
     {
         Console.WriteLine("尝试停止自校准模式...");

         // 通知 PLC 停止
         bool writeSuccess = await _plcClient.WriteDRegisterAsync(2142, 1);
         if (!writeSuccess)
         {
             MessageBox.Show("无法停止自校准模式，写入 D 寄存器失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             return;
         }

         StopCalibration(false);
         isOn = false;
         UpdateUI("启动自校准模式", "当前状态：待机状态", false);
     }


     private async Task<bool> ConfirmSampleInsertion(SelectionForm selectionForm)
     {
         while (true)
         {

             DialogResult result = MessageBox.Show(
                 $"系统文件：C:\\system\\system.ini\n" +
                 $"标样文件：{selectionForm.StandardFilePath}\n" +
                 $"标定循环次数：{selectionForm.CalibrationCount}\n" +
                 $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                 "放入样棒后点击确认？",
                 "放入样棒",
                 MessageBoxButtons.OKCancel,
                 MessageBoxIcon.Question
             );

             if (result != DialogResult.OK)
             {
                 MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 return false; 
             }


             int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);

             if (response != null)
             {
                 int scanAreaStatus = response[0];


                 if (scanAreaStatus == 1)
                 {

                     return true;
                 }
                 else
                 {

                     MessageBox.Show("扫码区没有检测到样棒或待检棒，请放入样棒。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 }
             }
             else
             {
                 MessageBox.Show("无法读取 D2132 寄存器的值，检查 PLC 连接。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }
     }


     private void StartCalibrationTask(SelectionForm selectionForm)
     {
         string selectedStandardFile = selectionForm.StandardFilePath;
         totalCycles = selectionForm.CalibrationCount;
         currentCycle = 0;

         isOn = true;
         UpdateUI("自校准模式已开启", "当前状态：自校准模式", false);

         cancellationTokenSource = new CancellationTokenSource();
         CancellationToken token = cancellationTokenSource.Token;
        //Task.Run(() => RunCalibrationLoop(selectedStandardFile, token));
     }


     private void UpdateUI(string buttonText, string statusText, bool enableButton2)
     {
         button1.Text = buttonText;
         label1.Text = statusText;
         button2.Enabled = enableButton2;
     }
*/





    /*using BLL.Hardware.ScanGang;
using System.Net;
using System.Windows.Forms;

namespace WinFormsApp1321
{
    public partial class Form1 : Form
    {
        private bool isOn = false; // 按钮状态
        private int currentCycle = 0; // 当前循环次数
        private int totalCycles = 0; // 总循环次数
        private CancellationTokenSource cancellationTokenSource; // 控制循环停止
        private bool isCalibrationMode = false;



        private TCPServer _tcpServer;
        private PLCClient _plcClient;
        private ScanGangBasic _scanGangBasic;

        public Form1()
        {
            InitializeComponent();
            // 初始化 PLC 和扫码枪
            _plcClient = new PLCClient("127.0.0.1", 6000);
            _scanGangBasic = new ScanGangBasic();

            // 初始化 TCPServer，并传入 PLC 和扫码枪实例
            _tcpServer = new TCPServer(_plcClient, _scanGangBasic);
        }


     

        private async void button1_Click(object sender, EventArgs e)
        {
            // 判断当前状态
            if (!isOn)
            {
                Console.WriteLine("尝试启动自校准模式...");

                // 寄存器写入 3，表示启动自校准模式
                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 3);

                if (writeSuccess)
                {
                    TCPServer.Mode = 1;
                    // 写入成功，进入自校准模式，弹出文件选择窗口
                    SelectionForm selectionForm = new SelectionForm();
                    selectionForm.ShowDialog();

                    if (selectionForm.DialogResult == DialogResult.OK)
                    {
                        // 放入样棒框
                        DialogResult result = MessageBox.Show(
                            $"系统文件：C:\\system\\system.ini\n" +
                            $"标样文件：{selectionForm.StandardFilePath}\n" +
                            $"标定循环次数：{selectionForm.CalibrationCount}\n" +
                            $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                            "放入样棒后点击确认？",
                            "放入样棒",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question
                        );

                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                       
                        bool sampleInserted = false;
                        while (!sampleInserted)
                        {
                            // 等待 PLC 读取扫描区的状态
                            int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);

                            if (response != null)
                            {
                                int scanAreaStatus = response[0];

                                // 判断扫码区是否存在样棒或待检棒
                                if (scanAreaStatus == 1)
                                {
                                    MessageBox.Show("扫码区存在样棒或待检棒，发送扫码成功", "扫码成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    // 发送扫码成功信号给 PLC
                                     confirmWriteSuccess = await _plcClient.WriteDRegisterAsync(2132, 3);
                                    if (!confirmWriteSuccess)
                                    {
                                        MessageBox.Show("无法通知 PLC 开始循环（D2132 = 3 失败）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }

                                    string selectedStandardFile = selectionForm.StandardFilePath;
                                    totalCycles = selectionForm.CalibrationCount;
                                    MessageBox.Show($"Total Cycles: {totalCycles}", "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    currentCycle = 0;

                                    isOn = true;
                                    button1.Text = "自校准模式已开启";
                                    label1.Text = "当前状态：自校准模式";
                                    button2.Enabled = false;

                                    // 启动循环任务
                                    cancellationTokenSource = new CancellationTokenSource();
                                    CancellationToken token = cancellationTokenSource.Token;
                                    Task.Run(() => RunCalibrationLoop(selectedStandardFile, token));

                                    sampleInserted = true; // 退出循环
                                }
                                else
                                {
                                    // 如果没有检测到样棒，显示提示框并继续等待
                                    result = MessageBox.Show("扫码区没有样棒或待检棒，请放入样棒后点击确认。",
                                                              "等待样棒", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                                    // 如果点击了取消，退出循环并结束自校准模式
                                    if (result == DialogResult.Cancel)
                                    {
                                        MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }
                                }
                            }

                            else
                            {
                                MessageBox.Show("无法读取 D2132 寄存器的值，检查 PLC 连接。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                    }
                }
                else
                {
                    bool errorReportSuccess = await _plcClient.WriteDRegisterAsync(2135, 1);
                    if (errorReportSuccess)
                    {
                        MessageBox.Show("无法向 D2135 发送异常报告！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    //  MessageBox.Show("无法写入 D 寄存器！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    StopCalibration(true);
                }
            }
            else
            {
                Console.WriteLine("尝试停止自校准模式...");


                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2133, 1);

                if (writeSuccess)
                {

                    StopCalibration(false);


                    isOn = false;
                    button1.Text = "启动自校准模式";
                    label1.Text = "当前状态：待机状态";
                    button2.Enabled = false;
                }
                else
                {
                    // 写入 D 寄存器失败时，弹出错误提示
                    MessageBox.Show("无法停止自校准模式，写入 D 寄存器失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private async void button2_Click(object sender, EventArgs e)
        {
            if (!isOn)  // 当前未开启
            {
                Console.WriteLine("尝试进入检测模式...");

                // 向 D2130 发送 1，通知 PLC 开启检测模式
                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 1);

                if (writeSuccess)
                {
                    // 更新状态
                    isOn = true;
                    button2.Text = "退出检测模式";
                    label1.Text = "当前状态：检测模式";
                    button1.Enabled = false;

                    Form2 form2 = new Form2();
                    var result = form2.ShowDialog();

                    if (result == DialogResult.Cancel)
                    {
                        Console.WriteLine("用户取消检测模式，恢复待机状态...");
                        await StopDetectionAsync();
                    }
                    else
                    {
                        // Form2 返回 OK，弹出确认框
                        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        string barcode = form2.BarcodeText;

                        DialogResult confirmationResult = MessageBox.Show(
                            $"当前时间：{currentTime}\n条码：{barcode}\n\n确认返回主界面？",
                            "确认信息",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question
                        );


                        if (confirmationResult == DialogResult.Cancel)
                        {

                            MessageBox.Show("操作已取消，退出检测模式。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await StopDetectionAsync();


                            isOn = false;
                            button2.Text = "进入检测模式";
                            label1.Text = "当前状态：待机状态";
                            button1.Enabled = true;
                        }

                        else if (confirmationResult == DialogResult.OK)
                        {
                            form2.SaveBarcodeToFile(form2.BarcodeText);
                            form2.Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("无法进入检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else  // 已经进入检测模式，点击后退出
            {
                Console.WriteLine("尝试退出检测模式...");

                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2134, 1);

                if (writeSuccess)
                {
                    await StopDetectionAsync();
                }
                else
                {
                    MessageBox.Show("无法退出检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


 
        private async Task StopDetectionAsync()
        {

            // 关闭检测模式界面（Form2）
            foreach (Form form in Application.OpenForms)
            {
                if (form is Form2)
                {
                    form.Close();
                    break;
                }
            }

            // 启用自校准按钮
            button1.Enabled = true;

            // 状态更新
            isOn = false;
            button2.Text = "进入检测模式";
            label1.Text = "当前状态：待机状态";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            *//* // 复位状态
             //isCalibrationMode = false;
             isOn = false;


             label1.Text = "当前状态：待机状态";


             button1.Enabled = true;
             button2.Enabled = true;
             button1.Text = "自校准模式关闭";
             button2.Text = "检测模式关闭";

             MessageBox.Show("系统已恢复为待机状态！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
          //   StopCalibration(true);*//*
        }

        *//*  private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
          {
              DateTime lastCycleEndTime = DateTime.Now;
              string iniPath = "C:\\system\\system.ini";

              while (currentCycle < totalCycles)
              {
                  if (token.IsCancellationRequested)
                  {
                      MessageBox.Show("自校准任务已停止！", "停止", MessageBoxButtons.OK, MessageBoxIcon.Information);
                      StopCalibration();
                      return;
                  }

                  currentCycle++;
                  UpdateCycleLabel();

                  bool isMatched = CompareIniFiles("D:\\标样\\样管1.ini", selectedStandardFile);

                  if (!isMatched)
                  {
                      MessageBox.Show("出现缺陷数据异常！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                      StopCalibration();
                      return;
                  }

                  lastCycleEndTime = DateTime.Now;  // 记录本次循环的结束时间

                  if (currentCycle >= totalCycles)
                  {
                      MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                      DateTime validUntil = lastCycleEndTime.AddHours(2); // 计算有效期限
                      WriteDeadlineToIni(iniPath, validUntil);  // 写入 system.ini
                      UpdateValidUntilLabel(validUntil); // 更新 UI

                      StopCalibration();
                  }

                  await Task.Delay(10000, token);
              }
          }*//*
        public bool confirmWriteSuccess;
        *//*private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
        {
            DateTime lastCycleEndTime = DateTime.Now;
            string iniPath = "C:\\system\\system.ini";

            while (currentCycle < totalCycles && !token.IsCancellationRequested)
            {
                NotifyCycleStart();

                if (currentCycle == 0)
                {
                    if (!await WaitForPLCReady() || !CheckFirstTest())
                    {
                        StopCalibration(true);
                        return;
                    }
                }
                else if (!CheckPreviousTest())
                {
                    StopCalibration(true);
                    return;
                }

                if (!await WaitForTestCompletion())
                {
                    StopCalibration(true);
                    return;
                }

                if (!await HandleTestResult())
                {
                    StopCalibration(true);
                    return;
                }

                UpdateCycleCount();

                if (currentCycle >= totalCycles)
                {
                    CompleteCalibration(lastCycleEndTime, iniPath);
                    return;
                }
            }
        }*//*

        private void NotifyCycleStart()
        {
            MessageBox.Show($"第 {currentCycle + 1} 次校准开始！", "开始校准", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task<bool> WaitForPLCReady()
        {
            while (true)
            {
                int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);
                if (response != null && response.Length > 0 && response[0] == 3)
                {
                    return true;
                }

                if (MessageBox.Show("PLC 未准备好，请检查设备状态，点击确认继续等待。", "等待 PLC", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    MessageBox.Show("操作已取消，校准终止！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
        }

        private bool CheckFirstTest()
        {
            if (!_tcpServer.IsTestCompleted())
            {
                MessageBox.Show("第一次校准未合格，校准终止！", "测试未合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool CheckPreviousTest()
        {
            if (!_tcpServer.IsTestCompleted())
            {
                MessageBox.Show($"第 {currentCycle} 次校准未合格，校准终止！", "测试未合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private async Task<bool> WaitForTestCompletion()
        {
            while (!_tcpServer.IsTestCompleted())
            {
                await Task.Delay(1000);
            }
            return true;
        }

        private async Task<bool> HandleTestResult()
        {
            bool isTestPassed = _tcpServer.IsTestCompleted();
            int registerValue = isTestPassed ? 2 : 1;
            bool writeSuccess = await _plcClient.WriteDRegisterAsync(2142, registerValue);

            if (!writeSuccess)
            {
                MessageBox.Show($"写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void UpdateCycleCount()
        {
            currentCycle++;
            UpdateCycleLabel();
        }

        private void CompleteCalibration(DateTime lastCycleEndTime, string iniPath)
        {
            MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DateTime validUntil = lastCycleEndTime.AddHours(2);
            WriteDeadlineToIni(iniPath, validUntil);
            UpdateValidUntilLabel(validUntil);

            this.Invoke(new Action(() => button2.Enabled = true));

            StopCalibration(false);
        }



        private void WriteDeadlineToIni(string iniPath, DateTime deadline)
        {
            try
            {
                List<string> lines = new List<string>();

                if (File.Exists(iniPath))
                {
                    lines = File.ReadAllLines(iniPath).ToList();
                }

                bool found = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("Deadline="))
                    {
                        lines[i] = $"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"; // 直接更新
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    lines.Add($"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"); // 确保一行
                }

                File.WriteAllLines(iniPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void UpdateValidUntilLabel(DateTime validUntil)
        {
            if (label3.InvokeRequired)
            {
                label3.Invoke(new Action(() => UpdateValidUntilLabel(validUntil)));
            }
            else
            {
                label3.Text = $"检测有效期限：{validUntil:yyyy-MM-dd HH:mm:ss}";
            }
        }



        private DateTime ReadDeadlineFromIni(string iniPath)
        {
            try
            {
                if (!File.Exists(iniPath))
                    return DateTime.MinValue;

                string[] lines = File.ReadAllLines(iniPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("Deadline="))
                    {
                        string deadlineStr = line.Split('=')[1].Trim();
                        if (DateTime.TryParse(deadlineStr, out DateTime deadline))
                        {
                            return deadline;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DateTime.MinValue;
        }


        private async void Form1_Load(object sender, EventArgs e)
        {
            string iniPath = "C:\\system\\system.ini";
            DateTime deadline = ReadDeadlineFromIni(iniPath);
            if (deadline != DateTime.MinValue)
            {
                UpdateValidUntilLabel(deadline);
            }

            Task.Run(() => CheckDeadline()); // 启动检查任务

            try
            {

                // 连接 PLC
                bool plcConnected = await _plcClient.ConnectAsync();
                if (plcConnected)
                {
                    Console.WriteLine("PLC 连接成功");
                }
                else
                {
                    Console.WriteLine("PLC 连接失败");
                }


                // 连接扫码枪
                string scannerIp = "127.0.0.1"; // 你的扫码枪 IP
                int scannerPort = 5001; // 端口号
                string deviceId = "Scanner_01"; // 设备 ID
                string errorMessage = string.Empty;
                bool scannerConnected = _scanGangBasic.Connect(scannerIp, scannerPort, deviceId, out errorMessage);
                if (scannerConnected)
                {
                    Console.WriteLine("扫码枪连接成功");
                }
                else
                {
                    Console.WriteLine("扫码枪连接失败");
                }

                // 启动 TCP 服务器
                await _tcpServer.StartWoLiuAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化失败: {ex.Message}");
            }
        }


        private async void CheckDeadline()
        {
            while (true)
            {
                string iniPath = "C:\\system\\system.ini";
                DateTime deadline = ReadDeadlineFromIni(iniPath);
                DateTime now = DateTime.Now;

                if (deadline != DateTime.MinValue)
                {
                    TimeSpan remaining = deadline - now;

                    if (remaining.TotalMinutes <= 60 && remaining.TotalMinutes > 59)
                    {
                        MessageBox.Show("检测有效期即将到期！剩余不到 1 小时。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (remaining.TotalSeconds <= 0)
                    {
                        MessageBox.Show("检测有效期已过期！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // 使用 Invoke 确保 UI 线程操作
                        if (button2.InvokeRequired)
                        {
                            button2.Invoke(new Action(() => button2.Enabled = false));
                        }
                        else
                        {
                            button2.Enabled = false;
                        }
                    }
                    *//* else
                     {
                         // 使用 Invoke 确保 UI 线程操作
                         if (button2.InvokeRequired)
                         {
                             button2.Invoke(new Action(() => button2.Enabled = true));
                         }
                         else
                         {
                            button2.Enabled = true;
                         }
                     }*//*
                }

                await Task.Delay(1800000); // 每 30fz检查一次
            }
        }



        private void UpdateCycleLabel()
        {
            // 更新当前循环次数和总循环次数
            if (label2.InvokeRequired)
            {
                // 如果在非UI线程，使用Invoke来回到UI线程更新
                label2.Invoke(new Action(UpdateCycleLabel));
            }
            else
            {
                label2.Text = $"当前循环次数: {currentCycle}";
            }
        }


        private void StopCalibration(bool isManualStop = false)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            bool isCalibrationSuccessful = (currentCycle > 0 && currentCycle >= totalCycles);

            currentCycle = 0;
            totalCycles = 0;
            isOn = false;

            this.Invoke(new Action(() =>
            {
                button1.Text = "自校准模式关闭";
                label1.Text = "当前状态：待机状态";
                label2.Text = "当前循环次数：0";

                // 手动停止 or 异常终止，都应该禁用检测模式
                button2.Enabled = isCalibrationSuccessful && !isManualStop;
            }));
        }

      
        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }



        *//* private void Form1_Load(object sender, EventArgs e)
         {

         }*//*


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
*//*
using BLL.Hardware.ScanGang;
using System.Net;
using System.Windows.Forms;

namespace WinFormsApp1321
    {
        public partial class Form1 : Form
        {
            private bool isOn = false; // 按钮状态
            private int currentCycle = 0; // 当前循环次数
            private int totalCycles = 0; // 总循环次数
            private CancellationTokenSource cancellationTokenSource; // 控制循环停止
            private bool isCalibrationMode = false;



            private TCPServer _tcpServer;
            private PLCClient _plcClient;
            private ScanGangBasic _scanGangBasic;

            public Form1()
            {
                InitializeComponent();
                // 初始化 PLC 和扫码枪
                _plcClient = new PLCClient("127.0.0.1", 6000);
                _scanGangBasic = new ScanGangBasic();

                // 初始化 TCPServer，并传入 PLC 和扫码枪实例
                _tcpServer = new TCPServer(_plcClient, _scanGangBasic);
            }




            private async void button1_Click(object sender, EventArgs e)
            {
                // 判断当前状态
                if (!isOn)
                {
                    Console.WriteLine("尝试启动自校准模式...");

                    // 寄存器写入 3，表示启动自校准模式
                    bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 3);

                    if (writeSuccess)
                    {
                        TCPServer.Mode = 1;
                        // 写入成功，进入自校准模式，弹出文件选择窗口
                        SelectionForm selectionForm = new SelectionForm();
                        selectionForm.ShowDialog();

                        if (selectionForm.DialogResult == DialogResult.OK)
                        {
                            // 放入样棒框
                            DialogResult result = MessageBox.Show(
                                $"系统文件：C:\\system\\system.ini\n" +
                                $"标样文件：{selectionForm.StandardFilePath}\n" +
                                $"标定循环次数：{selectionForm.CalibrationCount}\n" +
                                $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                "放入样棒后点击确认？",
                                "放入样棒",
                                MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question
                            );

                            if (result == DialogResult.Cancel)
                            {
                                MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }


                            bool sampleInserted = false;
                            while (!sampleInserted)
                            {
                                // 等待 PLC 读取扫描区的状态
                                int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);

                                if (response != null)
                                {
                                    int scanAreaStatus = response[0];

                                    // 判断扫码区是否存在样棒或待检棒
                                    if (scanAreaStatus == 1)
                                    {
                                        MessageBox.Show("扫码区存在样棒或待检棒，发送扫码成功", "扫码成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        // 发送扫码成功信号给 PLC
                                        confirmWriteSuccess = await _plcClient.WriteDRegisterAsync(2132, 3);
                                        if (!confirmWriteSuccess)
                                        {
                                            MessageBox.Show("无法通知 PLC 开始循环（D2132 = 3 失败）", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            return;
                                        }

                                        string selectedStandardFile = selectionForm.StandardFilePath;
                                        totalCycles = selectionForm.CalibrationCount;
                                        MessageBox.Show($"Total Cycles: {totalCycles}", "Debug Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                        currentCycle = 0;

                                        isOn = true;
                                        button1.Text = "自校准模式已开启";
                                        label1.Text = "当前状态：自校准模式";
                                        button2.Enabled = false;

                                        // 启动循环任务
                                        cancellationTokenSource = new CancellationTokenSource();
                                        CancellationToken token = cancellationTokenSource.Token;
                                        Task.Run(() => RunCalibrationLoop(selectedStandardFile, token));

                                        sampleInserted = true; // 退出循环
                                    }
                                    else
                                    {
                                        // 如果没有检测到样棒，显示提示框并继续等待
                                        result = MessageBox.Show("扫码区没有样棒或待检棒，请放入样棒后点击确认。",
                                                                  "等待样棒", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                                        // 如果点击了取消，退出循环并结束自校准模式
                                        if (result == DialogResult.Cancel)
                                        {
                                            MessageBox.Show("操作已取消，自校准模式未开启。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            return;
                                        }
                                    }
                                }

                                else
                                {
                                    MessageBox.Show("无法读取 D2132 寄存器的值，检查 PLC 连接。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                        }
                    }
                    else
                    {
                        *//* bool errorReportSuccess = await _plcClient.WriteDRegisterAsync(2135, 1);
                         if (errorReportSuccess)
                         {
                             MessageBox.Show("无法向 D2135 发送异常报告！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                         }*//*

                        //  MessageBox.Show("无法写入 D 寄存器！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        StopCalibration(true);
                    }
                }
                else
                {
                    Console.WriteLine("尝试停止自校准模式...");


                    bool writeSuccess = await _plcClient.WriteDRegisterAsync(2133, 1);

                    if (writeSuccess)
                    {

                        StopCalibration(false);


                        isOn = false;
                        button1.Text = "启动自校准模式";
                        label1.Text = "当前状态：待机状态";
                        button2.Enabled = false;
                    }
                    else
                    {
                        // 写入 D 寄存器失败时，弹出错误提示
                        MessageBox.Show("无法停止自校准模式，写入 D 寄存器失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            private async void button2_Click(object sender, EventArgs e)
            {
                if (!isOn)  // 当前未开启
                {
                    Console.WriteLine("尝试进入检测模式...");

                    // 向 D2130 发送 1，通知 PLC 开启检测模式
                    bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 1);

                    if (writeSuccess)
                    {
                        // 更新状态
                        isOn = true;
                        button2.Text = "退出检测模式";
                        label1.Text = "当前状态：检测模式";
                        button1.Enabled = false;

                        Form2 form2 = new Form2();
                        var result = form2.ShowDialog();

                        if (result == DialogResult.Cancel)
                        {
                            Console.WriteLine("用户取消检测模式，恢复待机状态...");
                            await StopDetectionAsync();
                        }
                        else
                        {
                            // Form2 返回 OK，弹出确认框
                            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string barcode = form2.BarcodeText;

                            DialogResult confirmationResult = MessageBox.Show(
                                $"当前时间：{currentTime}\n条码：{barcode}\n\n确认返回主界面？",
                                "确认信息",
                                MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question
                            );


                            if (confirmationResult == DialogResult.Cancel)
                            {

                                MessageBox.Show("操作已取消，退出检测模式。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await StopDetectionAsync();


                                isOn = false;
                                button2.Text = "进入检测模式";
                                label1.Text = "当前状态：待机状态";
                                button1.Enabled = true;
                            }

                            else if (confirmationResult == DialogResult.OK)
                            {
                                form2.SaveBarcodeToFile(form2.BarcodeText);
                                form2.Close();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("无法进入检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else  // 已经进入检测模式，点击后退出
                {
                    Console.WriteLine("尝试退出检测模式...");

                    bool writeSuccess = await _plcClient.WriteDRegisterAsync(2134, 1);

                    if (writeSuccess)
                    {
                        await StopDetectionAsync();
                    }
                    else
                    {
                        MessageBox.Show("无法退出检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }



            private async Task StopDetectionAsync()
            {

                // 关闭检测模式界面（Form2）
                foreach (Form form in Application.OpenForms)
                {
                    if (form is Form2)
                    {
                        form.Close();
                        break;
                    }
                }

                // 启用自校准按钮
                button1.Enabled = true;

                // 状态更新
                isOn = false;
                button2.Text = "进入检测模式";
                label1.Text = "当前状态：待机状态";
            }

            private void button3_Click(object sender, EventArgs e)
            {
                *//* // 复位状态
                 //isCalibrationMode = false;
                 isOn = false;


                 label1.Text = "当前状态：待机状态";


                 button1.Enabled = true;
                 button2.Enabled = true;
                 button1.Text = "自校准模式关闭";
                 button2.Text = "检测模式关闭";

                 MessageBox.Show("系统已恢复为待机状态！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
              //   StopCalibration(true);*//*
            }

            *//*  private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
              {
                  DateTime lastCycleEndTime = DateTime.Now;
                  string iniPath = "C:\\system\\system.ini";

                  while (currentCycle < totalCycles)
                  {
                      if (token.IsCancellationRequested)
                      {
                          MessageBox.Show("自校准任务已停止！", "停止", MessageBoxButtons.OK, MessageBoxIcon.Information);
                          StopCalibration();
                          return;
                      }

                      currentCycle++;
                      UpdateCycleLabel();

                      bool isMatched = CompareIniFiles("D:\\标样\\样管1.ini", selectedStandardFile);

                      if (!isMatched)
                      {
                          MessageBox.Show("出现缺陷数据异常！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                          StopCalibration();
                          return;
                      }

                      lastCycleEndTime = DateTime.Now;  // 记录本次循环的结束时间

                      if (currentCycle >= totalCycles)
                      {
                          MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                          DateTime validUntil = lastCycleEndTime.AddHours(2); // 计算有效期限
                          WriteDeadlineToIni(iniPath, validUntil);  // 写入 system.ini
                          UpdateValidUntilLabel(validUntil); // 更新 UI

                          StopCalibration();
                      }

                      await Task.Delay(10000, token);
                  }
              }*//*
            public bool confirmWriteSuccess;
            private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
            {
                DateTime lastCycleEndTime = DateTime.Now;
                string iniPath = "C:\\system\\system.ini";

                while (currentCycle < totalCycles && !token.IsCancellationRequested)
                {
                    NotifyCycleStart();

                    if (currentCycle == 0)
                    {
                        if (!await WaitForPLCReady() || !CheckFirstTest())
                        {
                            StopCalibration(true);
                            return;
                        }
                    }
                    else if (!CheckPreviousTest())
                    {
                        StopCalibration(true);
                        return;
                    }

                    if (!await WaitForTestCompletion())
                    {
                        StopCalibration(true);
                        return;
                    }

                    if (!await HandleTestResult())
                    {
                        StopCalibration(true);
                        return;
                    }

                    UpdateCycleCount();

                    if (currentCycle >= totalCycles)
                    {
                        CompleteCalibration(lastCycleEndTime, iniPath);
                        return;
                    }
                }
            }

            private void NotifyCycleStart()
            {
                MessageBox.Show($"第 {currentCycle + 1} 次校准开始！", "开始校准", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            private async Task<bool> WaitForPLCReady()
            {
                while (true)
                {
                    int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);
                    if (response != null && response.Length > 0 && response[0] == 3)
                    {
                        return true;
                    }

                    if (MessageBox.Show("PLC 未准备好，请检查设备状态，点击确认继续等待。", "等待 PLC", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                    {
                        MessageBox.Show("操作已取消，校准终止！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }

            private bool CheckFirstTest()
            {
                if (!_tcpServer.IsTestCompleted())
                {
                    MessageBox.Show("第一次校准未合格，校准终止！", "测试未合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }

            private bool CheckPreviousTest()
            {
                if (!_tcpServer.IsTestCompleted())
                {
                    MessageBox.Show($"第 {currentCycle} 次校准未合格，校准终止！", "测试未合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }

            private async Task<bool> WaitForTestCompletion()
            {
                while (!_tcpServer.IsTestCompleted())
                {
                    await Task.Delay(1000);
                }
                return true;
            }

            private async Task<bool> HandleTestResult()
            {
                bool isTestPassed = _tcpServer.IsTestCompleted();
                int registerValue = isTestPassed ? 2 : 1;
                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2142, registerValue);

                if (!writeSuccess)
                {
                    MessageBox.Show($"写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                return true;
            }

            private void UpdateCycleCount()
            {
                currentCycle++;
                UpdateCycleLabel();
            }

            private void CompleteCalibration(DateTime lastCycleEndTime, string iniPath)
            {
                MessageBox.Show("检测完成！所有循环已执行。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DateTime validUntil = lastCycleEndTime.AddHours(2);
                WriteDeadlineToIni(iniPath, validUntil);
                UpdateValidUntilLabel(validUntil);

                this.Invoke(new Action(() => button2.Enabled = true));

                StopCalibration(false);
            }



            private void WriteDeadlineToIni(string iniPath, DateTime deadline)
            {
                try
                {
                    List<string> lines = new List<string>();

                    if (File.Exists(iniPath))
                    {
                        lines = File.ReadAllLines(iniPath).ToList();
                    }

                    bool found = false;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].StartsWith("Deadline="))
                        {
                            lines[i] = $"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"; // 直接更新
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        lines.Add($"Deadline={deadline:yyyy-MM-dd HH:mm:ss}"); // 确保一行
                    }

                    File.WriteAllLines(iniPath, lines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"写入系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }



            private void UpdateValidUntilLabel(DateTime validUntil)
            {
                if (label3.InvokeRequired)
                {
                    label3.Invoke(new Action(() => UpdateValidUntilLabel(validUntil)));
                }
                else
                {
                    label3.Text = $"检测有效期限：{validUntil:yyyy-MM-dd HH:mm:ss}";
                }
            }



            private DateTime ReadDeadlineFromIni(string iniPath)
            {
                try
                {
                    if (!File.Exists(iniPath))
                        return DateTime.MinValue;

                    string[] lines = File.ReadAllLines(iniPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Deadline="))
                        {
                            string deadlineStr = line.Split('=')[1].Trim();
                            if (DateTime.TryParse(deadlineStr, out DateTime deadline))
                            {
                                return deadline;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取系统文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return DateTime.MinValue;
            }


            private async void Form1_Load(object sender, EventArgs e)
            {
                string iniPath = "C:\\system\\system.ini";
                DateTime deadline = ReadDeadlineFromIni(iniPath);
                if (deadline != DateTime.MinValue)
                {
                    UpdateValidUntilLabel(deadline);
                }

                Task.Run(() => CheckDeadline()); // 启动检查任务

                try
                {

                    // 连接 PLC
                    bool plcConnected = await _plcClient.ConnectAsync();
                    if (plcConnected)
                    {
                        Console.WriteLine("PLC 连接成功");
                    }
                    else
                    {
                        Console.WriteLine("PLC 连接失败");
                    }


                    // 连接扫码枪
                    string scannerIp = "127.0.0.1"; // 你的扫码枪 IP
                    int scannerPort = 5001; // 端口号
                    string deviceId = "Scanner_01"; // 设备 ID
                    string errorMessage = string.Empty;
                    bool scannerConnected = _scanGangBasic.Connect(scannerIp, scannerPort, deviceId, out errorMessage);
                    if (scannerConnected)
                    {
                        Console.WriteLine("扫码枪连接成功");
                    }
                    else
                    {
                        Console.WriteLine("扫码枪连接失败");
                    }

                    // 启动 TCP 服务器
                    await _tcpServer.StartWoLiuAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"初始化失败: {ex.Message}");
                }
            }


            private async void CheckDeadline()
            {
                while (true)
                {
                    string iniPath = "C:\\system\\system.ini";
                    DateTime deadline = ReadDeadlineFromIni(iniPath);
                    DateTime now = DateTime.Now;

                    if (deadline != DateTime.MinValue)
                    {
                        TimeSpan remaining = deadline - now;

                        if (remaining.TotalMinutes <= 60 && remaining.TotalMinutes > 59)
                        {
                            MessageBox.Show("检测有效期即将到期！剩余不到 1 小时。", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (remaining.TotalSeconds <= 0)
                        {
                            MessageBox.Show("检测有效期已过期！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // 使用 Invoke 确保 UI 线程操作
                            if (button2.InvokeRequired)
                            {
                                button2.Invoke(new Action(() => button2.Enabled = false));
                            }
                            else
                            {
                                button2.Enabled = false;
                            }
                        }
                        *//* else
                         {
                             // 使用 Invoke 确保 UI 线程操作
                             if (button2.InvokeRequired)
                             {
                                 button2.Invoke(new Action(() => button2.Enabled = true));
                             }
                             else
                             {
                                button2.Enabled = true;
                             }
                         }*//*
                    }

                    await Task.Delay(1800000); // 每 30fz检查一次
                }
            }



            private void UpdateCycleLabel()
            {
                // 更新当前循环次数和总循环次数
                if (label2.InvokeRequired)
                {
                    // 如果在非UI线程，使用Invoke来回到UI线程更新
                    label2.Invoke(new Action(UpdateCycleLabel));
                }
                else
                {
                    label2.Text = $"当前循环次数: {currentCycle}";
                }
            }


            private void StopCalibration(bool isManualStop = false)
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                }

                bool isCalibrationSuccessful = (currentCycle > 0 && currentCycle >= totalCycles);

                currentCycle = 0;
                totalCycles = 0;
                isOn = false;

                this.Invoke(new Action(() =>
                {
                    button1.Text = "自校准模式关闭";
                    label1.Text = "当前状态：待机状态";
                    label2.Text = "当前循环次数：0";

                    // 手动停止 or 异常终止，都应该禁用检测模式
                    button2.Enabled = isCalibrationSuccessful && !isManualStop;
                }));
            }


            private void toolStripComboBox1_Click(object sender, EventArgs e)
            {

            }

            private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
            {

            }

            private void label1_Click(object sender, EventArgs e)
            {

            }



            *//* private void Form1_Load(object sender, EventArgs e)
             {

             }*//*


            private void label2_Click(object sender, EventArgs e)
            {

            }

            private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
            {

            }

            private void label3_Click(object sender, EventArgs e)
            {

            }

            private void label4_Click(object sender, EventArgs e)
            {

            }
        }
    }

}
*/