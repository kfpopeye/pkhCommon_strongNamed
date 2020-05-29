using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pkhCommon.Windows
{
    public partial class Browser : Form
    {
        public Browser()
        {
            InitializeComponent();

            this.webBrowser1.CanGoBackChanged += new EventHandler(webBrowser1_CanGoBackChanged);
            this.webBrowser1.CanGoForwardChanged += new EventHandler(webBrowser1_CanGoForwardChanged);
        }



        public void ShowDialog(string link)
        {
            this.BackButton.Visible = false;
            this.ForwardButton.Visible = false;
            this.BottomStatusLabel.Visible = false;
            this.toolStripProgressBar1.Visible = false;
            this.Text = "License Management";

            string address = link.Replace("\\", "/").Replace(" ", "%20");

            if (!address.StartsWith("file:///", true, null))
            {
                address = "file:///" + address;
            }
            try
            {
                webBrowser1.Navigate(new Uri(address));
                this.Show();
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        public void ShowHelp(string link)
        {
            string address = link.Replace("\\", "/").Replace(" ", "%20");

            if (!address.StartsWith("file:///", true, null))
            {
                address = "file:///" + address;
            }
            try
            {
                webBrowser1.Navigate(new Uri(address));
                this.Show();
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        public void Show(string address)
        {
            if (!address.StartsWith("http://", true, null) && !address.StartsWith("https://", true, null))
            {
                address = "http://" + address;
            }
            try
            {
                webBrowser1.Navigate(new Uri(address));
                this.Show();
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        #region EventHandlers
        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        private void webBrowser1_CanGoBackChanged(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
                BackButton.Enabled = true;
            else
                BackButton.Enabled = false;
        }

        private void webBrowser1_CanGoForwardChanged(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward)
                ForwardButton.Enabled = true;
            else
                ForwardButton.Enabled = false;
        }

        private void Browser_SizeChanged(object sender, EventArgs e)
        {
            this.webBrowser1.Size = new Size(this.Width - 6, this.Height - 63);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            toolStripProgressBar1.MarqueeAnimationSpeed = 0;
            toolStripProgressBar1.Value = 0;
            BottomStatusLabel.Text = webBrowser1.Url.ToString();
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            toolStripProgressBar1.MarqueeAnimationSpeed = 100;
        }
        #endregion
    }
}
