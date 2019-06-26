using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace programaNF
{
    partial class NFe
    {
        public int NFAutorizacaoLote(string xmlDoc, string cnpj, int cUF, int ambiente)
        {
            #region"Seleção de Certificado Digital"
            if (xCert == null)
                xCert = selectCert(cnpj);
            if (xCert == null)
            {
                errorBroken = "Nenhum Certificado Digital selecionado.";
                return 999;
            }
            #endregion

            urlNF = new UrlNF(cUF, ambiente);

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4";

            xmlDoc = createXML_LoteNFe(xmlDoc, cnpj, cUF);

            string xmlSoap = getSoapXml(xmlDoc.ToString(), urlService, Convert.ToString(cUF), urlNF.ver_Autorizacaos);

            #region "Gera o arquivo XML para ver os dados"
            string path = pathApp + "\\EnvioNFe-autorizacao.xml";
            TextWriter tw = new StreamWriter(path);
            tw.WriteLine(xmlSoap.ToString());
            tw.Close();
            #endregion

            retXML = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/NfeAutorizacao/nfeAutorizacaoLote";

                retXML = RequestWebService(urlNF._Autorizacaos, xmlSoap, action, ambiente);

                if (retXML != String.Empty){
                    return 0;
                }
                else
                    return 999;
            }
            else
                return 999;
        }

        private string createXML_LoteNFe(string xmlDoc, string cnpj, int cUF)
        {
            String xml = String.Empty;
            MemoryStream stream = new MemoryStream();

            XmlDocument document = new XmlDocument();
            xmlDoc = xmlDoc.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
            xmlDoc = xmlDoc.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
            document.LoadXml(xmlDoc.ToString());
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("enviNFe", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_Autorizacaos);
                writer.WriteElementString("idLote", "5");
                writer.WriteElementString("indSinc", "0");

                document.WriteContentTo(writer);

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
