using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;

namespace programaNF
{
    partial class NFe
    {
         public int NFeCancelar(string nProt, string correcao, string cnpj, string chave, string nSeq, string idLote, string date, int cUF, int ambiente)
         {
            correcao = alteraCaracter(correcao);

            urlNF = new UrlNF(cUF, ambiente);

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRecepcaoEvento4";

            string xmlDoc = createXML_Cancelamento(nProt, correcao, cnpj, chave, nSeq, idLote, date, ambiente, cUF);

            if (AssinarNFE(xmlDoc.ToString(), cnpj, "evento") == 0)
                xmlDoc = getXmlAssinado();
            else
                return 999;

            string xmlSoap = getSoapXml(xmlDoc, urlService, Convert.ToString(cUF), urlNF.ver_RecepcaoEvento);

            retXML = String.Empty;
            procEvento = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/RecepcaoEvento/nfeRecepcaoEvento";

                retXML = RequestWebService(urlNF._RecepcaoEvento, xmlSoap, action, ambiente);

                if (retXML != String.Empty){
                    createProcEvento(xmlDoc, retXML, urlNF.ver_RecepcaoEvento);
                    return 0;
                }
                else
                    return 999;
            }
            else
                return 999;
        }

         private string createXML_Cancelamento(string nProt, string descCarta, string cnpj, string chave, string nSeq, string idLote, string date, int amb, int cUF)
        {
           // String dTimeNow = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss-02:00");
            String dTimeNow = date;

            String xml = String.Empty;
            MemoryStream stream = new MemoryStream();
            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartElement("envEvento", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_RecepcaoEvento);
                writer.WriteElementString("idLote", idLote);

                writer.WriteStartElement("evento", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", urlNF.ver_RecepcaoEvento);

                writer.WriteStartElement("infEvento");
                writer.WriteAttributeString("Id", "ID110111" + chave + "0" + nSeq);
                writer.WriteElementString("cOrgao", Convert.ToString(cUF));
                writer.WriteElementString("tpAmb", Convert.ToString(amb));
                writer.WriteElementString("CNPJ", cnpj);
                writer.WriteElementString("chNFe", chave);
                writer.WriteElementString("dhEvento", dTimeNow);
                writer.WriteElementString("tpEvento", "110111");
                writer.WriteElementString("nSeqEvento", nSeq);
                writer.WriteElementString("verEvento", urlNF.ver_RecepcaoEvento);

                writer.WriteStartElement("detEvento");
                writer.WriteAttributeString("versao", urlNF.ver_RecepcaoEvento);
                writer.WriteElementString("descEvento", "Cancelamento");
                writer.WriteElementString("nProt", nProt);
                writer.WriteElementString("xJust", descCarta);
                writer.WriteEndElement();

                writer.WriteEndElement();

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
