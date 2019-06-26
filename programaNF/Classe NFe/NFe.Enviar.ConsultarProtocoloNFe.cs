using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace programaNF
{
    partial class NFe
    {
        public int NFeConsultarProtocolo(string chaveNF, string cnpj, int cUF, int ambiente)
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

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeConsultaProtocolo4";

            string xmlDoc = createXML_ConsultaProtocolo(chaveNF, Convert.ToString(ambiente));

            string xmlSoap = getSoapXml(xmlDoc.ToString(), urlService, Convert.ToString(cUF), urlNF.ver_ConsultaNFe);

            retXML = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/NfeConsulta2/nfeConsultaNF2";

                retXML = RequestWebService(urlNF._ConsultaNFe, xmlSoap, action, ambiente);

                if (retXML != String.Empty)
                {
                    if (getRetStat() == "100" || getRetStat() == "101")
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

        private string createXML_ConsultaProtocolo(string chaveNF, string amb)
        {
            String xml = String.Empty;
            MemoryStream stream = new MemoryStream();
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("consSitNFe", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_ConsultaNFe);
                writer.WriteElementString("tpAmb", amb);
                writer.WriteElementString("xServ", "CONSULTAR");
                writer.WriteElementString("chNFe", chaveNF);

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
