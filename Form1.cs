using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class Form1 : Form
    {
        public static IWebDriver driver = new ChromeDriver(@"C:\Program Files (x86)\Google\Chrome\Application");
        public static string firmware = "";
        public string version = "AABBCC";
        public string filepathFirmware = @"C:\Firmware\tclinux.bin";
        public Form1()
        {
            InitializeComponent();
        }
        
        private void btnConnect_Click(object sender, EventArgs e)
        {
            PingIP();
        }
        private void PingIP()
        {
            Log("ping 192.168.1.1...");
            Ping myPing = new Ping();
            PingReply reply = myPing.Send("192.168.1.1", 1000);
            if (reply != null)
            {
                Log("Status :  " + reply.Status  + "\n Time : " + reply.RoundtripTime.ToString() + "\n Address : " + reply.Address);
            }
            if (reply.Status == 0)
            {
                // MessageBox.Show("Ping success");
                WriteFile();
                string MACAddress = GetMAC();
                Log("MAC address: " + MACAddress);
                Selenium();
                if(GetFirmware()!=version)
                {
                    UpgradeFirmware(filepathFirmware);
                }
                else
                {
                    Restart();
                }
            }
            else
            {
                Log("Please wait...");
                Thread.Sleep(5000);
                PingIP();

            }
        }
        private void Log(string text)
        {
            txtLog1.AppendText("["+DateTime.Now+"]  "+ text + "\r\n");
        }
        private void WriteFile()
        {
            Process ps = new Process();
            ps.StartInfo.FileName = "cmd.exe";
            ps.StartInfo.Arguments = @"/K arp -a";
            ps.StartInfo.RedirectStandardOutput = true;
            ps.StartInfo.UseShellExecute = false;
            ps.Start();
            string output = ps.StandardOutput.ReadToEnd().Trim();
            ps.WaitForExit();
            //MessageBox.Show(output);
            string[] lines = new[] { output };

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\Users\QuynhDam\Documents\Visual Studio 2015\Projects\WindowsFormsApp\WindowsFormsApp\WriteLines.txt"))
            {
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
            }
        }
        private static string GetMAC()
        {
            string lineReplace = String.Empty;
            string lineSub = String.Empty;
            string MAC = String.Empty;
            using (var streamReader = File.OpenText(@"C:\Users\QuynhDam\Documents\Visual Studio 2015\Projects\WindowsFormsApp\WindowsFormsApp\WriteLines.txt"))
            {
                var text = streamReader.ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in text)
                {

                    if (line.Contains("192.168.1.1 "))
                    {
                        //MessageBox.Show(line);
                        string lineTrim = line.Trim();
                        //Regex rg = new Regex(@"\s+");
                        //rg.Replace(line, " ");
                        lineReplace = lineTrim.Replace(" ", "");
                        lineSub = lineReplace.Substring(14, 14);
                        MAC = lineSub.Replace("-", "").ToUpper();
                        
                        // MessageBox.Show(MAC);
                    }

                }
                
                return MAC;

            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void Login(string user, string pass)
        {
         
            IWebElement queryUser = driver.FindElement(By.Id("username"));
            queryUser.SendKeys(user);
            IWebElement queryPass = driver.FindElement(By.Id("password"));
            queryPass.SendKeys(pass);
            IWebElement querySubmit = driver.FindElement(By.Name("btnsubmit"));
            querySubmit.Click();
            string TxtTitle_Login = "GPON Home Gateway";
            string title_Login = driver.Title;
            if (title_Login == TxtTitle_Login)
            {
                Console.WriteLine("Logged");
                string s= GetFirmware();
                Log("Firmware version: " + s);
                
            }
            else
            {

                Console.WriteLine("Loggin fail");
                Login(user, pass);
            }

        }
        private string GetFirmware()
        {
            driver.Url = "http://192.168.1.1/cgi-bin/status_deviceinfo.asp";
            IWebElement queryFirmware = driver.FindElement(By.CssSelector("#block1 > table:nth-child(2) > tbody > tr:nth-child(3) > td:nth-child(3)"));
            firmware = queryFirmware.Text;
            return firmware;
        }
        private void Selenium()
        {
            string TxtTitle_Start = "login";
            //IWebDriver driver = new ChromeDriver(@"C:\Program Files (x86)\Google\Chrome\Application");
            driver.Url = "http://192.168.1.1/";
            string title_Redirect = driver.Title;
            if (title_Redirect == TxtTitle_Start)
            {
                Console.WriteLine("Redirect success");
                Login("admin", GetMAC());

            }
            else
                Console.WriteLine("Redirect fail");
        }
        private void UpgradeFirmware(string filepath)
        {
         
            driver.Url = "http://192.168.1.1/cgi-bin/tools_update.asp";
            IWebElement queryTclinux = driver.FindElement(By.CssSelector("table.tabdata:nth-child(2) tbody:nth-child(1) tr:nth-child(1) td.tabdata:nth-child(3) select:nth-child(1) > option:nth-child(2)"));
            queryTclinux.Click();
            IWebElement queryGetTclinux = driver.FindElement(By.XPath("//input[@id='xFile']"));
            queryGetTclinux.SendKeys(filepath);
            IWebElement queryUpgrade = driver.FindElement(By.XPath("/html[1]/body[1]/form[1]/div[1]/div[1]/div[2]/table[2]/tbody[1]/tr[1]/td[2]/input[1]"));
            queryUpgrade.Click();
        
        }
        private void Restart()
        {
            driver.Url = "http://192.168.1.1/cgi-bin/tools_system.asp";
            IWebElement queryRestart = driver.FindElement(By.XPath("//input[@value='Restore']"));
            queryRestart.Click();
            driver.SwitchTo().Alert().Accept();

        }

    }
}
