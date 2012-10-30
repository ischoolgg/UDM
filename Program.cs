using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace UDMLoader
{
    public class Program
    {
        private static List<string> GetAttributes(
            IEnumerable<XElement> Elements,
            string Name)
        {
            List<string> Result = new List<string>();

            foreach (XElement Element in Elements)
            {
                string Url = Element.AttributeText("Url");

                if (!string.IsNullOrWhiteSpace(Url))
                    if (!Result.Contains(Url))
                        Result.Add(Url);
            }

            return Result;
        }

        [FISCA.MainMethod()]
        public static void Main()
        {
            try
            {
                //<UDMInstall>
                //    <Common>
                //        <UDM Url="http://module.ischool.com.tw/module/148/StudentTransfer/udm.xml"/>
                //    </Common>
                //    <UDM Url="http://module.ischool.com.tw/module/148/Counsel/udm.xml">
                //        <ApplyTo>chhs.hcc.edu.tw</ApplyTo>
                //        <ApplyTo>n.chhs.hcc.edu.tw</ApplyTo>
                //        <ApplyTo>mdhs.tc.edu.tw</ApplyTo>
                //    </UDM>
                //</UDMInstall>

                XElement Element = XElement.Load("https://raw.github.com/ischoolgg/UDM/master/UDMInstall.xml");

                IEnumerable<XElement> elmComms = Element.XPathSelectElements("Common/UDM");
                IEnumerable<XElement> elmModules = Element.XPathSelectElements("UDM[ApplyTo='"+ FISCA.Authentication.DSAServices.AccessPoint +"']");
                
                List<string> CommonURLs = GetAttributes(elmComms,"Url");
                List<string> ModuleURLs = GetAttributes(elmModules, "Url");
                List<string> InstalledURLs = new List<string>();

                #region 取兩者間的聯合，沒有重複
                List<string> URLs = new List<string>();

                foreach (string CommonURL in CommonURLs)
                    if (!URLs.Contains(CommonURL))
                        URLs.Add(CommonURL);

                foreach (string ModuleURL in ModuleURLs)
                    if (!URLs.Contains(ModuleURL))
                        URLs.Add(ModuleURL);
                #endregion

                Task.Factory.StartNew
                (
                    () =>
                    {
                        foreach (string URL in URLs)
                        {
                            try
                            {
                                FISCA.ServerModule.AutoManaged(URL);
                                InstalledURLs.Add(URL);
                            }
                            catch (Exception ve)
                            {
                                MessageBox.Show(ve.Message);
                            }
                        }
                    }
                ).ContinueWith
                ((vTask) =>
                    {
                        if (FISCA.RTContext.IsDiagMode)
                            FISCA.Presentation.MotherForm.StartMenu["進階工具"]["UDM自動安裝列表"].Click += (xsender, xe) => FISCA.Presentation.Controls.MsgBox.Show(string.Join(System.Environment.NewLine, InstalledURLs));
                    }
                );
            }
            catch (Exception e)
            {
 
            }
        }
    }
}