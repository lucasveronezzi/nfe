using System;
using System.Net;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace programaNF
{
    public partial class NFe
    {
        private UrlNF urlNF;
        private int indEvent;
        private string RequestWebService(string wsURL, string param, string action, int ambiente)
        {

            indEvent = 0;
            try
            {
                protNFe = String.Empty;
                if (wsURL == "")
                {
                    errorBroken = "Não foi possivel localizar o endereço do WebService para o Estado e ambiente solicitado";
                    return String.Empty;
                }

                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(CustomValidation);
                //System.Net.ServicePointManager.CertificatePolicy = new MyPolicy();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                cookies = new CookieContainer();
                Uri urlpost = new Uri(wsURL);
                HttpWebRequest httpPostConsultaNFe = (HttpWebRequest)HttpWebRequest.Create(urlpost);

                string postConsultaComParametros = param;
                byte[] buffer2 = Encoding.ASCII.GetBytes(postConsultaComParametros);

                httpPostConsultaNFe.CookieContainer = cookies;
                httpPostConsultaNFe.Timeout = 300000;
                httpPostConsultaNFe.ContentType = "application/soap+xml; charset=utf-8";
                httpPostConsultaNFe.Method = "POST";
                httpPostConsultaNFe.ClientCertificates.Add(xCert);
                httpPostConsultaNFe.ContentLength = buffer2.Length;

                Stream PostData = httpPostConsultaNFe.GetRequestStream();
                PostData.Write(buffer2, 0, buffer2.Length);
                PostData.Close();
                
                HttpWebResponse responsePost = (HttpWebResponse)httpPostConsultaNFe.GetResponse();
                Stream istreamPost = responsePost.GetResponseStream();
                StreamReader strRespotaUrlConsultaNFe = new StreamReader(istreamPost, System.Text.Encoding.UTF8);

                string retornoXML = strRespotaUrlConsultaNFe.ReadToEnd();
                

                #region "Gera o arquivo retorno XML para ver os dados"
                string path2 = pathApp + "\\NF-e_retorno_soap.xml";
                TextWriter tw1 = new StreamWriter(path2);
                tw1.WriteLine(retornoXML);
                tw1.Close();
                #endregion
  
                return retornoXML;
            }
            catch (Exception ex)
            {
                errorBroken = ex.Message;
                
                errorBrokenDetalhado = ex.StackTrace;
                #region "Gera o arquivo de error"
                string path2 = pathApp + "\\error_envio.txt";
                TextWriter tw1 = new StreamWriter(path2);
                tw1.WriteLine(ex.StackTrace);
                tw1.Close();
                #endregion
                System.Windows.Forms.MessageBox.Show("Erro no envio: " + errorBroken);
                return String.Empty;
                //eventos.AppLog("NFeStatusServico", ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error);
            }
        }

        public class MyPolicy : ICertificatePolicy
        {
            public bool CheckValidationResult(
                  ServicePoint srvPoint
                , X509Certificate certificate
                , WebRequest request
                , int certificateProblem)
            {
                //Return True to force the certificate to be accepted.
                return true;

            } // end CheckValidationResult
        } // class MyPolicy


        private static bool CustomValidation(object sender,
            X509Certificate cert,
            X509Chain chain, System.Net.Security.SslPolicyErrors error)
                    {
                        return true;
        }


/*****************************************************************************************************************************************************/
/**************************************** Get das informações da Nota Fiscal *********************************************************************/
        public String getRetProtNFe()
        {
            return protNFe;
        }
        public String getRetNFStatus()
        {
            int indexIni = protNFe.IndexOf("<cStat>") + 7;
            int comp = protNFe.IndexOf("</cStat>") - indexIni;
            return protNFe.Substring(indexIni, comp);
        }
        public String getRetNFMotivo()
        {
            int indexIni = protNFe.IndexOf("<xMotivo>") + 9;
            int comp = protNFe.IndexOf("</xMotivo>") - indexIni;
            return protNFe.Substring(indexIni, comp);
        }
        public String getRetNFDhRecibo()
        {
            int indexIni = retXML.IndexOf("<dhRecbto>") + 10;
            int comp = retXML.IndexOf("</dhRecbto>") - indexIni;
            return retXML.Substring(indexIni, comp);
        }
        public String getRetNFProt()
        {
            int indexIni = protNFe.IndexOf("<nProt>") + 7;
            int comp = protNFe.IndexOf("</nProt>") - indexIni;
            return protNFe.Substring(indexIni, comp);
        }
/*****************************************************************************************************************************************************/
/****************************** Get das Informações do Retorno do XML **********************************************************************************/
        public String getRetXML()
        {
            return retXML;
        }
        public String getRetRecibo()
        {
            int indexIni = retXML.IndexOf("<nRec>") + 6;
            int comp = retXML.IndexOf("</nRec>") - indexIni;
            return retXML.Substring(indexIni, comp);
        }
        public String getRetnProt()
        {
            int indexIni = retXML.IndexOf("<nProt>") + 7;
            int comp = retXML.IndexOf("</nProt>") - indexIni;
            return retXML.Substring(indexIni, comp);
        }
        public String getRetDhRecibo()
        {
            int indexIni = retXML.IndexOf("<dhRecbto>") +10;
            int comp = retXML.IndexOf("</dhRecbto>") - indexIni;
            return retXML.Substring(indexIni, comp);
        }
        public String getRetStat()
        {
            int indexIni = retXML.IndexOf("<cStat>") + 7;
            int comp = retXML.IndexOf("</cStat>") - indexIni;
            return retXML.Substring(indexIni, comp);
        }
        public String getRetMotivo()
        {
            int indexIni = retXML.IndexOf("<xMotivo>") + 9;
            int comp = retXML.IndexOf("</xMotivo>") - indexIni;
            return retXML.Substring(indexIni, comp);
        }
/*****************************************************************************************************************************************************/
/***************************************** Get das informações de Evento ******************************************************************************/
        public String getProcEvento()
        {
            return procEvento;
        }
        public void SalvarEventoProc(string caminho)
        {
            #region "Gera o arquivo XML do evento processado para distribuição"
            string path2 = caminho;
            TextWriter tw1 = new StreamWriter(path2);
            tw1.WriteLine(procEvento);
            tw1.Close();
            #endregion
        }
        public int hasEvento()
        {
            if (retXML.IndexOf("<procEventoNFe", indEvent) > -1)
            {
                int indexIni = retXML.IndexOf("<procEventoNFe", indEvent);
                int comp = retXML.IndexOf("</procEventoNFe>", indEvent) - indexIni + 16;
                indEvent = indexIni + comp - 1;
                procEvento = retXML.Substring(indexIni, comp);
                procEvento = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + procEvento; 
                return 1;
            }
            return 0;
        }
        public String getProcEventoStatus()
        {
            int indexIni = procEvento.IndexOf("<cStat>", procEvento.IndexOf("<retEvento")) + 7;
            int comp = procEvento.IndexOf("</cStat>", procEvento.IndexOf("<retEvento")) - indexIni;
            return procEvento.Substring(indexIni, comp);
        }
        public String getProcEventoMotivo()
        {
            int indexIni = procEvento.IndexOf("<xMotivo>", procEvento.IndexOf("<retEvento")) + 9;
            int comp = procEvento.IndexOf("</xMotivo>", procEvento.IndexOf("<retEvento")) - indexIni;
            return procEvento.Substring(indexIni, comp);
        }

        public String getProcEventoProt()
        {
            int indexIni = procEvento.IndexOf("<nProt>", procEvento.IndexOf("<retEvento")) + 7;
            int comp = procEvento.IndexOf("</nProt>", procEvento.IndexOf("<retEvento")) - indexIni;
            return procEvento.Substring(indexIni, comp);
        }

        public String getProcEventoNome()
        {
            int indexIni = procEvento.IndexOf("<xEvento>", procEvento.IndexOf("<retEvento")) + 9;
            int comp = procEvento.IndexOf("</xEvento>", procEvento.IndexOf("<retEvento")) - indexIni;
            return procEvento.Substring(indexIni, comp);
        }
        private void createProcEvento(string xmlEnvio, string xmlRet, string versao)
        {
            xmlRet = xmlRet.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");

            MemoryStream stream = new MemoryStream();
            XmlDocument documentEnvio = new XmlDocument();
            XmlDocument documentRet = new XmlDocument();
            documentEnvio.LoadXml(xmlEnvio.ToString());
            documentRet.LoadXml(xmlRet.ToString());

            using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.WriteStartElement("procEventoNFe");
                writer.WriteAttributeString("xmlns", "http://www.portalfiscal.inf.br/nfe");
                writer.WriteAttributeString("versao", versao);

                writer.WriteStartElement("evento");
                writer.WriteAttributeString("versao", versao);
                documentEnvio.GetElementsByTagName("evento").Item(0).WriteContentTo(writer);
                writer.WriteEndElement();

                writer.WriteStartElement("retEvento");
                writer.WriteAttributeString("versao", versao);
                documentRet.GetElementsByTagName("retEvento").Item(0).WriteContentTo(writer);
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.Flush();

                StreamReader reader = new StreamReader(stream, Encoding.UTF8, true);
                stream.Seek(0, SeekOrigin.Begin);

                procEvento += reader.ReadToEnd();
            }
            
        }
/*****************************************************************************************************************************************************/ 
        private String getSoapXml(string xml, string url, string cUF, string versao)
        {
           
            try
            {
                String result = String.Empty;
                MemoryStream stream = new MemoryStream();
                XmlDocument document = new XmlDocument();
                xml = xml.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
                xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                document.LoadXml(xml.ToString());

                using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
                {
                    //writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("soap12:Envelope");
                    writer.WriteAttributeString("xmlns:soap12", "http://www.w3.org/2003/05/soap-envelope");
                    writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");

                    writer.WriteStartElement("soap12:Body");
                    writer.WriteStartElement("nfeDadosMsg");
                    writer.WriteAttributeString("xmlns", url);

                    document.WriteContentTo(writer);

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();

                    StreamReader reader = new StreamReader(stream, Encoding.UTF8, true);
                    stream.Seek(0, SeekOrigin.Begin);

                    result += reader.ReadToEnd();
                }

                #region "Gera o arquivo XML para ver os dados"
                string path = pathApp + "\\NF-e_envio_soap.xml";
                TextWriter tw = new StreamWriter(path);
                tw.WriteLine(result.ToString());
                tw.Close();
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                errorBroken = ex.Message;
                errorBrokenDetalhado = ex.StackTrace;
                return String.Empty;
            }
        }

        private struct Estados
        {
            public const int RO = 11;
            public const int AC = 12;
            public const int AM = 13;
            public const int RR = 14;
            public const int PA = 15;
            public const int AP = 16;
            public const int TO = 17;
            public const int MA = 21;
            public const int PI = 22;
            public const int CE = 23;
            public const int RN = 24;
            public const int PB = 25;
            public const int PE = 26;
            public const int AL = 27;
            public const int SE = 28;
            public const int BA = 29;
            public const int MG = 31;
            public const int ES = 32;
            public const int RJ = 33;
            public const int SP = 35;
            public const int PR = 41;
            public const int SC = 42;
            public const int RS = 43;
            public const int MS = 50;
            public const int MT = 51;
            public const int GO = 52;
            public const int DF = 53;
        }

        #region "UF AUTORIZADORAS"
        struct UrlNF
        {
            public String cOrgaoRecep;
            public String _StatusServicos;
            public String ver_StatusServicos;
            public String _RetAutorizacao;
            public String ver_RetAutorizacao;
            public String _Autorizacaos;
            public String ver_Autorizacaos;
            public String _ConsultaNFe;
            public String ver_ConsultaNFe;
            public String _Inutilizacao;
            public String ver_Inutilizacao;
            public String _ConsultaCadastro;
            public String ver_ConsultaCadastro;
            public String _RecepcaoEvento;
            public String ver_RecepcaoEvento;

            public UrlNF(int cUF, int amb)
            {
                cOrgaoRecep = "";
                _StatusServicos     = "";
                ver_StatusServicos  = "";
                _RetAutorizacao     = "";
                ver_RetAutorizacao  = "";
                _Autorizacaos       = "";
                ver_Autorizacaos    = "";
                _ConsultaNFe        = "";
                ver_ConsultaNFe     = "";
                _Inutilizacao       = "";
                ver_Inutilizacao    = "";
                _ConsultaCadastro   = "";
                ver_ConsultaCadastro = "";
                _RecepcaoEvento     = "";
                ver_RecepcaoEvento  = "";

                if (amb == 1)
                {
                    /*switch (cUF)
                    {
                        case (int)Estados.SP:
                           cOrgaoRecep          = Convert.ToString(Estados.SP);
                           _StatusServicos      = @"https://nfe.fazenda.sp.gov.br/ws/nfestatusservico2.asmx";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao      = @"https://nfe.fazenda.sp.gov.br/ws/nferetautorizacao.asmx";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos        = @"https://nfe.fazenda.sp.gov.br/ws/nfeautorizacao.asmx";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe         = @"https://nfe.fazenda.sp.gov.br/ws/nfeconsulta2.asmx";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao        = @"https://nfe.fazenda.sp.gov.br/ws/nfeinutilizacao2.asmx";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro    = @"https://nfe.fazenda.sp.gov.br/ws/cadconsultacadastro2.asmx";
                           ver_ConsultaCadastro = "2.00";
                           _RecepcaoEvento      = @"https://nfe.fazenda.sp.gov.br/ws/recepcaoevento.asmx";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        case (int)Estados.AM:
                           cOrgaoRecep          = Convert.ToString(Estados.AM);
                           _StatusServicos      = @"https://nfe.sefaz.am.gov.br/services2/services/NfeStatusServico2";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao      = @"https://nfe.sefaz.am.gov.br/services2/services/NfeRetAutorizacao";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos        = @"https://nfe.sefaz.am.gov.br/services2/services/NfeAutorizacao";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe         = @"https://nfe.sefaz.am.gov.br/services2/services/NfeConsulta2";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao        = @"https://nfe.sefaz.am.gov.br/services2/services/NfeInutilizacao2";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro    = @"https://nfe.sefaz.am.gov.br/services2/services/cadconsultacadastro2";
                           ver_ConsultaCadastro = "3.10";
                           _RecepcaoEvento      = @"https://nfe.sefaz.am.gov.br/services2/services/RecepcaoEvento";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        case (int)Estados.BA:
                           cOrgaoRecep          = Convert.ToString(Estados.BA);
                           _StatusServicos      = @"https://nfe.sefaz.ba.gov.br/webservices/NfeStatusServico/NfeStatusServico.asmx";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao      = @"https://nfe.sefaz.ba.gov.br/webservices/NfeRetAutorizacao/NfeRetAutorizacao.asmx";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos        = @"https://nfe.sefaz.ba.gov.br/webservices/NfeAutorizacao/NfeAutorizacao.asmx";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe         = @"https://nfe.sefaz.ba.gov.br/webservices/NfeConsulta/NfeConsulta.asmx";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao        = @"https://nfe.sefaz.ba.gov.br/webservices/NfeInutilizacao/NfeInutilizacao.asmx";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro    = @"https://nfe.sefaz.ba.gov.br/webservices/nfenw/CadConsultaCadastro2.asmx";
                           ver_ConsultaCadastro = "3.10";
                           _RecepcaoEvento      = @"https://nfe.sefaz.ba.gov.br/webservices/sre/recepcaoevento.asmx";
                           ver_RecepcaoEvento   = "3.10";
                           break;

                        case (int)Estados.CE:
                           cOrgaoRecep          = Convert.ToString(Estados.CE);
                           _StatusServicos      = @"https://nfe.sefaz.ce.gov.br/nfe2/services/NfeStatusServico2?wsdl";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.sefaz.ce.gov.br/nfe2/services/NfeRetAutorizacao?wsdl";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.sefaz.ce.gov.br/nfe2/services/NfeAutorizacao?wsdl";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.sefaz.ce.gov.br/nfe2/services/NfeConsulta2?wsdl";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.sefaz.ce.gov.br/nfe2/services/NfeInutilizacao2?wsdl";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://nfe.sefaz.ce.gov.br/nfe2/services/CadConsultaCadastro2?wsdl";
                           ver_ConsultaCadastro = "3.10";
                           _RecepcaoEvento      = @"https://nfe.sefaz.ce.gov.br/nfe2/services/RecepcaoEvento?wsdl";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        case (int)Estados.GO:
                            cOrgaoRecep          = Convert.ToString(Estados.GO);
                            _StatusServicos = @"https://nfe.sefaz.go.gov.br/nfe/services/v2/NfeStatusServico2?wsdl";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.sefaz.go.gov.br/nfe/services/v2/NfeRetAutorizacao?wsdl";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.sefaz.go.gov.br/nfe/services/v2/NfeAutorizacao?wsdl";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.sefaz.go.gov.br/nfe/services/v2/NfeConsulta2?wsdl";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.sefaz.go.gov.br/nfe/services/v2/NfeInutilizacao2?wsdl";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://nfe.sefaz.go.gov.br/nfe/services/v2/CadConsultaCadastro2?wsdl";
                           ver_ConsultaCadastro = "3.10";
                           _RecepcaoEvento = @"https://nfe.sefaz.go.gov.br/nfe/services/v2/RecepcaoEvento?wsdl";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        case (int)Estados.MG:
                            cOrgaoRecep          = Convert.ToString(Estados.MG);
                            _StatusServicos = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NfeStatus2";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NfeRetAutorizacao";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NfeAutorizacao";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NfeConsulta2";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NfeInutilizacao2";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://nfe.fazenda.mg.gov.br/nfe2/services/cadconsultacadastro2";
                           ver_ConsultaCadastro = "2.00";
                           _RecepcaoEvento = @"https://nfe.fazenda.mg.gov.br/nfe2/services/RecepcaoEvento";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        case (int)Estados.MS:
                            cOrgaoRecep          = Convert.ToString(Estados.MS);
                            _StatusServicos = @"https://nfe.fazenda.ms.gov.br/producao/services2/NfeStatusServico2";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.fazenda.ms.gov.br/producao/services2/NfeRetAutorizacao";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.fazenda.ms.gov.br/producao/services2/NfeAutorizacao";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.fazenda.ms.gov.br/producao/services2/NfeConsulta2";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.fazenda.ms.gov.br/producao/services2/NfeInutilizacao2";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://nfe.fazenda.ms.gov.br/producao/services2/CadConsultaCadastro2";
                           ver_ConsultaCadastro = "2.00";
                           _RecepcaoEvento = @"https://nfe.fazenda.ms.gov.br/producao/services2/RecepcaoEvento";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        case (int)Estados.MT:
                            cOrgaoRecep          = Convert.ToString(Estados.MG);
                            _StatusServicos = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeStatusServico2?wsdl";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeRetAutorizacao?wsdl";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeAutorizacao?wsdl";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeConsulta2?wsdl";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeInutilizacao2?wsdl";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/CadConsultaCadastro2?wsdl";
                           ver_ConsultaCadastro = "3.10";
                           _RecepcaoEvento = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/RecepcaoEvento?wsdl";
                           ver_RecepcaoEvento   = "3.10";
                           break;

                        case (int)Estados.PE:
                            cOrgaoRecep          = Convert.ToString(Estados.PE);
                            _StatusServicos = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeStatusServico2";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeRetAutorizacao?wsdl";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeAutorizacao?wsdl";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeConsulta2";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeInutilizacao2";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/CadConsultaCadastro2";
                           ver_ConsultaCadastro = "3.10";
                           _RecepcaoEvento = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/RecepcaoEvento";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        case (int)Estados.PR:
                            cOrgaoRecep          = Convert.ToString(Estados.PR);
                            _StatusServicos = @"https://nfe.fazenda.pr.gov.br/nfe/NFeStatusServico3?wsdl";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.fazenda.pr.gov.br/nfe/NFeRetAutorizacao3?wsdl";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.fazenda.pr.gov.br/nfe/NFeAutorizacao3?wsdl";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.fazenda.pr.gov.br/nfe/NFeConsulta3?wsdl";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.fazenda.pr.gov.br/nfe/NFeInutilizacao3?wsdl";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://nfe.fazenda.pr.gov.br/nfe/CadConsultaCadastro2?wsdl";
                           ver_ConsultaCadastro = "3.10";
                           _RecepcaoEvento = @"https://nfe.fazenda.pr.gov.br/nfe/NFeRecepcaoEvento?wsdl";
                           ver_RecepcaoEvento   = "3.10";
                           break;

                        case (int)Estados.RS:
                            cOrgaoRecep          = Convert.ToString(Estados.RS);
                            _StatusServicos = @"https://nfe.sefazrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico2.asmx";
                           ver_StatusServicos   = "3.10";
                           _RetAutorizacao = @"https://nfe.sefazrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao.asmx";
                           ver_RetAutorizacao   = "3.10";
                           _Autorizacaos = @"https://nfe.sefazrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao.asmx";
                           ver_Autorizacaos     = "3.10";
                           _ConsultaNFe = @"https://nfe.sefazrs.rs.gov.br/ws/NfeConsulta/NfeConsulta2.asmx";
                           ver_ConsultaNFe      = "3.10";
                           _Inutilizacao = @"https://nfe.sefazrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao2.asmx";
                           ver_Inutilizacao     = "3.10";
                           _ConsultaCadastro = @"https://cad.sefazrs.rs.gov.br/ws/cadconsultacadastro/cadconsultacadastro2.asmx";
                           ver_ConsultaCadastro = "1.00";
                           _RecepcaoEvento = @"https://nfe.sefazrs.rs.gov.br/ws/recepcaoevento/recepcaoevento.asmx";
                           ver_RecepcaoEvento   = "1.00";
                           break;

                        default:
                            #region "SVRS - SEFAZ VIRTUAL RIO GRANDE DO SUL = AC, AL, AP, DF, ES, PB, RJ, RN, RO, RR, SC, SE, TO"
                            if (cUF == (int)Estados.AC ||
                                    cUF == (int)Estados.AL ||
                                    cUF == (int)Estados.AP ||
                                    cUF == (int)Estados.DF ||
                                    cUF == (int)Estados.ES ||
                                    cUF == (int)Estados.PB ||
                                    cUF == (int)Estados.RJ ||
                                    cUF == (int)Estados.RN ||
                                    cUF == (int)Estados.RO ||
                                    cUF == (int)Estados.RR ||
                                    cUF == (int)Estados.SC ||
                                    cUF == (int)Estados.SE ||
                                    cUF == (int)Estados.TO)
                            {
                                cOrgaoRecep         = Convert.ToString(Estados.RS);
                                _StatusServicos     = @"https://nfe.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico2.asmx";
                                ver_StatusServicos  = "3.10";
                                _RetAutorizacao     = @"https://nfe.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao.asmx";
                                ver_RetAutorizacao  = "3.10";
                                _Autorizacaos       = @"https://nfe.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao.asmx";
                                ver_Autorizacaos    = "3.10";
                                _ConsultaNFe        = @"https://nfe.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta2.asmx";
                                ver_ConsultaNFe     = "3.10";
                                _Inutilizacao       = @"https://nfe.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao2.asmx";
                                ver_Inutilizacao    = "3.10";
                                _ConsultaCadastro   = @"https://cad.svrs.rs.gov.br/ws/cadconsultacadastro/cadconsultacadastro2.asmx";
                                ver_ConsultaCadastro = "1.00";
                                _RecepcaoEvento     = @"https://nfe.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento.asmx";
                                ver_RecepcaoEvento  = "1.00";
                            }
                            #endregion
                            else
                                #region "SEFAZ VIRTUAL AMBIENTE NACIONAL = MA, PA, PI"
                                if (cUF == (int)Estados.MA || cUF == (int)Estados.PA || cUF == (int)Estados.PI)
                                {
                                    _StatusServicos = @"https://www.sefazvirtual.fazenda.gov.br/NfeStatusServico2/NfeStatusServico2.asmx ";
                                    _RetAutorizacao = @"https://www.sefazvirtual.fazenda.gov.br/NfeRetAutorizacao/NfeRetAutorizacao.asmx";
                                    _Autorizacaos   = @"https://www.sefazvirtual.fazenda.gov.br/NfeAutorizacao/NfeAutorizacao.asmx";
                                    _ConsultaNFe    = @"https://www.sefazvirtual.fazenda.gov.br/NfeConsulta2/NfeConsulta2.asmx ";
                                    _Inutilizacao   = @"https://www.sefazvirtual.fazenda.gov.br/NfeInutilizacao2/NfeInutilizacao2.asmx ";
                                    _RecepcaoEvento = @"https://www.sefazvirtual.fazenda.gov.br/RecepcaoEvento/RecepcaoEvento.asmx";
                                }
                                #endregion
                            break;
                    }*/
                    switch (cUF)
                    {
                        case (int)Estados.SP:
                            cOrgaoRecep = Convert.ToString(Estados.SP);
                            _StatusServicos = @"https://nfe.fazenda.sp.gov.br/ws/nfestatusservico4.asmx";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.fazenda.sp.gov.br/ws/nfeconsultaprotocolo4.asmx";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.fazenda.sp.gov.br/ws/nfeinutilizacao4.asmx";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.fazenda.sp.gov.br/ws/cadconsultacadastro4.asmx";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.fazenda.sp.gov.br/ws/nferecepcaoevento4.asmx";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.AM:
                            cOrgaoRecep = Convert.ToString(Estados.AM);
                            _StatusServicos = @"https://nfe.sefaz.am.gov.br/services2/services/NfeStatusServico4";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefaz.am.gov.br/services2/services/NfeRetAutorizacao4";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefaz.am.gov.br/services2/services/NfeAutorizacao4";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefaz.am.gov.br/services2/services/NfeConsulta4";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefaz.am.gov.br/services2/services/NfeInutilizacao4";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.sefaz.am.gov.br/services2/services/cadconsultacadastro2";
                            ver_ConsultaCadastro = "3.10";
                            _RecepcaoEvento = @"https://nfe.sefaz.am.gov.br/services2/services/RecepcaoEvento4";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.BA:
                            cOrgaoRecep = Convert.ToString(Estados.BA);
                            _StatusServicos = @"https://nfe.sefaz.ba.gov.br/webservices/NFeStatusServico4/NFeStatusServico4.asmx";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefaz.ba.gov.br/webservices/NFeRetAutorizacao4/NFeRetAutorizacao4.asmx";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefaz.ba.gov.br/webservices/NFeAutorizacao4/NFeAutorizacao4.asmx";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefaz.ba.gov.br/webservices/NFeConsultaProtocolo4/NFeConsultaProtocolo4.asmx";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefaz.ba.gov.br/webservices/NFeInutilizacao4/NFeInutilizacao4.asmx";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.sefaz.ba.gov.br/webservices/CadConsultaCadastro4/CadConsultaCadastro4.asmx";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.sefaz.ba.gov.br/webservices/NFeRecepcaoEvento4/NFeRecepcaoEvento4.asmx";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.CE:
                            cOrgaoRecep = Convert.ToString(Estados.CE);
                            _StatusServicos = @"https://nfe.sefaz.ce.gov.br/nfe4/services/NfeStatusServico4?wsdl";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefaz.ce.gov.br/nfe4/services/NfeRetAutorizacao4?wsdl";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefaz.ce.gov.br/nfe4/services/NfeAutorizacao4?wsdl";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefaz.ce.gov.br/nfe4/services/NFeConsultaProtocolo4?wsdl";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefaz.ce.gov.br/nfe4/services/NfeInutilizacao4?wsdl";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.sefaz.ce.gov.br/nfe4/services/CadConsultaCadastro4?wsdl";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.sefaz.ce.gov.br/nfe4/services/NFeRecepcaoEvento4?wsdl";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.GO:
                            cOrgaoRecep = Convert.ToString(Estados.GO);
                            _StatusServicos = @"https://nfe.sefaz.go.gov.br/nfe/services/NfeStatusServico4?wsdl";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefaz.go.gov.br/nfe/services/NfeRetAutorizacao4?wsdl";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefaz.go.gov.br/nfe/services/NfeAutorizacao4?wsdl";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefaz.go.gov.br/nfe/services/NFeConsultaProtocolo4?wsdl";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefaz.go.gov.br/nfe/services/NfeInutilizacao4?wsdl";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.sefaz.go.gov.br/nfe/services/CadConsultaCadastro4?wsdl";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.sefaz.go.gov.br/nfe/services/NFeRecepcaoEvento4?wsdl";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.MG:
                            cOrgaoRecep = Convert.ToString(Estados.MG);
                            _StatusServicos = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NFeStatusServico4";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NFeRetAutorizacao4";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NFeAutorizacao4";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NFeConsultaProtocolo4";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NFeInutilizacao4";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.fazenda.mg.gov.br/nfe2/services/cadconsultacadastro2";
                            ver_ConsultaCadastro = "2.00";
                            _RecepcaoEvento = @"https://nfe.fazenda.mg.gov.br/nfe2/services/NFeRecepcaoEvento4";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.MS:
                            cOrgaoRecep = Convert.ToString(Estados.MS);
                            _StatusServicos = @"https://nfe.fazenda.ms.gov.br/ws/NFeStatusServico4";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.fazenda.ms.gov.br/ws/NFeRetAutorizacao4";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.fazenda.ms.gov.br/ws/NFeAutorizacao4";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.fazenda.ms.gov.br/ws/NFeConsultaProtocolo4";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.fazenda.ms.gov.br/ws/NFeInutilizacao4";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.fazenda.ms.gov.br/ws/CadConsultaCadastro4";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.fazenda.ms.gov.br/ws/NFeRecepcaoEvento4";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.MT:
                            cOrgaoRecep = Convert.ToString(Estados.MG);
                            _StatusServicos = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeStatusServico4?wsdl";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeRetAutorizacao4?wsdl";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeAutorizacao4?wsdl";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeConsulta4?wsdl";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/NfeInutilizacao4?wsdl";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/CadConsultaCadastro4?wsdl";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.sefaz.mt.gov.br/nfews/v2/services/RecepcaoEvento4?wsdl";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.PE:
                            cOrgaoRecep = Convert.ToString(Estados.PE);
                            _StatusServicos = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeStatusServico4";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeRetAutorizacao4";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeAutorizacao4";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NFeConsultaProtocolo4";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NfeInutilizacao4";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/CadConsultaCadastro2";
                            ver_ConsultaCadastro = "3.10";
                            _RecepcaoEvento = @"https://nfe.sefaz.pe.gov.br/nfe-service/services/NFeRecepcaoEvento4";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.PR:
                            cOrgaoRecep = Convert.ToString(Estados.PR);
                            _StatusServicos = @"https://nfe.sefa.pr.gov.br/nfe/NFeStatusServico4?wsdl";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefa.pr.gov.br/nfe/NFeRetAutorizacao4?wsdl";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefa.pr.gov.br/nfe/NFeAutorizacao4?wsdl";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefa.pr.gov.br/nfe/NFeConsultaProtocolo4?wsdl";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefa.pr.gov.br/nfe/NFeInutilizacao4?wsdl";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://nfe.sefa.pr.gov.br/nfe/CadConsultaCadastro4?wsdl";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.sefa.pr.gov.br/nfe/NFeRecepcaoEvento4?wsdl";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        case (int)Estados.RS:
                            cOrgaoRecep = Convert.ToString(Estados.RS);
                            _StatusServicos = @"https://nfe.sefazrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx";
                            ver_StatusServicos = "4.00";
                            _RetAutorizacao = @"https://nfe.sefazrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx";
                            ver_RetAutorizacao = "4.00";
                            _Autorizacaos = @"https://nfe.sefazrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx";
                            ver_Autorizacaos = "4.00";
                            _ConsultaNFe = @"https://nfe.sefazrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx";
                            ver_ConsultaNFe = "4.00";
                            _Inutilizacao = @"https://nfe.sefazrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx";
                            ver_Inutilizacao = "4.00";
                            _ConsultaCadastro = @"https://cad.sefazrs.rs.gov.br/ws/cadconsultacadastro/cadconsultacadastro4.asmx";
                            ver_ConsultaCadastro = "4.00";
                            _RecepcaoEvento = @"https://nfe.sefazrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx";
                            ver_RecepcaoEvento = "1.00";
                            break;

                        default:
                            #region "SVRS - SEFAZ VIRTUAL RIO GRANDE DO SUL = AC, AL, AP, DF, ES, PB, RJ, RN, RO, RR, SC, SE, TO"
                            if (cUF == (int)Estados.AC ||
                                    cUF == (int)Estados.AL ||
                                    cUF == (int)Estados.AP ||
                                    cUF == (int)Estados.DF ||
                                    cUF == (int)Estados.ES ||
                                    cUF == (int)Estados.PB ||
                                    cUF == (int)Estados.RJ ||
                                    cUF == (int)Estados.RN ||
                                    cUF == (int)Estados.RO ||
                                    cUF == (int)Estados.RR ||
                                    cUF == (int)Estados.SC ||
                                    cUF == (int)Estados.SE ||
                                    cUF == (int)Estados.TO)
                            {
                                cOrgaoRecep = Convert.ToString(Estados.RS);
                                _StatusServicos = @"https://nfe.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx";
                                ver_StatusServicos = "4.00";
                                _RetAutorizacao = @"https://nfe.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx";
                                ver_RetAutorizacao = "4.00";
                                _Autorizacaos = @"https://nfe.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx";
                                ver_Autorizacaos = "4.00";
                                _ConsultaNFe = @"https://nfe.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx";
                                ver_ConsultaNFe = "4.00";
                                _Inutilizacao = @"https://nfe.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx";
                                ver_Inutilizacao = "4.00";
                                _ConsultaCadastro = @"https://cad.svrs.rs.gov.br/ws/cadconsultacadastro/cadconsultacadastro4.asmx";
                                ver_ConsultaCadastro = "4.00";
                                _RecepcaoEvento = @"https://nfe.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx";
                                ver_RecepcaoEvento = "1.00";
                            }
                            #endregion
                            else
                                #region "SEFAZ VIRTUAL AMBIENTE NACIONAL = MA, PA, PI"
                                if (cUF == (int)Estados.MA || cUF == (int)Estados.PA || cUF == (int)Estados.PI)
                                {
                                    _StatusServicos = @"https://www.sefazvirtual.fazenda.gov.br/NFeStatusServico4/NFeStatusServico4.asmx ";
                                    ver_StatusServicos = "4.00";
                                    _RetAutorizacao = @"https://www.sefazvirtual.fazenda.gov.br/NFeRetAutorizacao4/NFeRetAutorizacao4.asmx";
                                    ver_RetAutorizacao = "4.00";
                                    _Autorizacaos = @"https://www.sefazvirtual.fazenda.gov.br/NFeAutorizacao4/NFeAutorizacao4.asmx";
                                    ver_Autorizacaos = "4.00";
                                    _ConsultaNFe = @"https://www.sefazvirtual.fazenda.gov.br/NFeConsultaProtocolo4/NFeConsultaProtocolo4.asmx ";
                                    ver_ConsultaNFe = "4.00";
                                    _Inutilizacao = @"https://www.sefazvirtual.fazenda.gov.br/NFeInutilizacao4/NFeInutilizacao4.asmx ";
                                    ver_Inutilizacao = "4.00";
                                    _RecepcaoEvento = @"https://www.sefazvirtual.fazenda.gov.br/NFeRecepcaoEvento4/NFeRecepcaoEvento4.asmx";
                                    ver_RecepcaoEvento = "1.00";
                                }
                                #endregion
                            break;
                    }
                }
                else
                {
                    switch (cUF)
                    {
                        case (int)Estados.SP:
                          cOrgaoRecep           = Convert.ToString(Estados.SP);
                          _StatusServicos       = @"https://homologacao.nfe.fazenda.sp.gov.br/ws/nfestatusservico4.asmx";
                          ver_StatusServicos    = "4.00";
                          _RetAutorizacao       = @"https://homologacao.nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx";
                          ver_RetAutorizacao    = "4.00";
                          _Autorizacaos         = @"https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx";
                          ver_Autorizacaos      = "4.00";
                          _ConsultaNFe          = @"https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeconsultaprotocolo4.asmx";
                          ver_ConsultaNFe       = "4.00";
                          _Inutilizacao         = @"https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeinutilizacao4.asmx";
                          ver_Inutilizacao      = "4.00";
                          _ConsultaCadastro     = @"https://homologacao.nfe.fazenda.sp.gov.br/ws/cadconsultacadastro4.asmx";
                          ver_ConsultaCadastro  = "4.00";
                          _RecepcaoEvento       = @"https://homologacao.nfe.fazenda.sp.gov.br/ws/nferecepcaoevento4.asmx";
                          ver_RecepcaoEvento    = "1.00";
                          break;
                        case (int) Estados.PR:
                            cOrgaoRecep           = Convert.ToString(Estados.SP);
                            _StatusServicos =    @"https://homologacao.nfe.fazenda.pr.gov.br/nfe/NFeStatusServico3?wsdl";
                          ver_StatusServicos    = "3.10";
                          _RetAutorizacao = @"https://homologacao.nfe.fazenda.pr.gov.br/nfe/NFeRetAutorizacao3?wsdl";
                          ver_RetAutorizacao    = "3.10";
                          _Autorizacaos = @"https://homologacao.nfe.fazenda.pr.gov.br/nfe/NFeAutorizacao3?wsdl";
                          ver_Autorizacaos      = "3.10";
                          _ConsultaNFe = @"https://homologacao.nfe.fazenda.pr.gov.br/nfe/NFeConsulta3?wsdl";
                          ver_ConsultaNFe       = "3.10";
                          _Inutilizacao = @"https://homologacao.nfe.fazenda.pr.gov.br/nfe/NFeInutilizacao3?wsdl";
                          ver_Inutilizacao      = "3.10";
                          _ConsultaCadastro = @"https://homologacao.nfe.fazenda.pr.gov.br/nfe/CadConsultaCadastro2?wsdl";
                          ver_ConsultaCadastro  = "3.10";
                          _RecepcaoEvento = @"https://homologacao.nfe.fazenda.pr.gov.br/nfe/NFeRecepcaoEvento?wsdl";
                          ver_RecepcaoEvento    = "3.10";
                          break;
                    }
                }
            }
        }
        #endregion
    }
}
