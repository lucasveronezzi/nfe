using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.Windows.Forms;

namespace programaNF
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("programaNF.NFe")]
    [ComVisible(true)]
    public partial class NFe
    {
        #region "Atributos"
        private String errorBroken;
        private String errorBrokenDetalhado;
        private String retXML;
        private String protNFe;
        private String procEvento;
        private String pathApp;
        private CookieContainer cookies;
        private List<String> listErro = new List<string>();
        private List<String> listAlerta = new List<string>();
        #endregion

        public NFe()
        {
            pathApp = Path.GetDirectoryName(Application.ExecutablePath);

            pathApp = pathApp+"\\vs_nfe\\xml";
               
            //MessageBox.Show(pathApp);
            bool exists = System.IO.Directory.Exists(pathApp);

           if (!exists)
               System.IO.Directory.CreateDirectory(pathApp);
        }

        public int ValidarXML(string arquivoXml, string schemaXML)
        {
            try
            {
                arquivoXml = alteraCaracter(arquivoXml, 1);
                listErro.Clear();
                listAlerta.Clear();

                XmlReader xmlReader;
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add("http://www.portalfiscal.inf.br/nfe", schemaXML);
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

                if(arquivoXml.StartsWith("<"))
                    xmlReader = XmlReader.Create(new StringReader(arquivoXml), settings);
                else
                {
                     xmlReader = XmlReader.Create(new StringReader(File.ReadAllText(arquivoXml)), settings);
                    
                }

                while (xmlReader.Read()) { }

                return 0;
            }

            catch(XmlException e1){
                errorBroken = e1.Message;
                errorBrokenDetalhado = e1.StackTrace;
                return 999;
            }

            catch(Exception e){
                errorBroken = e.Message;
                errorBrokenDetalhado = e.StackTrace;
                return 999;
            }
        }

        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                listAlerta.Add(e.Exception.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                if (!e.Exception.Message.Contains("xmldsig#Signature") && !e.Exception.Message.Contains("'NFe' no espaço para nome 'http://www.portalfiscal.inf.br/nfe' apresenta conteúdo incompleto") && !e.Exception.Message.Contains("'NFe' in namespace 'http://www.portalfiscal.inf.br/nfe' has incomplete content"))
                {
                    String message = e.Exception.Message;
                    message = message.Replace("cvc-type.3.1.3:", "");
                    message = message.Replace("cvc-complex-type.2.4.a:", "");
                    message = message.Replace("cvc-complex-type.2.4.b:", "");
                    message = message.Replace("cvc-attribute.3: ", "");
                    message = message.Replace("cvc-pattern-valid: ", "");
                    message = message.Replace("The value", "O valor");
                    message = message.Replace("of element", "do campo");
                    message = message.Replace("is not valid", "não é valido");
                    message = message.Replace("Invalid content was found starting with element", "Encontrado o campo");
                    message = message.Replace("One of", "Campo(s)");
                    message = message.Replace("is expected", "é obrigatorio");
                    message = message.Replace("\\{", "");
                    message = message.Replace("\\}", "");
                    message = message.Replace("\"", "");
                    message = message.Replace("http://www.portalfiscal.inf.br/nfe:", "");
                    message = message.Trim();
                    listErro.Add(message);
                }
            }  
        }
        public static string remove_non_ascii(string src)
        {
            return System.Text.RegularExpressions.Regex.Replace(src, @"[^\u0000-\u007F]", " ");
        }
        private string alteraCaracter(string texto, int op = 0)
        {
            if (op == 0)
            {
                texto = texto.Replace("<", "&lt;");
                texto = texto.Replace(">", "&gt;");
                texto = texto.Replace("\"", "&quot;");
            }

            texto = texto.Replace("&", "&lt;");
            texto = texto.Replace("'", "&#39;");
            return texto;
        }
        private string removeAcentuacao(string texto)
        {
            texto = texto.Replace("ã", "a");
            texto = texto.Replace("â", "a");
            texto = texto.Replace("á", "a");
            texto = texto.Replace("à", "a");
            texto = texto.Replace("ç", "c");
            texto = texto.Replace("é", "e");
            texto = texto.Replace("è", "e");
            texto = texto.Replace("ê", "e");
            texto = texto.Replace("õ", "o");
            texto = texto.Replace("ò", "o");
            texto = texto.Replace("ó", "o");
            texto = texto.Replace("ô", "o");
            texto = texto.Replace("ú", "u");
            texto = texto.Replace("ù", "u");
            texto = texto.Replace("í", "i");
            texto = texto.Replace("ì", "i");

            texto = texto.Replace("Ã", "A");
            texto = texto.Replace("Â", "A");
            texto = texto.Replace("Á", "A");
            texto = texto.Replace("À", "A");
            texto = texto.Replace("Ç", "C");
            texto = texto.Replace("É", "E");
            texto = texto.Replace("È", "E");
            texto = texto.Replace("Ê", "E");
            texto = texto.Replace("Õ", "O");
            texto = texto.Replace("Ò", "O");
            texto = texto.Replace("Ó", "O");
            texto = texto.Replace("Ô", "O");
            texto = texto.Replace("Ú", "U");
            texto = texto.Replace("Ù", "U");
            texto = texto.Replace("Í", "I");
            texto = texto.Replace("Ì", "I");

            texto = texto.Replace("&", "e");

            texto = texto.Replace("º", "");
            texto = texto.Replace("°", "");

            return texto;
        }

        public string getErroV(int index)
        {
            return listErro[index].Replace(@"\n", "");
        }

        public string getAlertaV(int index)
        {
            return listAlerta[index].Replace(@"\n", "");
        }

        public int getTotErro()
        {
            return listErro.Count;
        }

        public int getTotAlerta()
        {
            return listAlerta.Count;
        }

        public String getErroBroken()
        {
            MessageBox.Show(errorBroken);
            return errorBroken;
        }

        public String getErroBrokenDetalhado()
        {
            return errorBrokenDetalhado;
        }

        public void showLoading()
        {
            Form prompt = new Form();
            prompt.Width = 420;
            prompt.Height = 240;
            prompt.Text = String.Empty;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            prompt.TransparencyKey = System.Drawing.Color.Turquoise;
            prompt.BackColor = System.Drawing.Color.Turquoise;
            PictureBox picture = new PictureBox();
            picture.Height = 240;
            picture.Width = 420;
            picture.ImageLocation = "loading.gif";

            prompt.Controls.Add(picture);
            prompt.Visible = true;
            prompt.Show();
        }


    }
}

/*int too = ValidarXML(xmlDoc, @"C:\nf-e\xsd\Cancelar\envEventoCancNFe_v1.00.xsd");
            if (too != 0)
            {
                if (too > 100)
                {
                    return too;
                }
                else
                {
                    for (int x = 0; x < too; x++)
                    {
                        System.Windows.Forms.MessageBox.Show(getErro(x));
                    }
                    return too;
                }
            }*/