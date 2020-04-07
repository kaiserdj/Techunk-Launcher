using System;
using System.Threading;
using System.Windows.Forms;
using Techunk_Api.Launcher;
using System.Diagnostics;
using System.IO;
using Techunk_Api;

namespace Techunk_Launcher
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // Check java runtime
            log Llog = new log();
            Llog.main_log();
            log.log_.Warn("Prueba");

            var java = new Techunk_Api.Utils.MJava(Minecraft.DefaultPath + "\\runtime");
            if (!java.CheckJavaw())
            {
                var form = new Download_Form();
                form.Show();
                bool iscom = false;

                java.DownloadProgressChanged += (s, v) =>
                {
                    form.ChangeProgress(v.ProgressPercentage);
                };
                java.UnzipCompleted += (t, w) =>
                {
                    form.Close();
                    this.Show();
                    iscom = true;
                };

                java.DownloadJavaAsync();

                while (!iscom)
                {
                    Application.DoEvents();
                }
            }

            Txt_Java.Text = Minecraft.DefaultPath + "\\runtime\\bin\\javaw.exe";
        }

        bool allowOffline = true;
        MProfileInfo[] versions;
        MSession session;
        GameConsole GameConsole;

        private void Main_Shown(object sender, EventArgs e)
        {
            // Initialize launcher

            Txt_Path.Text = Minecraft.CurrentPath + "\\.minecraft";
            var th = new Thread(new ThreadStart(delegate
            {
                Minecraft.Initialize(Txt_Path.Text);

                versions = MProfileInfo.GetProfiles();
                Invoke((MethodInvoker)delegate
                {
                    foreach (var item in versions)
                    {
                        Cb_Version.Items.Add(item.Name);
                    }
                });

                // Try auto login

                var login = new MLogin();
                MSession result = login.TryAutoLogin();

                if (result.Result != MLoginResult.Success)
                    return;

                MessageBox.Show("¡Auto inicio de sesión completado!\n\nUsuario: " + result.Username);
                session = result;

                Invoke((MethodInvoker)delegate {
                    Btn_Login.Enabled = false;
                    Btn_Login.Text = "Auto inicio\nCompletado";
                    Txt_User.Text = session.Username;
                    Txt_Pass.Text = "#########";
                });
            }));
            th.Start();
        }

        private void Btn_Apply_Click(object sender, EventArgs e)
        {
            Minecraft.Initialize(Txt_Path.Text);
            versions = MProfileInfo.GetProfiles();
            Cb_Version.Items.Clear();
            foreach (var item in versions)
            {
                Cb_Version.Items.Add(item.Name);
            }
        }

        private void Btn_Login_Click(object sender, EventArgs e)
        {
            // Login
            if(Txt_User.Text == "")
            {
                MessageBox.Show("El campo usuario no puede estar vacio");
                return;
            }

            Btn_Login.Enabled = false;
            if (Txt_Pass.Text == "")
            {
                if (allowOffline)
                {
                    var login = new MLogin();
                    // session = MSession.GetOfflineSession(Txt_User.Text);
                    session = login.offline(Txt_User.Text);
                    MessageBox.Show("Inicio pirata completado : " + session.Username);
                }
                else
                {
                    MessageBox.Show("La contraseña no puede estar vacia");
                    Btn_Login.Enabled = true;
                    return;
                }
            }
            else
            {
                var th = new Thread(new ThreadStart(delegate
                {
                    var login = new MLogin();
                    var result = login.Authenticate(Txt_User.Text, Txt_Pass.Text);
                    if (result.Result == MLoginResult.Success)
                    {
                        MessageBox.Show("Inicio completado : " + result.Username);
                        session = result;
                    }
                    else
                    {
                        MessageBox.Show(result.Result.ToString() + "\n" + result.Message);
                        Invoke((MethodInvoker)delegate { Btn_Login.Enabled = true; });
                    }
                }));
                th.Start();
            }
        }

        private void Btn_Launch_Click(object sender, EventArgs e)
        {
            // Launch

            if (session == null)
            {
                MessageBox.Show("Inicie sesión primero");
                return;
            }


            if (Cb_Version.Text == "")
            {
                MessageBox.Show("Porfavor eliga una versión");
                return;
            }
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;

            string startVersion = Cb_Version.Text;
            string javaPath = Txt_Java.Text;
            string xmx = Txt_Ram.Text;
            string launcherName = Txt_LauncherName.Text;
            string serverIp = Txt_ServerIp.Text;

            var th = new Thread(new ThreadStart(delegate
            {
                var profile = MProfile.FindProfile(versions, startVersion); // Find Profile

                DownloadGame(profile); // Download game files

                MLaunchOption option = new MLaunchOption() // Set options
                {
                    StartProfile = profile,
                    JavaPath = javaPath,
                    LauncherName = launcherName,
                    MaximumRamMb = int.Parse(xmx),
                    ServerIp = serverIp,
                    Session = session,
                    CustomJavaParameter = Txt_JavaArgs.Text
                };

                if (Txt_ScWd.Text != "" && Txt_ScHt.Text != "")
                {
                    option.ScreenHeight = int.Parse(Txt_ScHt.Text);
                    option.ScreenWidth = int.Parse(Txt_ScWd.Text);
                }

                MLaunch launch = new MLaunch(option); // Start Process
                var process = launch.GetProcess();

                this.Invoke((MethodInvoker)delegate
                {
                    if(GameConsole != null)
                        GameConsole.Close();

                    GameConsole = new GameConsole();
                    GameConsole.Show();

                    groupBox1.Enabled = true;
                    groupBox2.Enabled = true;
                });

                DebugProcess(process);
            }));
            th.Start();
        }
        private void DownloadGame(MProfile profile, bool downloadResource = true)
        {
            MDownloader downloader = new MDownloader(profile);
            downloader.ChangeFile += Downloader_ChangeFile;
            downloader.ChangeProgress += Downloader_ChangeProgress;
            downloader.DownloadAll(downloadResource);
        }
        private void Downloader_ChangeProgress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                progressBar2.Value = e.ProgressPercentage;
            });

        }

        #region DEBUG PROCESS

        private void DebugProcess(Process process)
        {
            Console.WriteLine("Write game args");
            File.WriteAllText("launcher.txt", process.StartInfo.Arguments);

            Console.WriteLine("Set Debug Process");
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;

            Console.WriteLine("Start Debug Process");
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            output(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            output(e.Data);
        }

        void output(string msg)
        {
            Invoke(new Action(() =>
            {
                if (GameConsole != null)
                    GameConsole.AddLog(msg);
            }));
        }

        #endregion
        private void Downloader_ChangeFile(DownloadFileChangedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                Lv_Status.Text = e.FileKind.ToString() + " : " + e.FileName;
                progressBar1.Maximum = e.TotalFileCount;
                progressBar1.Value = e.ProgressedFileCount;
            });
        }

        private void Btn_loginOption_Click(object sender, EventArgs e)
        {
            var form3 = new Logout_and_Cache();
            form3.Show();
        }
    }
}
