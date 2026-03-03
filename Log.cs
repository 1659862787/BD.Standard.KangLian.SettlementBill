using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BD.Standard.KangLian.SettlementBill
{
    public class Log
    {
        public static void log(string data )
        {
            string path = @"E:\//Log\";
            //debug==================================================  
            //StreamWriter dout = new StreamWriter(@"c:\" + System.DateTime.Now.ToString("yyyMMddHHmmss") + ".txt");  
            StreamWriter dout = new StreamWriter(path + System.DateTime.Now.ToString("yyyMMdd")+ ".txt", true);
            //dout.Write(readme + "\r\n");
            dout.Write("操作结果：" + "\r\n" + data + "\r\n操作时间：" + System.DateTime.Now.ToString("yyy-MM-dd HH:mm:ss")+"\r\n");
            //debug==================================================  
            dout.Close();
        }
    }
}
