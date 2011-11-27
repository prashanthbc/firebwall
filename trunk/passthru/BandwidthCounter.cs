using System;
using System.Collections.Generic;
using System.Text;

namespace PassThru
{
    public class BandwidthCounter
    {
        class MiniCounter
        {
            public uint bytes = 0;
            public uint kbytes = 0;
            public uint mbytes = 0;
            public uint gbytes = 0;
            public uint tbytes = 0;
            public uint pbytes = 0;
            DateTime lastRead = DateTime.Now;

            public void AddBytes(uint count)
            {
                bytes += count;
                while (bytes > 1024)
                {
                    kbytes++;
                    bytes -= 1024;
                }
                while (kbytes > 1024)
                {
                    mbytes++;
                    kbytes -= 1024;
                }
                while (mbytes > 1024)
                {
                    gbytes++;
                    mbytes -= 1024;
                }
                while (gbytes > 1024)
                {
                    tbytes++;
                    gbytes -= 1024;
                }
                while (tbytes > 1024)
                {
                    pbytes++;
                    tbytes -= 1024;
                }
            }

            public override string ToString()
            {
                if (pbytes > 0)
                {
                    double ret = (double)pbytes + ((double)((double)tbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " Pb";
                }
                else if (tbytes > 0)
                {
                    double ret = (double)tbytes + ((double)((double)gbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " Tb";
                }
                else if (gbytes > 0)
                {
                    double ret = (double)gbytes + ((double)((double)mbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " Gb";
                }
                else if (mbytes > 0)
                {
                    double ret = (double)mbytes + ((double)((double)kbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " Mb";
                }
                else if (kbytes > 0)
                {
                    double ret = (double)kbytes + ((double)((double)bytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " Kb";
                }
                else
                {
                    double ret = bytes;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " b";
                }
            }
        }

        public uint bytes = 0;
        public uint kbytes = 0;
        public uint mbytes = 0;
        public uint gbytes = 0;
        public uint tbytes = 0;
        public uint pbytes = 0;
        MiniCounter perSecond = new MiniCounter();

        public BandwidthCounter()
        {

        }

        public string GetPerSecond()
        {
            string s = perSecond.ToString() + "/s";
            perSecond = new MiniCounter();
            return s;
        }

        public void AddBytes(uint count)
        {
            count = 8 * count;
            perSecond.AddBytes(count);
            bytes += count;
            while (bytes > 1024)
            {
                kbytes++;
                bytes -= 1024;
            }
            while (kbytes > 1024)
            {
                mbytes++;
                kbytes -= 1024;
            }
            while (mbytes > 1024)
            {
                gbytes++;
                mbytes -= 1024;
            }
            while (gbytes > 1024)
            {
                tbytes++;
                gbytes -= 1024;
            }
            while (tbytes > 1024)
            {
                pbytes++;
                tbytes -= 1024;
            }
        }

        public override string ToString()
        {
            if (pbytes > 0)
            {
                double ret = (double)pbytes + ((double)((double)tbytes / 1024));
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " Pb";
            }
            else if (tbytes > 0)
            {
                double ret = (double)tbytes + ((double)((double)gbytes / 1024));
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " Tb";
            }
            else if (gbytes > 0)
            {
                double ret = (double)gbytes + ((double)((double)mbytes / 1024));
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " Gb";
            }
            else if (mbytes > 0)
            {
                double ret = (double)mbytes + ((double)((double)kbytes / 1024));
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " Mb";
            }
            else if (kbytes > 0)
            {
                double ret = (double)kbytes + ((double)((double)bytes / 1024));
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " Kb";
            }
            else
            {
                string s = bytes.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " b";
            }
        }
    }
}
