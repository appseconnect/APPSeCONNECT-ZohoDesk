using InSync.eConnect.APPSeCONNECT.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InSync.eConnect.APPSeCONNECT.Utils;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Xml;
using System.Data.Entity.Design.PluralizationServices;

namespace InSync.eConnect.ZohoDesk
{
    public class XmlToJsonConverter : ICustomConverter
    {
        public XmlToJsonConverter(string source)
        {
            this.Source = source;
        }
        public string Source { get; set; }

        public ReturnMessage<string> Format()
        {
            var returnMessage = new ReturnMessage<string>();

            returnMessage.SetSuccess(this.ConvertXMLToJSON(this.Source));
            
            return returnMessage;
        }

        public string ConvertXMLToJSON(string xmldata)
        {
            string returnJson = string.Empty;
            try
            {
                XElement el = XElement.Parse(xmldata);
                var childs = el.Descendants();
                List<string> pluralnames = new List<string>();

                PluralizationService ps = PluralizationService.CreateService(System.Globalization.CultureInfo.GetCultureInfo("en-us"));
                string name = string.Empty;
                int count = 0;
                foreach (XElement node in childs)
                {
                    count = node.Descendants().Count();
                    if (count > 0)
                    {
                        name = node.Name.ToString();
                        if (name.Contains("_"))
                        {
                            var arr = name.Split('_');
                            if (ps.IsPlural(arr[1]))
                            {
                                pluralnames.Add(name);
                            }
                        }
                        else
                        {
                            if (ps.IsPlural(name))
                            {
                                pluralnames.Add(name);
                            }
                        }
                    }
                }

                //ToDo : Remove the use of both XDocument and XmlDocument
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmldata);
                string root = doc.DocumentElement.Name;

                XmlNodeList rows;
                XmlNode contentNode;
                foreach (string str in pluralnames)
                {
                    try
                    {
                        rows = doc.SelectNodes("//" + str);
                        if (rows.Count == 1)
                        {
                            contentNode = doc.SelectSingleNode("//" + root);
                            contentNode.InsertAfter(doc.CreateNode("element", str, ""), rows[0]);
                        }
                    }
                    catch
                    {

                    }
                }

                var jsonData = JsonConvert.SerializeXmlNode(doc).Replace(",null]", "]");

                var position = jsonData.IndexOf(":");
                var subString = jsonData.Substring(1, position + 1);
                jsonData = jsonData.Replace(subString, "");
                jsonData = jsonData.Remove(jsonData.Length - 1);

                returnJson = jsonData;
                return returnJson;
            }
            catch
            {
                return returnJson;
            }
        }
    }
}
