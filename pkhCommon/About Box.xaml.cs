using System;
using System.Windows;
using System.ComponentModel;
using System.Reflection;
using System.Net;
using System.IO;
using System.Xml;

namespace pkhCommon.Windows
{
    /// <summary>
    /// Non-localized about box. Localized apps should implement their own.
    /// </summary>
    public partial class About_Box : Window
    {
        private string AppVersion = null;
        private bool IsReVVed = false;
        private Assembly TheAssembly = null;

        public About_Box()
        {
            InitializeComponent();

            TheAssembly = Assembly.GetExecutingAssembly();
            this.Title = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}.{1}.{2}.{3}", MajorVersion, MinorVersion, Build, Revision);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            AppVersion = Build;
            this.textBoxDescription.Text = "There will be no further updates for this product.";
        }

        /// <summary>
        /// For non-localized apps only
        /// </summary>
        /// <param name="a"></param>
        public About_Box(Assembly a)
        {
            InitializeComponent();

            TheAssembly = a;
            this.Title = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}.{1}.{2}.{3}", MajorVersion, MinorVersion, Build, Revision);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            AppVersion = Build;
            this.textBoxDescription.Text = "There will be no further updates for this product.";
            if (AssemblyTitle == "ReVVed")
            {
                AppVersion = TheAssembly.GetName().Version.Minor.ToString();
                IsReVVed = true;
            }
        }

        /// <summary>
        /// Used for multilicense apps only.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="companyName"></param>
        //public About_Box(Assembly a, string companyName, string hardwareId)
        //{
        //    InitializeComponent();

        //    TheAssembly = a;
        //    this.Title = String.Format("About {0}", AssemblyTitle + " ML");
        //    this.labelProductName.Text = AssemblyProduct + " ML";
        //    this.labelVersion.Text = String.Format("Version {0}.{1}.{2}.{3}", MajorVersion, MinorVersion, Build, Revision);
        //    this.labelCopyright.Text = AssemblyCopyright;
        //    this.labelCompanyName.Text = AssemblyCompany;
        //    AppVersion = Build;
        //if(companyName != null)
        //    this.textBoxDescription.Text = "This software is licensed to " + companyName + "\n";
        //else
        //    this.textBoxDescription.Text = "This software is not licensed.\n";
        //this.textBoxDescription.AppendText(string.Format("Computer: {0} ({1})", Environment.MachineName, hardwareId));
        //    this.textBoxDescription.AppendText("There will be no further updates for this product.");
        //}

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(TheAssembly.CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return TheAssembly.GetName().Version.ToString();
            }
        }

        public string MajorVersion
        {
            get
            {
                return TheAssembly.GetName().Version.Major.ToString();
            }
        }

        public string MinorVersion
        {
            get
            {
                return TheAssembly.GetName().Version.Minor.ToString();
            }
        }

        public string Build
        {
            get
            {
                return TheAssembly.GetName().Version.Build.ToString();
            }
        }

        public string Revision
        {
            get
            {
                return TheAssembly.GetName().Version.Revision.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            TestButton.Visibility = Visibility.Visible;
            this.Title += " - Debug Mode";
#endif
            //BackgroundWorker worker = new BackgroundWorker();
            //worker.DoWork += worker_DoWork;
            //worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            //worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.pkhlineworks.ca/softwareversions.xml");
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                // Gets the stream associated with the response.
                Stream receiveStream = myHttpWebResponse.GetResponseStream();

                //Major-Revit version   Minor-App version   MajorRevision-App release version    MinorRevision-not used
                //AssemblyTitle must be as it appears in softwareversions.xml
                XmlReader Xread = XmlReader.Create(receiveStream);
                string appSymbol = null;
                if (IsReVVed)
                    appSymbol = AssemblyTitle + MajorVersion;
                else
                    appSymbol = (AssemblyTitle + MajorVersion + "_V" + MinorVersion).Replace(" ","_");
                if (Xread.ReadToFollowing(appSymbol))
                    System.Diagnostics.Debug.WriteLine(string.Format("Found symbol: {0}", appSymbol));
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Did not find symbol: {0}", appSymbol));
                    e.Result = "Could not find this software listed in updates.";
                    myHttpWebResponse.Close();
                    receiveStream.Close();
                    return;
                }

                int ver = Xread.ReadElementContentAsInt();
                int thisVer = Convert.ToInt32(AppVersion);

                // Releases the resources of the response.
                myHttpWebResponse.Close();
                // Releases the resources of the Stream.
                receiveStream.Close();

                if (ver > thisVer)
                    e.Result = ("A newer version of " + AssemblyTitle + " is available." + " Version " + ver.ToString() + " is available at www.pkhlineworks.ca.");
                else
                    e.Result = "Your product is up to date.";
            }
            catch (Exception)
            {
                e.Result = "Could not contact server to check for updates.";
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.textBoxDescription.AppendText("\n");
            this.textBoxDescription.AppendText(e.Result.ToString());
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Exception err = new Exception("Test exception created.");
            err.Data.Add("test data", "my test data in exception");
            throw err;
        }
    }
}
