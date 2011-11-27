using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace PassThru
{
    public class UpdateChecker
    {
        int versionA = 0;
        int versionB = 3;
        int versionC = 2;
        int versionD = 2;
        Thread updateThread;

        public void Updater()
        {
            updateThread = new Thread(new ThreadStart(UpdateLoop));
            updateThread.Start();
        }

        public void Close()
        {
            updateThread.Abort();
        }

        public void MyDownloadFile(Uri url, string outputFilePath)
        {
            const int BUFFER_SIZE = 16 * 1024;
            using (var outputFileStream = File.Create(outputFilePath, BUFFER_SIZE))
            {
                WebRequest req = WebRequest.Create(url);
                using (WebResponse response = req.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        do
                        {
                            bytesRead = responseStream.Read(buffer, 0, BUFFER_SIZE);
                            outputFileStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                }
            }
        }

        public void UpdateLoop()
        {
            WebClient wc = new WebClient();
            while (true)
            {
                Thread.Sleep(30000);
                string ret = CheckForNewVersion();
                if (ret != null)
                {
                    if (MessageBox.Show("Would you like to download the newest version of fireBwall?", "New Update Available!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {                        
                        string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        folder = folder + Path.DirectorySeparatorChar + "firebwall";
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);
                        folder = folder + Path.DirectorySeparatorChar + "updates";
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);
                        string file = ret.Substring(ret.IndexOf("/files/") + "/files/".Length);
                        MyDownloadFile(new Uri(ret), folder + Path.DirectorySeparatorChar + file);
                        MessageBox.Show("Update has finished downloading.  Please install the update from the firebwall folder in My Documents.");
                    }
                }
                Thread.Sleep(new TimeSpan(0, 15, 0));
            }
        }

        public string CheckForNewVersion()
        {
            WebClient wc = new WebClient();
            string xml = wc.DownloadString("https://code.google.com/feeds/p/firebwall/downloads/basic");
            foreach (string line in xml.Split("\n".ToCharArray()))
            {
                if (line.Contains("href=\"http://code.google.com/p/firebwall/downloads/detail?name=firebwall-"))
                {
                    string url = line.Substring(line.IndexOf("href=\"") + "href=\"".Length);
                    url = url.Substring(0, url.IndexOf("\""));
                    string version = url.Substring(url.IndexOf("-") + 1, url.IndexOf(".msi") - (url.IndexOf("-") + 1));
                    url = url.Replace("http://code.google.com/p/firebwall/downloads/detail?name=", "https://firebwall.googlecode.com/files/");
                    string a = version.Substring(0, version.IndexOf("."));
                    if (versionA == int.Parse(a))
                    {
                        version = version.Substring(version.IndexOf(".") + 1);
                        string b = version.Substring(0, version.IndexOf("."));
                        if (versionB == int.Parse(b))
                        {
                            version = version.Substring(version.IndexOf(".") + 1);
                            string c = version.Substring(0, version.IndexOf("."));
                            if (versionC == int.Parse(c))
                            {
                                version = version.Substring(version.IndexOf(".") + 1);
                                if (versionD == int.Parse(version))
                                {
                                    return null;
                                }
                                else if (versionD < int.Parse(version))
                                {
                                    return url;
                                }
                            }
                            else if (versionC < int.Parse(c))
                            {
                                return url;
                            }
                        }
                        else if (versionB < int.Parse(b))
                        {
                            return url;
                        }
                    }
                    else if (versionA < int.Parse(a))
                    {
                        return url;
                    }
                }
            }
            return null;
        }
    }
}
