﻿using BLL.Hardware.ScanGang;
using System.Net;
using System.Text;
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

        private System.Windows.Forms.Timer detectionTimer;

        private TCPServer _tcpServer;
        private PLCClient _plcClient;
        private ScanGangBasic _scanGangBasic;
        public static byte[] BarcodeBytes { get; set; } = Array.Empty<byte>();
        public static byte[] BatchNumber { get; set; } = Array.Empty<byte>();

        private System.Windows.Forms.Timer heartbeatTimer;
        

        public Form1()
        {
            InitializeComponent();

            heartbeatTimer = new System.Windows.Forms.Timer();
            heartbeatTimer.Interval = 4000; // 1秒
            heartbeatTimer.Tick += async (s, e) => await _plcClient.SendHeartbeatAsync();

            textBox1.ReadOnly = true;
            textBox1.Text = "";
            textBox2.Enabled = false;
            button4.Enabled = false;
            detectionTimer = new System.Windows.Forms.Timer();
            detectionTimer.Interval = 5000;
            detectionTimer.Tick += DetectionTimer_Tick;
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
                    TCPServer.Mode = true;
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
                                    //确认双端涡流仪器收到
                                    await Task.Delay(20000); // 等待 20 秒
                                    int countAA = TCPServer.ScanAASuccessCount;
                                    int countBB = TCPServer.ScanBBSuccessCount;
                                    if (countAA == 0 || countBB == 0)
                                    {
                                        MessageBox.Show("双端涡流仪器未收到扫码信号", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
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
                    /* bool errorReportSuccess = await _plcClient.WriteDRegisterAsync(2135, 1);
                     if (errorReportSuccess)
                     {
                         MessageBox.Show("无法向 D2135 发送异常报告！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     }*/

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
            if (!isOn) // 进入检测模式
            {
                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2130, 1);

                if (writeSuccess)
                {
                    TCPServer.Mode = false;
                    isOn = true;
                    button2.Text = "退出检测模式";
                    label1.Text = "当前状态：检测模式";
                    button1.Enabled = false;

                    textBox2.Enabled = true;
                    button4.Enabled = true;

                    detectionTimer.Start();
                }
                else
                {
                    MessageBox.Show("无法进入检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else // 退出检测模式
            {
                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2140, 1);

                if (writeSuccess)
                {
                    isOn = false;
                    detectionTimer.Stop();
                    button2.Text = "进入检测模式";
                    label1.Text = "当前状态：待机状态";
                    button1.Enabled = true;
                }
                else
                {
                    MessageBox.Show("无法退出检测模式，PLC通信失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string lastSentBarcode = string.Empty; // 记录上次已发送的条码

        private async void DetectionTimer_Tick(object sender, EventArgs e)
        {
            while (isOn)
            {
                int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);

                if (response != null && response.Length > 0 && response[0] == 1)
                {
                    // 读取条码
                    string currentBarcode = Encoding.UTF8.GetString(Form1.BarcodeBytes ?? new byte[0]);

                    // 如果条码没有变，则不重复发送
                    if (currentBarcode != lastSentBarcode)
                    {
                        await ReadAndSendBarcode();
                        lastSentBarcode = currentBarcode; // 记录已发送的条码
                    }
                }

                int[] stopSignal = await _plcClient.ReadDRegisterAsync(2140, 1);
                if (stopSignal != null && stopSignal.Length > 0 && stopSignal[0] == 1)
                {
                    Console.WriteLine("检测模式手动停止...");
                    await StopDetectionAsync();
                    break; // 退出循环
                }

                await Task.Delay(500); // 避免高频循环
            }
        }

        private async Task ReadAndSendBarcode()
        {
            string result;
            string errStr;
            bool success = _scanGangBasic.ScanOnce(out result, out errStr);

            if (success && result != "未扫描到条码")
            {
                // 显示条码
                textBox2.Text = result;
                //转换成byte数组供后台使用
                Form1.BarcodeBytes = Encoding.UTF8.GetBytes(result);

                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2132, 3);

                if (writeSuccess)
                {
                    Console.WriteLine($"条码 {result} 扫描成功！");
                    await Task.Delay(20000); // 等待 20 秒
                    //确认双端涡流仪器收到
                    int countAA = TCPServer.ScanAASuccessCount;
                    int countBB = TCPServer.ScanBBSuccessCount;
                    if (countAA == 0 || countBB == 0)
                    {
                        MessageBox.Show("双端涡流仪器未收到扫码信号", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //  ProcessFinalTestData 进行试件判断
                    bool isTestPassed = await CheckTestResultWithTimeout(TimeSpan.FromSeconds(20));
                    int statusCode = isTestPassed ? 1 : 2;


                    await _plcClient.WriteDRegisterAsync(2138, statusCode);
                    Console.WriteLine($"试件检测结果：{(isTestPassed ? "合格" : "不合格")}，已发送至 D2138。");
                }
                else
                {
                    Console.WriteLine("无法向 PLC 发送扫码成功信息！");
                }
            }
            else
            {
                Console.WriteLine($"扫描失败：{errStr}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (!isOn)
            {
                MessageBox.Show("请先进入检测模式！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            string batchNumber = textBox2.Text.Trim();


            if (string.IsNullOrWhiteSpace(batchNumber))
            {
                MessageBox.Show("批次号不能为空，请输入有效的批次号！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 将字符串转换为 byte[]
            Form1.BatchNumber = Encoding.UTF8.GetBytes(batchNumber);
            SaveBatchNumberToFile(batchNumber);
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

      


        public bool confirmWriteSuccess;

        private async Task RunCalibrationLoop(string selectedStandardFile, CancellationToken token)
        {
            DateTime lastCycleEndTime = DateTime.Now;
            string iniPath = "C:\\system\\system.ini";

            while (currentCycle < totalCycles && !token.IsCancellationRequested)
            {
                NotifyCycleStart();

                // **等待涡流检测软件返回检测结果**
                bool isTestPassed = await CheckTestResultWithTimeout(TimeSpan.FromSeconds(20));
                int registerValue = isTestPassed ? 2 : 1;

                // **如果检测不合格，直接停止校准**
                if (!isTestPassed)
                {
                    bool writeFail = await _plcClient.WriteDRegisterAsync(2142, registerValue);
                    if (!writeFail)
                    {
                        MessageBox.Show($"写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show("本次校准不合格，停止校准。", "不合格", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    StopCalibration(true);
                    return;
                }
                // **合格后，向 PLC 写入 D2142 = 2**
                bool writeSuccess = await _plcClient.WriteDRegisterAsync(2142, registerValue);
                if (!writeSuccess)
                {
                    MessageBox.Show($"写入 D2142 失败，校准终止！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    StopCalibration(true);
                    return;
                }

                // **等待 PLC 反馈，检查 D2132 是否为 1**
                while (true)
                {
                    int[] response = await _plcClient.ReadDRegisterAsync(2132, 1);
                    if (response != null && response.Length > 0 && response[0] == 1)
                    {
                        break;  // **D2132 == 1，继续**
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

                // **更新循环计数**
                UpdateCycleCount();

                if (currentCycle >= totalCycles)
                {
                    CompleteCalibration(lastCycleEndTime, iniPath);
                    return;
                }

            }
        }

        private async Task<bool> CheckTestResultWithTimeout(TimeSpan timeout)
        {
            Console.WriteLine("开始检查测试结果...");
            Task<bool> task;

            // 根据 TCPServer.Mode 选择调用不同的方法
            if (TCPServer.Mode)
            {
                task = Task.Run(() => _tcpServer.ProcessFinalTestData());
            }
            else
            {
                task = Task.Run(() => _tcpServer.ProcessFinalFormalData());
            }

            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                Console.WriteLine("测试结果已返回。");
                return task.Result;
            }
            else
            {
                Console.WriteLine("超时未接收到测试结果。");
                MessageBox.Show("未能在20秒内接收到样棒是否合格的结果。", "超时错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void NotifyCycleStart()
        {
            MessageBox.Show($"第 {currentCycle + 1} 次校准开始！", "开始校准", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    heartbeatTimer.Start();
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



        /* private void Form1_Load(object sender, EventArgs e)
         {

         }*/


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


        private void label4_Click_1(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }




        private void SaveBatchNumberToFile(string batchNumber)
        {
            string directoryPath = @"C:\system\"; // 保存目录
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filePath = Path.Combine(directoryPath, $"{timestamp}_batch.txt");

            try
            {

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }


                string content = $"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n批次号：{batchNumber}\n-------------------\n";
                File.AppendAllText(filePath, content, Encoding.UTF8);


                MessageBox.Show($"批次号已成功保存到文件：{filePath}", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {

                MessageBox.Show($"保存批次号时发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


        
    }
}
