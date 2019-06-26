using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace programaNF
{
    partial class NFe
    {
        public int NFeRetAutorizacao(string nRecibo, string cnpj, int cUF, int ambiente)
        {
            #region"Seleção de Certificado Digital"
            if (xCert == null)
                xCert = selectCert(cnpj);
            if (xCert == null)
            {
                errorBroken = "Nenhum Certificado Digital selecionado";
                return 999;
            }
            #endregion

            urlNF = new UrlNF(cUF, ambiente);

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRetAutorizacao4";

            string xmlDoc = createXML_RetAutorizacao(nRecibo, Convert.ToString(ambiente));

            string xmlSoap = getSoapXml(xmlDoc.ToString(), urlService, Convert.ToString(cUF), urlNF.ver_RetAutorizacao);

            retXML = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/NfeRetAutorizacao/nfeRetAutorizacaoLote";

                Thread.Sleep(10000);

                retXML = RequestWebService(urlNF._RetAutorizacao, xmlSoap, action, ambiente);

                if (retXML != String.Empty)
                {
                    if (getRetStat() == "104")
                    {
                        int indexIni = retXML.IndexOf("<protNFe");
                        int comp = retXML.IndexOf("</protNFe>") + 10 - indexIni;
                        protNFe = retXML.Substring(indexIni, comp);
                    }

                    return 0;
                }    
                else
                    return 999;
            }
            else
                return 999;
        }

        public string gerarXMLProc(string xml, string xmlProtocolo)
        {
            XmlDocument documentNFe = new XmlDocument();
            XmlDocument documentProc = new XmlDocument();
            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
            xmlProtocolo = xmlProtocolo.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
            xmlProtocolo = xmlProtocolo.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
            documentNFe.LoadXml(xml.ToString());
            documentProc.LoadXml(xmlProtocolo.ToString());

            String retXMLProc = String.Empty;
            MemoryStream stream = new MemoryStream();
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("nfeProc", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_RetAutorizacao);

                documentNFe.WriteContentTo(writer);

                writer.WriteStartElement("protNFe");
                writer.WriteAttributeString("versao", urlNF.ver_RetAutorizacao);
                XmlNodeList listNode = documentProc.GetElementsByTagName("protNFe");
                listNode.Item(0).WriteContentTo(writer);
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteEndDocument();
                writer.Flush();

                StreamReader reader = new StreamReader(stream, Encoding.UTF8, true);
                stream.Seek(0, SeekOrigin.Begin);

                retXMLProc += reader.ReadToEnd();
            }
            return retXMLProc;
        }

        public void SalvarXMLProc(string xml, string xmlProtocolo, string caminho, string nome)
        {
            string path2 = caminho + "\\" + nome + ".xml";
            TextWriter tw1 = new StreamWriter(path2);
            tw1.WriteLine(gerarXMLProc(xml, xmlProtocolo));
            tw1.Close();
        }

        private string createXML_RetAutorizacao(string nRecibo, string amb)
        {
            String xml = String.Empty;
            MemoryStream stream = new MemoryStream();
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("consReciNFe", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_RetAutorizacao);
                
                writer.WriteElementString("tpAmb", amb);
                writer.WriteElementString("nRec", nRecibo);
                
                writer.WriteEndElement();

                writer.WriteEndDocument();
                writer.Flush();

                StreamReader reader = new StreamReader(stream, Encoding.UTF8, true);
                stream.Seek(0, SeekOrigin.Begin);

                xml += reader.ReadToEnd();
            }
            return xml;
        }
    }
}
