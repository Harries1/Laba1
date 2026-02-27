using System;
using System.IO;
using System.Management;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace HardwareMonitor
{
    public partial class MainWindow : Window
    {
        private StringBuilder reportBuilder = new StringBuilder();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            reportBuilder.Clear();

            LoadCPU();
            LoadRAM();
            LoadDisks();
            LoadVideo();
            LoadNetwork();
            LoadSystem();
        }

        // ================= CPU =================
        private void LoadCPU()
        {
            StringBuilder sb = new StringBuilder();

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine($"Модель: {obj["Name"]}");
                sb.AppendLine($"Ядер: {obj["NumberOfCores"]}");
                sb.AppendLine($"Потоков: {obj["NumberOfLogicalProcessors"]}");
                sb.AppendLine($"Частота (MHz): {obj["MaxClockSpeed"]}");
                sb.AppendLine($"Загрузка (%): {obj["LoadPercentage"]}");
            }

            CpuBox.Text = sb.ToString();
            reportBuilder.AppendLine("=== CPU ===");
            reportBuilder.AppendLine(sb.ToString());
        }

        // ================= RAM =================
        private void LoadRAM()
        {
            StringBuilder sb = new StringBuilder();

            using (var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in osSearcher.Get())
                {
                    double total = Convert.ToDouble(obj["TotalVisibleMemorySize"]) / 1024 / 1024;
                    double free = Convert.ToDouble(obj["FreePhysicalMemory"]) / 1024 / 1024;

                    sb.AppendLine($"Общий объём (GB): {total:F2}");
                    sb.AppendLine($"Свободно (GB): {free:F2}");
                    sb.AppendLine($"Используется (%): {((total - free) / total * 100):F2}");
                }
            }

            sb.AppendLine("\nПланки памяти:");

            using (var ramSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
            {
                foreach (ManagementObject obj in ramSearcher.Get())
                {
                    sb.AppendLine($"Производитель: {obj["Manufacturer"]}");
                    sb.AppendLine($"Объём (GB): {Convert.ToDouble(obj["Capacity"]) / 1024 / 1024 / 1024:F2}");
                    sb.AppendLine($"Скорость (MHz): {obj["Speed"]}");
                    sb.AppendLine("---------------------------");
                }
            }

            RamBox.Text = sb.ToString();
            reportBuilder.AppendLine("=== RAM ===");
            reportBuilder.AppendLine(sb.ToString());
        }

        // ================= DISKS =================
        private void LoadDisks()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Физические диски:");
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    sb.AppendLine($"Модель: {obj["Model"]}");
                    sb.AppendLine($"Размер (GB): {Convert.ToDouble(obj["Size"]) / 1024 / 1024 / 1024:F2}");
                    sb.AppendLine("----------------------");
                }
            }

            sb.AppendLine("\nЛогические диски:");
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    sb.AppendLine($"Диск: {obj["Name"]}");
                    sb.AppendLine($"Файловая система: {obj["FileSystem"]}");
                    sb.AppendLine($"Размер (GB): {Convert.ToDouble(obj["Size"]) / 1024 / 1024 / 1024:F2}");
                    sb.AppendLine($"Свободно (GB): {Convert.ToDouble(obj["FreeSpace"]) / 1024 / 1024 / 1024:F2}");
                    sb.AppendLine("----------------------");
                }
            }

            DiskBox.Text = sb.ToString();
            reportBuilder.AppendLine("=== DISKS ===");
            reportBuilder.AppendLine(sb.ToString());
        }

        // ================= VIDEO =================
        private void LoadVideo()
        {
            StringBuilder sb = new StringBuilder();

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine($"Название: {obj["Name"]}");
                sb.AppendLine($"Видеопамять (MB): {Convert.ToDouble(obj["AdapterRAM"]) / 1024 / 1024:F0}");
                sb.AppendLine($"Разрешение: {obj["CurrentHorizontalResolution"]}x{obj["CurrentVerticalResolution"]}");
                sb.AppendLine("----------------------");
            }

            VideoBox.Text = sb.ToString();
            reportBuilder.AppendLine("=== VIDEO ===");
            reportBuilder.AppendLine(sb.ToString());
        }

        // ================= NETWORK =================
        private void LoadNetwork()
        {
            StringBuilder sb = new StringBuilder();

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter=True");
            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine($"Название: {obj["Name"]}");
                sb.AppendLine($"MAC: {obj["MACAddress"]}");
                sb.AppendLine($"Скорость: {obj["Speed"]}");
                sb.AppendLine($"Статус: {obj["NetConnectionStatus"]}");
                sb.AppendLine("----------------------");
            }

            NetworkBox.Text = sb.ToString();
            reportBuilder.AppendLine("=== NETWORK ===");
            reportBuilder.AppendLine(sb.ToString());
        }

        // ================= SYSTEM =================
        private void LoadSystem()
        {
            StringBuilder sb = new StringBuilder();

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine($"ОС: {obj["Caption"]}");
                sb.AppendLine($"Версия: {obj["Version"]}");
                sb.AppendLine($"Архитектура: {obj["OSArchitecture"]}");
            }

            sb.AppendLine($"Имя ПК: {Environment.MachineName}");
            sb.AppendLine($"Пользователь: {Environment.UserName}");

            SystemBox.Text = sb.ToString();
            reportBuilder.AppendLine("=== SYSTEM ===");
            reportBuilder.AppendLine(sb.ToString());
        }

        // ================= EXPORT =================
        private void ExportTxt_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("report.txt", reportBuilder.ToString());
            MessageBox.Show("Экспортировано в report.txt");
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText("report.csv", reportBuilder.ToString().Replace(":", ";"));
            MessageBox.Show("Экспортировано в report.csv");
        }

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            var obj = new { Report = reportBuilder.ToString() };
            string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("report.json", json);
            MessageBox.Show("Экспортировано в report.json");
        }
    }
}