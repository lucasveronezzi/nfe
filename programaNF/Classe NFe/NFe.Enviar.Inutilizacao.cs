using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace programaNF
{
    partial class NFe
    {
        public int NFeInutilizar(string just, int nInicial, int nFinal, string cnpj, int cUF, int ambiente)
        {
            just = alteraCaracter(just);

            urlNF = new UrlNF(cUF, ambiente);

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeInutilizacao4";

            string xmlDoc = createXML_Inutilizacao(just, Convert.ToString(nInicial), Convert.ToString(nFinal), cnpj, Convert.ToString(cUF), Convert.ToString(ambiente));

            if (AssinarNFE(xmlDoc.ToString(), cnpj, "inutNFe") == 0)
                xmlDoc = getXmlAssinado();
            else
                return 999;

            string xmlSoap = getSoapXml(xmlDoc.ToString(), urlService, Convert.ToString(cUF), urlNF.ver_Inutilizacao);

            retXML = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/NfeInutilizacao2/nfeInutilizacaoNF2";

                retXML = RequestWebService(urlNF._Inutilizacao, xmlSoap, action, ambiente);

                if (retXML != String.Empty){
                    return 0;
                }   
                else
                    return 999;
            }
            else
                return 999;
        }

        private string createXML_Inutilizacao(string just, string nInicial, string nFinal, string cnpj, string cUF, string amb)
        {
            String nInicialID = nInicial;
            String nFinalID = nFinal;
            for (int x = nInicial.Length; x < 9; x++)
            {
                nInicialID = "0" + nInicialID;
                if (nFinal.Length != 9)
                    nFinalID = "0" + nFinalID;
            }
            String ano = DateTime.UtcNow.ToString("yy");
            String xml = String.Empty;
            MemoryStream stream = new MemoryStream();
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("inutNFe", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_Inutilizacao);

                writer.WriteStartElement("infInut", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("Id", "ID" + cUF + ano + cnpj + "55" + "001" + nInicialID + nFinalID);
                writer.WriteElementString("tpAmb", amb);
                writer.WriteElementString("xServ", "INUTILIZAR");
                writer.WriteElementString("cUF", cUF);
                writer.WriteElementString("ano", ano);
                writer.WriteElementString("CNPJ", cnpj);
                writer.WriteElementString("mod", "55");
                writer.WriteElementString("serie", "1");
                writer.WriteElementString("nNFIni", nInicial);
                writer.WriteElementString("nNFFin", nFinal);
                writer.WriteElementString("xJust", just);
                writer.WriteEndElement();
                
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
