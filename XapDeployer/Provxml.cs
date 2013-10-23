using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XapDeployer
{
    public class Provxml
    {
        public static string GetMultiInstallParameter(string fileName)
        {
            byte[] bytes = ApplicationApi.Functions.ReadFile(fileName);
            var encodings = new System.Text.Encoding[3] { Encoding.UTF8, Encoding.Unicode, Encoding.BigEndianUnicode };
            bool correctEncodingFound = false;
            XDocument reader = null;
            for (int i = 0; i < encodings.Length; ++i)
            {
                try
                {
                    Encoding enc = encodings[i];
                    string text = enc.GetString(bytes, 0, bytes.Length);

                    byte[] newBytes = Encoding.UTF8.GetBytes(text);

                    var memStream = new MemoryStream(newBytes);
                    reader = XDocument.Load(memStream);
                    memStream.Close();
                }
                catch (Exception ex)
                {
                    reader = null;
                    continue;
                }
                correctEncodingFound = true;
                break;
            }
            if (correctEncodingFound == false)
                return "";

            //var entries = from item in reader.Descendants("wap-provisioningdoc").Descendants("characteristic").Descendants("characteristic") select new {Value = item.Element("parm").Attribute("value").Value};
            var entries = from item in reader.Descendants("wap-provisioningdoc").Descendants("characteristic").Where(x => x.Attribute("type").Value == "AppInstall").Descendants("characteristic") select new { Value = item.Element("parm").Attribute("value").Value };
            string parameter = "";
            int currentEntry = 1;
            foreach (var entry in entries)
            {
                var parts = entry.Value.Split(';');

                string xapName = null,
                    licenseXml = null,
                    instanceId = null,
                    offerId = null;
                for (int i = 0; i < parts.Length; ++i)
                {
                    if (i == 0)
                        xapName = parts[i];
                    else if (i == 1)
                        licenseXml = parts[i];
                    else if (i == 2)
                        instanceId = parts[i];
                    else if (i == 3)
                        offerId = parts[i];
                }
                if (parameter != "")
                    parameter += "&";
                if (xapName != null)
                {
                    parameter += "file" + currentEntry.ToString() + "=" + xapName;
                    if (licenseXml != null)
                        parameter += "&" + "license" + currentEntry.ToString() + "=" + licenseXml;
                    if (instanceId != null)
                        parameter += "&" + "instance" + currentEntry.ToString() + "=" + instanceId;
                    if (offerId != null)
                        parameter += "&" + "offer" + currentEntry.ToString() + "=" + offerId;
                    currentEntry++;
                }
            }
            return parameter;
        }
    }
}
