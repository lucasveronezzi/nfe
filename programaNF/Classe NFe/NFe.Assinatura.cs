using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;

namespace programaNF
{
    public partial class NFe
    {
        private SignedXml signedXml;
        private String xmlAssinado;
        private X509Certificate2 xCert = null;

        public int AssinarNFE(string xml, string cnpj, string tagName)
        {
            try
            {
                xml = alteraCaracter(xml, 1);
                xmlAssinado = String.Empty;
                if (xCert == null)
                    xCert = selectCert(cnpj);

                if (xCert != null)
                {
                    xml = xml.Replace("\r\n", "");
                    string tagNameID = "";
                    if (tagName == "NFe")
                        tagNameID = "infNFe";
                    else 
                        if (tagName == "inutNFe")
                            tagNameID = "infInut";
                        else
                            tagNameID = "infEvento";

                    XmlDocument docRequest = new XmlDocument();
                    docRequest.PreserveWhitespace = false;

                    string docXML = String.Empty;

                    if (xml.StartsWith("<"))
                        docXML = xml.ToString();
                    else
                        docXML = File.ReadAllText(xml);

                    docRequest.LoadXml(remove_non_ascii(removeAcentuacao(docXML.ToString())));

                    XmlNodeList ListInfNFe = docRequest.GetElementsByTagName(tagNameID);
                    
                    foreach (XmlElement infNFe in ListInfNFe)
                    {
                        string id = infNFe.Attributes.GetNamedItem("Id").InnerText;
                        signedXml = new SignedXml(infNFe);
                        signedXml.SigningKey = xCert.PrivateKey;

                        Reference reference = new Reference("#" + id);
                        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                        reference.AddTransform(new XmlDsigC14NTransform());
                        signedXml.AddReference(reference);

                        KeyInfo keyInfo = new KeyInfo();
                        keyInfo.AddClause(new KeyInfoX509Data(xCert));

                        signedXml.KeyInfo = keyInfo;

                        signedXml.ComputeSignature();

                        XmlElement xmlSignature = docRequest.CreateElement("Signature", "http://www.w3.org/2000/09/xmldsig#");
                        XmlElement xmlSignedInfo = signedXml.SignedInfo.GetXml();
                        XmlElement xmlKeyInfo = signedXml.KeyInfo.GetXml();

                        XmlElement xmlSignatureValue = docRequest.CreateElement("SignatureValue", xmlSignature.NamespaceURI);
                        string signBase64 = Convert.ToBase64String(signedXml.Signature.SignatureValue, Base64FormattingOptions.InsertLineBreaks);
                        XmlText text = docRequest.CreateTextNode(signBase64);
                        xmlSignatureValue.AppendChild(text);

                        xmlSignature.AppendChild(docRequest.ImportNode(xmlSignedInfo, true));
                        xmlSignature.AppendChild(xmlSignatureValue);
                        xmlSignature.AppendChild(docRequest.ImportNode(xmlKeyInfo, true));

                        var evento = docRequest.GetElementsByTagName(tagName);
                        evento[0].AppendChild(xmlSignature);
                    }

                    xmlAssinado = docRequest.OuterXml;
                    docRequest.Save(pathApp + "\\NF-e_assinada.xml");
                    return 0;
                }
                else
                {
                    errorBroken = "Nenhum Certificado Digital selecionado";
                    return 999;
                }
            }
            catch (XmlException e1)
            {
                errorBrokenDetalhado = e1.StackTrace;
                errorBroken = e1.Message;
                return 999;
            }
            catch (CryptographicException e2)
            {
                errorBrokenDetalhado = e2.StackTrace;
                errorBroken = e2.Message;
                return 999;
            }
            catch (Exception e3)
            {
                errorBrokenDetalhado = e3.StackTrace;
                errorBroken = e3.Message;
                return 999;
            }
        }

        public String getXmlAssinado()
        {
            return xmlAssinado;
        }

        public X509Certificate2 selectCert(String cnpj)
        {
            try
            {
                X509Certificate2 certSelected = null;
                X509Store x509Store = new X509Store("My");
                x509Store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection col = x509Store.Certificates;
                X509Certificate2Collection cFiltro = col.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                if (cFiltro.Count > 0)
                {
                    Form prompt = new Form()
                    {
                        Width = 450,
                        Height = 130,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        Text = "Selecione um Certificado Digital",
                        StartPosition = FormStartPosition.CenterScreen,

                    };
                    Label textLabel = new Label() { Left = 12, Top = 10, Width = 400, Text = "Certificados dentro do Prazo de Validade" };
                    Button confirmation = new Button() { Text = "OK", Left = 330, Width = 100, Top = 70, DialogResult = DialogResult.OK };
                    confirmation.Click += (sender, e) => { prompt.Close(); };

                    ComboBox cmb = new ComboBox();
                    cmb.Location = new System.Drawing.Point(12, 40);
                    cmb.Size = new System.Drawing.Size(410, 20);
                    cmb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cmb.DropDownWidth = 400;
                    System.Object[] ItemRange = new System.Object[cFiltro.Count];
                    int x = 0;
                    foreach (X509Certificate2 certificate in cFiltro)
                    {
                        String emp = certificate.Subject;
                        int indexEmp = emp.IndexOf("CN=");
                        if (indexEmp >= 0)
                        {
                            int indexCNPJ = emp.IndexOf(":", indexEmp);
                            if (indexCNPJ - indexEmp > 0)
                                ItemRange[x] = emp.Substring(indexEmp, indexCNPJ - indexEmp);
                            else
                                ItemRange[x] = emp;
                        }
                        else
                        {
                            //ItemRange[x] = certificate.Subject;
                        }
                        x++;
                    }
                    cmb.Items.AddRange(ItemRange);
                    cmb.SelectedIndex = 0;

                    prompt.Controls.Add(textLabel);
                    prompt.Controls.Add(confirmation);
                    prompt.Controls.Add(cmb);
                    prompt.AcceptButton = confirmation;

                    if (prompt.ShowDialog() == DialogResult.OK)
                    {
                        String cnpjCert;

                        certSelected = cFiltro[cmb.SelectedIndex];

                        String emp = certSelected.SubjectName.Name;
                        int indexEmp = emp.IndexOf("CN=");
                        int indexCNPJ = emp.IndexOf(":", indexEmp);

                        if (indexCNPJ > 0) cnpjCert = emp.Substring(indexCNPJ + 1, 14);
                        else cnpjCert = cnpj;

                        if (certSelected.HasPrivateKey && (cnpj == "" || cnpjCert == cnpj))
                        {
                            return certSelected;
                        }
                        else
                        {
                            x509Store.Close();
                            System.Windows.Forms.MessageBox.Show("Certificado Digital Selecionado é invalido ou não corresponde com o CNPJ do Emitente.");
                            return selectCert(cnpj);
                        }
                    }
                }else
                    System.Windows.Forms.MessageBox.Show("Não foi localizado nenhum certificado digital instalado.");

                return null;
             
                /*X509Certificate2Collection sel = X509Certificate2UI.SelectFromCollection(cFiltro, "Certificados Digitais dentro do prazo de validade", "Selecione um certificado para Assinar a NF-E", X509SelectionFlag.SingleSelection);

                if (sel.Count > 0)
                {
                    X509Certificate2Enumerator en = sel.GetEnumerator();
                    en.MoveNext();
                    certSelected = en.Current;

                    String cnpjCert;

                    String emp = certSelected.SubjectName.Name;
                    int indexEmp = emp.IndexOf("CN=");
                    int indexCNPJ = emp.IndexOf(":", indexEmp);

                    if (indexCNPJ > 0) cnpjCert = emp.Substring(indexCNPJ + 1, 14);
                    else cnpjCert = cnpj;

                    if (certSelected.HasPrivateKey && (cnpj == "" || cnpjCert == cnpj))
                    {
                        return certSelected;
                    }
                    else
                    {
                        x509Store.Close();
                        System.Windows.Forms.MessageBox.Show("Certificado Digital Selecionado é invalido ou não corresponde com o CNPJ do Emitente.");
                        return selectCert(cnpj);
                    }
                }
                else
                {
                    return null;
                }*/
            }
            catch (Exception e)
            {
                errorBrokenDetalhado = e.StackTrace;
                errorBroken = e.Message;
                #region "Gera o arquivo de error"
                string path2 = pathApp + "\\erro_assinatura.txt";
                TextWriter tw1 = new StreamWriter(path2);
                tw1.WriteLine(e.StackTrace);
                tw1.Close();
                #endregion
                System.Windows.Forms.MessageBox.Show(e.Message);
                return null;
            }
            
        }
    }
}
