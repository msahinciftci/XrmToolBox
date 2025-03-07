﻿using McTools.Xrm.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XrmToolBox.AppCode;
using XrmToolBox.Extensibility;

namespace XrmToolBox.Forms
{
    public partial class OptionsDialog : Form
    {
        public OptionsDialog(Options option)
        {
            InitializeComponent();

            Option = (Options)option.Clone();

            lblChangePathDescription.Text = string.Format(lblChangePathDescription.Text, Paths.XrmToolBoxPath);
            propertyGrid1.SelectedObject = Option;
            chkOptinAI.Checked = Option.OptinForApplicationInsights;

            PopulateAssemblies();

            CheckAppProtocolStatus();
        }

        public Options Option { get; private set; }

        private static string assemblyPrioritizer(string assemblyName)
        {
            return
                assemblyName.Equals("XrmToolBox") ? "AAAAAAAAAAAA" :
                assemblyName.Contains("XrmToolBox") ? "AAAAAAAAAAAB" :
                assemblyName.Equals(Assembly.GetExecutingAssembly().GetName().Name) ? "AAAAAAAAAAAC" :
                assemblyName;
        }

        private void btnAppProtocol_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(RegistryHelper.XtbProtocolPath()))
                {
                    RegistryHelper.RemoveXtbProtocol();
                }
                else
                {
                    RegistryHelper.AddXtbProtocol(Application.ExecutablePath, Paths.XrmToolBoxPath == "." ? new FileInfo(Application.ExecutablePath).DirectoryName : Paths.XrmToolBoxPath);
                }

                CheckAppProtocolStatus();
            }
            catch (Exception error)
            {
                MessageBox.Show(this, $"An error occured when setting application protocol: {error.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            Option = (Options)propertyGrid1.SelectedObject;
            Option.OptinForApplicationInsights = chkOptinAI.Checked;

            Option.Save();

            if (cbbProxyUsage.SelectedIndex == 2)
            {
                ConnectionManager.Instance.ConnectionsList.UseCustomProxy = true;
                ConnectionManager.Instance.ConnectionsList.UseInternetExplorerProxy = false;
                ConnectionManager.Instance.ConnectionsList.ProxyAddress = txtProxyAddress.Text;
                ConnectionManager.Instance.ConnectionsList.UserName = txtProxyUser.Text;
                ConnectionManager.Instance.ConnectionsList.Password = txtProxyPassword.Text;
                ConnectionManager.Instance.ConnectionsList.ByPassProxyOnLocal = chkByPassProxyOnLocal.Checked;
                ConnectionManager.Instance.ConnectionsList.UseDefaultCredentials = !rbCustomAuthYes.Checked;
            }
            else
            {
                ConnectionManager.Instance.ConnectionsList.UseInternetExplorerProxy = cbbProxyUsage.SelectedIndex == 1;
                ConnectionManager.Instance.ConnectionsList.UseCustomProxy = false;
                ConnectionManager.Instance.ConnectionsList.ProxyAddress = null;
                ConnectionManager.Instance.ConnectionsList.UserName = null;
                ConnectionManager.Instance.ConnectionsList.Password = null;
                ConnectionManager.Instance.ConnectionsList.ByPassProxyOnLocal = false;
                ConnectionManager.Instance.ConnectionsList.UseDefaultCredentials = false;
            }

            try
            {
                WebProxyHelper.ApplyProxy();

                ConnectionManager.Instance.SaveConnectionsFile();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(this, @"An error occured: " + error.Message, @"Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void cbbProxyUsage_SelectedIndexChanged(object sender, EventArgs e)
        {
            var useCustomProxy = cbbProxyUsage.SelectedIndex == 2;

            txtProxyAddress.Enabled = useCustomProxy;
            txtProxyPassword.Enabled = useCustomProxy && rbCustomAuthYes.Checked;
            txtProxyUser.Enabled = useCustomProxy && rbCustomAuthYes.Checked;
            chkByPassProxyOnLocal.Enabled = useCustomProxy;
            rbCustomAuthYes.Enabled = useCustomProxy;
            rbCustomAuthNo.Enabled = useCustomProxy;
        }

        private void CheckAppProtocolStatus()
        {
            var protocolPath = RegistryHelper.XtbProtocolPath();
            var isEnabled = !string.IsNullOrEmpty(protocolPath);

            lblAppProtocolStatus.Text = string.Format(lblAppProtocolStatus.Tag.ToString(), isEnabled ? "Enabled" : "Disabled");
            lblAppProtocolStatus.ForeColor = isEnabled ? Color.Green : Color.Red;

            btnAppProtocol.Text = isEnabled ? "Disable" : "Enable";

            lblAppProtocolPath.Text = protocolPath;
            lblAppProtocolPath.Visible = isEnabled;
        }

        private ListViewItem GetListItem(AssemblyName a)
        {
            var assembly = Assembly.Load(a);
            var fi = FileVersionInfo.GetVersionInfo(assembly.Location);

            var item = new ListViewItem(a.Name);
            item.SubItems.Add(fi.FileVersion.ToString());
            return item;
        }

        private List<AssemblyName> GetReferencedAssemblies()
        {
            var names = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                    .Where(a => !a.Name.Equals("mscorlib") && !a.Name.StartsWith("System") && !a.Name.Contains("CSharp")).ToList();
            names.Add(Assembly.GetExecutingAssembly().GetName());
            names = names.OrderBy(a => assemblyPrioritizer(a.Name)).ToList();
            return names;
        }

        private void llOpenRootFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName);
        }

        private void llOpenStorageFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Paths.XrmToolBoxPath);
        }

        private void llProtocolDoc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.xrmtoolbox.com/documentation/for-developers/implement-application-protocol/");
        }

        private void PopulateAssemblies()
        {
            var assemblies = GetReferencedAssemblies();
            var items = assemblies.Select(a => GetListItem(a)).ToArray();
            lvAssemblies.Items.Clear();
            lvAssemblies.Items.AddRange(items);
        }

        private void rbCustomAuthYes_CheckedChanged(object sender, EventArgs e)
        {
            txtProxyPassword.Enabled = rbCustomAuthYes.Checked;
            txtProxyUser.Enabled = rbCustomAuthYes.Checked;
        }
    }
}