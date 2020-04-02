﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Techunk_Api.Launcher;

namespace Techunk_Launcher
{
    public partial class Logout_and_Cache : Form
    {
        public Logout_and_Cache()
        {
            InitializeComponent();
        }

        MLogin login = new MLogin();
        MSession session;

        private void Logout_and_Cache_Load(object sender, EventArgs e)
        {
            session = login.GetLocalToken();
            lvAT.Text = session.AccessToken;
            lvUsername.Text = session.Username;
            lvUUID.Text = session.UUID;
            lvCT.Text = session.ClientToken;
        }

        private void Btn_Signout_Click(object sender, EventArgs e)
        {
            // signout
            if (session.Username == "")
            {
                MessageBox.Show("Actualmente no hay una sesión");
                return;
            }

            if (session.AccessToken == "Offline")
            {
                MessageBox.Show("La sesión actual es una sesión Offline");
                return;
            }

            var result = login.Signout(txtEmail.Text, txtPassword.Text);
            if (result)
            {
                login.DeleteTokenFile();
                MessageBox.Show("Desconexión completada");
                Application.Exit();
            }
            else
                MessageBox.Show("Error al desconectar");
        }

        private void Btn_InvalidateS_Click(object sender, EventArgs e)
        {
            // invalidate
            if(session.Username == "")
            {
                MessageBox.Show("Actualmente no hay una sesión");
                return;
            }

            if(session.AccessToken == "Offline")
            {
                MessageBox.Show("La sesión actual es una sesión Offline");
                return;
            }

            var result = login.Invalidate();

            if (result)
            {
                MessageBox.Show("Sesión invalidada");
                Application.Exit();
            }
            else
                MessageBox.Show("Error al invalidad la sesión");
        }

        private void Btn_DeleteS_Click(object sender, EventArgs e)
        {
            // delete
            if (session.Username == "")
            {
                MessageBox.Show("Actualmente no hay una sesión");
                return;
            }

            login.DeleteTokenFile();
            MessageBox.Show("Sesión borrada");
            Application.Exit();
        }
    }
}
