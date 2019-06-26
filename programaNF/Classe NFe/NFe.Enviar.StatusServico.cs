using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace programaNF
{
    partial class NFe
    {
        public int NFeStatusServico(String cnpj, int cUF, int ambiente)
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

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4";

            string xmlDoc = createXML_StatusServicos(cUF, ambiente);

            string xmlSoap = getSoapXml(xmlDoc, urlService, Convert.ToString(cUF), urlNF.ver_StatusServicos);

            retXML = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/NfeStatusServico2/nfeStatusServicoNF2";

                retXML = RequestWebService(urlNF._StatusServicos, xmlSoap, action, ambiente);

                if (retXML != String.Empty)
                    return 0;
                else
                    return 999;
            }
            else
                return 999;
        }

        private string createXML_StatusServicos(int cUF, int ambiente)
        {
            String xml = String.Empty;
            MemoryStream stream = new MemoryStream(); 
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartElement("consStatServ", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_StatusServicos);

                writer.WriteElementString("tpAmb", ambiente.ToString());
                writer.WriteElementString("cUF", Convert.ToString(cUF));
                writer.WriteElementString("xServ", "STATUS");
                writer.WriteEndElement();
                writer.Flush();

                StreamReader reader = new StreamReader(stream, Encoding.UTF8, true);
                stream.Seek(0, SeekOrigin.Begin);

                xml += reader.ReadToEnd();
            }
            return xml;
        }
    }
}
