using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace programaNF
{
    partial class NFe
    {
        public int NFeConsultarCadastro(String cnpjD, string ufD, int cUF, int ambiente)
        {
            #region"Seleção de Certificado Digital"
            if (xCert == null)
                xCert = selectCert(cnpjD);
            if (xCert == null)
            {
                errorBroken = "Nenhum Certificado Digital selecionado";
                return 999;
            }
            #endregion

            urlNF = new UrlNF(cUF, ambiente);

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/CadConsultaCadastro4";

            string xmlDoc = createXML_ConsultarCadastro(cnpjD, ufD);

            string xmlSoap = getSoapXml(xmlDoc, urlService, Convert.ToString(cUF), urlNF.ver_ConsultaCadastro);

            retXML = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/CadConsultaCadastro2/consultaCadastro2";

                retXML = RequestWebService(urlNF._ConsultaCadastro, xmlSoap, action, ambiente);

                if (retXML != String.Empty)
                    return 0;
                else
                    return 999;
            }
            else
                return 999;
        }

        private string createXML_ConsultarCadastro(string cnpj, string uf)
        {
            String xml = String.Empty;
            MemoryStream stream = new MemoryStream();
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartElement("ConsCad", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_ConsultaCadastro);

                writer.WriteStartElement("infCons");
                writer.WriteElementString("xServ", "CONS-CAD");
                writer.WriteElementString("UF", uf);

                writer.WriteElementString("CNPJ", cnpj);

                writer.WriteEndElement();
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
