﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using FM;

namespace PassThru
{
    public class fireBwallMetaData
    {
        public List<string> changelog = new List<string>();
        public string Description = "";
        public string filename = "";
        public string version = "";
        public string downloadUrl = "";
        public string imageUrl = "";
        public List<string> screenShotUrls = new List<string>();
    }

    public class UpdateChecker
    {
        static int versionA = 0;
        static int versionB = 3;
        static int versionC = 10;
        static int versionD = 0;
        Thread updateThread;

        public static fireBwallMetaData availableFirebwall = null;
        static object padlock = new object();

        [Serializable]
        public class UpdateConfig
        {
            public uint MinuteInterval = 15;
            public bool Enabled = true;
            public bool StartUpCheck = true;
        }

        public UpdateConfig Config
        {
            get;
            set;
        }

        internal sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;
                try
                {
                    string ToAssemblyName = assemblyName.Split(',')[0];
                    Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly ass in Assemblies)
                    {
                        if (ass.FullName.Split(',')[0] == ToAssemblyName)
                        {
                            typeToDeserialize = ass.GetType(typeName);
                            break;
                        }
                    }
                }
                catch (System.Exception exception)
                {
                    throw exception;
                }
                return typeToDeserialize;
            }
        }

        public void LoadConfig()
        {
            try
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                folder = folder + Path.DirectorySeparatorChar + "firebwall";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                string file = folder + Path.DirectorySeparatorChar + "updating.cfg";
                if (File.Exists(file))
                {
                    FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                    bFormatter.Binder = new VersionConfigToNamespaceAssemblyObjectBinder();
                    Config = (UpdateConfig)bFormatter.Deserialize(stream);
                    stream.Close();
                }
                else
                {
                    Config = new UpdateConfig();
                }
            }
            catch
            {
                Config = new UpdateConfig();
            }
        }

        public void SaveConfig()
        {
            try
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                folder = folder + Path.DirectorySeparatorChar + "firebwall";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                string file = folder + Path.DirectorySeparatorChar + "updating.cfg";
                FileStream stream = File.Open(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                bFormatter.Serialize(stream, Config);
                stream.Close();
            }
            catch { }
        }

        public void Updater()
        {
            LoadConfig();
            updateThread = new Thread(new ThreadStart(UpdateLoop));
            updateThread.Name = "Update Loop";
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

        public void UpdateFirebwallMetaVersion()
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers[HttpRequestHeader.UserAgent] = "firebwall 0.3.11.0 Updater";
                XmlTextReader reader = new XmlTextReader("https://www.firebwall.com/api/firebwall/" + LanguageConfig.GetCurrentTwoLetter() + ".xml");
                lock (padlock)
                {
                    availableFirebwall = new fireBwallMetaData();
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "current")
                                {
                                    availableFirebwall.version = reader.GetAttribute("version");
                                }
                                else if (reader.Name == "filename")
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                        availableFirebwall.filename = reader.Value;
                                }
                                else if (reader.Name == "description")
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                        availableFirebwall.Description = reader.Value;
                                }
                                else if (reader.Name == "entry")
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                        availableFirebwall.changelog.Add(reader.Value);
                                }
                                else if (reader.Name == "downloadurl")
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                        availableFirebwall.downloadUrl = reader.Value;
                                }
                                else if (reader.Name == "imageurl")
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                        availableFirebwall.imageUrl = reader.Value;
                                }
                                else if (reader.Name == "url")
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                    {
                                        availableFirebwall.screenShotUrls.Add(reader.Value);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch { }
        }

        public static bool IsVersionNew()
        {
            string version = availableFirebwall.version;
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
                            return false;
                        }
                        else if (versionD < int.Parse(version))
                        {
                            return true;
                        }
                    }
                    else if (versionC < int.Parse(c))
                    {
                        return true;
                    }
                }
                else if (versionB < int.Parse(b))
                {
                    return true;
                }
            }
            else if (versionA < int.Parse(a))
            {
                return true;
            }
            return false;
        }

        public void UpdateLoop()
        {
            bool firstTime = true;
            while (true)
            {
                Thread.Sleep(30000);
                if ((firstTime && Config.StartUpCheck) || Config.Enabled)
                {
                    UpdateFirebwallMetaVersion();
                    if (availableFirebwall != null && IsVersionNew())
                    {
                        PassThru.Tabs.DownloadCenter.Instance.ShowFirebwallUpdate();
                        return;
                    }
                }
                firstTime = false;
                Thread.Sleep(new TimeSpan(0, (int)Config.MinuteInterval, 0));
            }
        }
    }
}
