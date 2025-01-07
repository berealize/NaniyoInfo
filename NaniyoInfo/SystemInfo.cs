using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Management;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NaniyoInfo
{
    public partial class SystemInfo : Form
    {
        public SystemInfo()
        {
            InitializeComponent();

            txtInfo.Text = "\r\n\r\n\r\n\r\n\r\n\r\nLoading System Information...";
            txtInfo.TextAlign = HorizontalAlignment.Center;
            txtInfo.Font = new Font(txtInfo.Font.Name, 20);
            
            txtInfo.SelectionStart = txtInfo.Text.Length;

            System.Threading.Thread th = new System.Threading.Thread(GetSystemInfo);
            th.Start();
        }

        public void GetSystemInfo()
        {
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine(GetComputerName());
            //sb.AppendLine(GetOSName());
            //sb.AppendLine(GetMotherBoardInfo());
            //sb.AppendLine(GetCPUInfo());
            //sb.AppendLine(GetGPUInfo());
            //sb.AppendLine(GetDiskInfo());
            //sb.AppendLine(GetRAMInfo());
            //sb.AppendLine(GetNetworkAdapter());

            if (txtInfo.InvokeRequired)
            {
                txtInfo.Invoke(new MethodInvoker(GetSystemInfo));
            }
            else
            {
                txtInfo.Text = string.Empty;
                txtInfo.TextAlign = HorizontalAlignment.Left;
                txtInfo.Font = new Font(txtInfo.Font.Name, 11);
                txtInfo.Text = sb.ToString();
                txtInfo.SelectionStart = txtInfo.Text.Length;
            }
        }

        public string GetComputerName()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("Computer Name : {0}\r\nDomain Group : {1}\r\nLogon Server : {2}\r\nLogon User : {3}", 
                            System.Environment.MachineName, 
                            System.Environment.UserDomainName, 
                            System.Environment.GetEnvironmentVariable("LogonServer").Replace("\\", ""), 
                            System.Environment.UserName));

            return sb.ToString();
        }

        public string GetOSName()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("OS Version : {0} ({1})\r\nOS Location : {2}",
                                System.Environment.OSVersion.VersionString,
                                (System.Environment.Is64BitOperatingSystem ? "x64" : "x86"),
                                System.Environment.SystemDirectory.Replace("\\system32", "")));

            return sb.ToString();
        }

        public string GetRAMInfo()
        {
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");

            foreach (ManagementObject obj in searcher.Get())
            {
                ulong u1Bytes = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
                ulong u1MB = u1Bytes / (1024 * 1024);
                ulong u1GB = u1MB / 1024;

                sb.AppendLine(string.Format("Memory : {0} MB ({1} GB)", u1MB.ToString(), u1GB.ToString()));
            }

            return sb.ToString();
        }


        public string GetMotherBoardInfo()
        {
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine(string.Format("Mother Board : {0} {1} {2}", obj["Manufacturer"], obj["Product"], obj["SerialNumber"]));
            }

            searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS ");

            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine(string.Format("Bios Info : {0} {1}", obj["Description"], obj["SerialNumber"]));
            }

            return sb.ToString();
        }

        public string GetCPUInfo()
        {
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

            int i = 0;
            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine(string.Format("CPU {0} : {1} ({2})", i.ToString(), obj["Name"].ToString(), obj["Description"].ToString()));
                i++;
            }

            return sb.ToString();
        }


        public string GetGPUInfo()
        {
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration");
            int i = 0;
            foreach (ManagementObject obj in searcher.Get())
            {
                sb.AppendLine(string.Format("GPU {0} : {1}", i.ToString(), obj["Description"].ToString()));
                i++;
            }

            return sb.ToString();
        }

        public string GetDiskInfo()
        {
            StringBuilder sb = new StringBuilder();

            DriveInfo[] drives = DriveInfo.GetDrives();

            sb.AppendLine(string.Format("Drive Information ( System Drive : {0} )", System.Environment.GetEnvironmentVariable("SystemDrive").Substring(0,1)));

            foreach (DriveInfo drive in drives)
            {
                string strTotalSize = Convert.ToInt32(drive.TotalSize / 1024 / 1024 / 1024).ToString();
                string strFreeSize = Convert.ToInt32(drive.AvailableFreeSpace / 1024 / 1024 / 1024).ToString();
                int iUsageSize = Convert.ToInt32(strTotalSize) - Convert.ToInt32(strFreeSize);
                string strUsage = iUsageSize.ToString();
                string strFormat = drive.DriveFormat;
                string strType = drive.DriveType.ToString();

                string drvInfo = string.Format(" [{0}:{1}({2})] Total : {3} GB, Usage : {4} GB, Free : {5} GB",
                                    drive.Name.Substring(0, 1),
                                    strFormat,
                                    strType,
                                    strTotalSize, 
                                    strUsage, 
                                    strFreeSize);

                sb.AppendLine(drvInfo);
            }

            return sb.ToString();
        }

        public string GetNetworkAdapter()
        {
            StringBuilder sb = new StringBuilder();

            ObjectQuery oq = new System.Management.ObjectQuery("Select * from Win32_NetworkAdapterConfiguration Where IPEnabled = 'true'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(oq);

            int i = 0;
            foreach (ManagementObject obj in searcher.Get())
            {
                string macAddress = obj["MACAddress"].ToString();
                string ipAddress = ((string[])obj["IPAddress"])[0].ToString();
                string desc = obj["Description"].ToString();

                sb.AppendLine(string.Format("Ethernet {0} IPv4 : {1}, MAC : {2} ({3})", i.ToString(), ipAddress, macAddress, desc));
                i++;
            }

            return sb.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //파일오픈창 생성 및 설정
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "인증서 선택";

            //AppData_NPKI();
            //Path_NPKI();
            //Path_x86_NPKI();

            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                string szSignCertFile = ofd.FileName;  // "SignCert.der경로"
                string szSingPriFile = "SignPri.key경로";
            

                X509Certificate2 cert = new X509Certificate2(szSignCertFile);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("[1.인증서명: {0} ]", cert.GetNameInfo(X509NameType.DnsName, false).ToString()));       // CertName 
                sb.AppendLine(string.Format("[2.인증기관: {0} ]", cert.GetNameInfo(X509NameType.DnsName, true).ToString()));        // RegOrganization
                sb.AppendLine(string.Format("[3.등록날짜: {0} ] 인증서 등록(시작)일", cert.NotBefore.ToString("yyyyMMdd")));        // RegDate 
                sb.AppendLine(string.Format("[4.만료날짜: {0} ] 인증서 만료일", cert.NotAfter.ToString("yyyyMMdd")));               // ExpDate
                sb.AppendLine(string.Format("[5.일련번호: {0} ] 16진수 문자열을 숫자로 변경 - 앞에 '0' 을 제외", cert.SerialNumber.TrimStart('0').ToString())); // SerialNo 

                sb.AppendLine(string.Format("[6.알고리즘: {0}({1}) ] ", cert.SignatureAlgorithm.FriendlyName, cert.SignatureAlgorithm.Value.ToString())); // SerialNo 
                
                sb.AppendLine(cert.ToString(true));

                txtInfo.AppendText(sb.ToString());
            }
        }

        //인증서 목록 불러오기 - AppData_NPKI
        private void AppData_NPKI()
        {
            string path = string.Empty;
            string strsub = string.Empty;

            DataTable dt = null;
            DataRow dr = null;

            dt_create01(ref dt);

            // C:\\Users\\사용자명\\AppData\\
            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToString();
            path += "\\AppData\\LocalLow\\NPKI\\yessign\\USER\\";

            System.IO.DirectoryInfo dirinfo = new System.IO.DirectoryInfo(path);
            foreach (var item in dirinfo.GetDirectories())
            {
                dr = dt.Rows.Add();
                dr["NPKI"] = item.Name.ToString();
            }

            if (dt.Rows.Count > 0)
            {
                // 폴더 열기
                System.Diagnostics.Process.Start(path);

                /*
                foreach (DataRow item in dt.Rows)
                {
                    MessageBox.Show(item["NPKI"].ToString());
                }
                */
            }
        }

        private void dt_create01(ref DataTable dt)
        {
            try
            {
                dt = new DataTable();

                dt.Columns.Add("NPKI");
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message.ToString());
            }
        }

        private void Path_x86_NPKI()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToString() + "\\NPKI\\yessign\\USER\\";

            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(path);
                if (dirinfo.GetDirectories().Count() > 0)
                {
                    // 폴더 열기
                    System.Diagnostics.Process.Start(path);
                }
            }
        }

        private void Path_NPKI()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToString() + "\\NPKI\\yessign\\USER\\";

            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(path);
                if (dirinfo.GetDirectories().Count() > 0)
                {
                    // 폴더 열기
                    System.Diagnostics.Process.Start(path);
                }
            }
        }

    }
}
