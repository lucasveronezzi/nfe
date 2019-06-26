using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows.Forms;

namespace programaNF
{
    partial class NFe
    {
        public int NFeCartaCorrecao(string correcao, string cnpj, string chave, string nSeq, string idLote, string date, int cUF, int ambiente)
        {
            correcao = alteraCaracter(correcao);

            urlNF = new UrlNF(cUF, ambiente);

            string urlService = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRecepcaoEvento4";

            string xmlDoc = createXML_CartaDeCorrecao(correcao, cnpj, chave, nSeq, idLote, date, ambiente);

            if (AssinarNFE(xmlDoc.ToString(), cnpj, "evento") == 0)
                xmlDoc = getXmlAssinado();
            else
                return 999;

            string xmlSoap = getSoapXml(xmlDoc.ToString(), urlService, Convert.ToString(cUF), urlNF.ver_RecepcaoEvento);

            retXML = String.Empty;
            procEvento = String.Empty;

            if (xmlSoap != String.Empty)
            {
                string action = "http://www.portalfiscal.inf.br/nfe/wsdl/RecepcaoEvento/nfeRecepcaoEvento";

                retXML = RequestWebService(urlNF._RecepcaoEvento, xmlSoap, action, ambiente);

                if (retXML != String.Empty)
                {
                    createProcEvento(xmlDoc, retXML, urlNF.ver_RecepcaoEvento);
                    return 0;
                } 
                else
                    return 999;
            }
            else
                return 999;
        }

        private string createXML_CartaDeCorrecao(string descCarta, string cnpj, string chave, string nSeq, string idLote, string date, int amb)
        {
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
                writer.WriteAttributeString("Id", "ID110110" + chave + "0" + nSeq);
                writer.WriteElementString("cOrgao", urlNF.cOrgaoRecep);
                writer.WriteElementString("tpAmb", Convert.ToString(amb));
                writer.WriteElementString("CNPJ", cnpj);
                writer.WriteElementString("chNFe", chave);
                writer.WriteElementString("dhEvento", dTimeNow);
                writer.WriteElementString("tpEvento", "110110");
                writer.WriteElementString("nSeqEvento", nSeq);
                writer.WriteElementString("verEvento", urlNF.ver_RecepcaoEvento);

                writer.WriteStartElement("detEvento");
                writer.WriteAttributeString("versao", urlNF.ver_RecepcaoEvento);
                writer.WriteElementString("descEvento", "Carta de Correcao");
                writer.WriteElementString("xCorrecao", descCarta);
                writer.WriteElementString("xCondUso", "A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, de 15 de dezembro de 1970 e pode ser utilizada para regularizacao de erro ocorrido na emissao de documento fiscal, desde que o erro nao esteja relacionado com: I - as variaveis que determinam o valor do imposto tais como: base de calculo, aliquota, diferenca de preco, quantidade, valor da operacao ou da prestacao; II - a correcao de dados cadastrais que implique mudanca do remetente ou do destinatario; III - a data de emissao ou de saida.");
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
