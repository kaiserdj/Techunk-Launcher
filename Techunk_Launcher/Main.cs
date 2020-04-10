using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Techunk_Api;
using Techunk_Api.Core;
using System.ComponentModel;

namespace Techunk_Launcher
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        Techunk Launcher;
        MSession Session;
        GameConsole GameConsole;
        bool allowOffline = true;

        private void Main_Shown(object sender, EventArgs e)
        {
            // Initialize launcher
            Txt_Path.Text = Minecraft.GetOSDefaultPath();

            var th = new Thread(new ThreadStart(delegate
            {
                InitializeLauncher();

                // Try auto login

                var login = new MLogin();
                MSession result = login.TryAutoLogin();

                if (result.Result != MLoginResult.Success)
                    return;

                MessageBox.Show("¡Auto inicio de sesión completado!\n\nUsuario: " + result.Username);
                Session = result;

                Invoke((MethodInvoker)delegate
                {
                    Btn_Login.Enabled = false;
                    Btn_Login.Text = "Auto inicio\nCompletado";
                    Txt_User.Text = Session.Username;
                    Txt_Pass.Text = "#########";
                });
            }));
            th.Start();
        }

        private void Btn_Apply_Click(object sender, EventArgs e)
        {
            InitializeLauncher();
        }

        private void InitializeLauncher()
        {
            Launcher = new Techunk(Txt_Path.Text);
            Launcher.FileChanged += Launcher_FileChanged;
            Launcher.ProgressChanged += Launcher_ProgressChanged;
            var versions = Launcher.UpdateProfileInfos();

            Invoke(new Action(() =>
            {
                Cb_Version.Items.Clear();
                foreach (var item in versions)
                {
                    Cb_Version.Items.Add(item.Name);
                }
            }));
        }

        private void Btn_Login_Click(object sender, EventArgs e)
        {
            // Login
            if (Txt_User.Text == "")
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
                    Session = login.offline(Txt_User.Text);
                    MessageBox.Show("Inicio pirata completado : " + Session.Username);
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
                        Session = result;
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

            if (Session == null)
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

            try
            {
                var versionname = Cb_Version.Text;
                var launchOption = new MLaunchOption()
                {
                    JavaPath = Txt_Java.Text,
                    MaximumRamMb = int.Parse(Txt_Ram.Text),
                    Session = this.Session,

                    VersionType = Txt_VersionType.Text,
                    GameLauncherName = Txt_GLauncherName.Text,
                    GameLauncherVersion = Txt_GLauncherVersion.Text,

                    ServerIp = Txt_ServerIp.Text,

                    DockName = Txt_DockName.Text,
                    DockIcon = Txt_DockIcon.Text
                };

                if (!string.IsNullOrEmpty(Txt_ServerPort.Text))
                    launchOption.ServerPort = int.Parse(Txt_ServerPort.Text);

                if (!string.IsNullOrEmpty(Txt_ScWd.Text) && !string.IsNullOrEmpty(Txt_ScHt.Text))
                {
                    launchOption.ScreenHeight = int.Parse(Txt_ScHt.Text);
                    launchOption.ScreenWidth = int.Parse(Txt_ScWd.Text);
                }

                if (!string.IsNullOrEmpty(Txt_JavaArgs.Text))
                    launchOption.JVMArguments = Txt_JavaArgs.Text.Split(' ');

                var th = new Thread(() =>
                {
                    var process = Launcher.CreateProcess(versionname, launchOption);
                    StartProcess(process);
                });
                th.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                var fWork = new Action(() =>
                {
                    if (GameConsole != null)
                        GameConsole.Close();

                    GameConsole = new GameConsole();
                    GameConsole.Show();

                    groupBox1.Enabled = true;
                    groupBox2.Enabled = true;
                });

                if (this.InvokeRequired)
                    this.Invoke(fWork);
                else
                    fWork.Invoke();
            }
        }

        private void Launcher_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                progressBar2.Value = e.ProgressPercentage;
            }));
        }

        private void Launcher_FileChanged(DownloadFileChangedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                progressBar1.Maximum = e.TotalFileCount;
                progressBar1.Value = e.ProgressedFileCount;
                Lv_Status.Text = e.FileKind.ToString() + " : " + e.FileName;
            }));
        }

        private void StartProcess(Process process)
        {
            File.WriteAllText("launcher.txt", process.StartInfo.Arguments);

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;

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
                if (GameConsole != null && !GameConsole.IsDisposed)
                    GameConsole.AddLog(msg);
            }));
        }

        private void Btn_loginOption_Click(object sender, EventArgs e)
        {
            var form3 = new Logout_and_Cache();
            form3.Show();
        }
    }
}