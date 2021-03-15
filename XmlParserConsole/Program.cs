using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using System;   
using System.Collections;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Layout.Borders;

namespace XmlParserConsole
{
    class Program
    {
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        static void Main(string[] args)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("C://Users//Таня//Desktop//Работа//endorsement.xml");
            // получим корневой элемент
            //XmlElement xRoot = xDoc.DocumentElement;
            //обход всех узлов в корневом элементе
            XmlContent Content = new XmlContent
            {
                RegistrationDate = xmlDocument.GetElementsByTagName("registration_date")[0].InnerText,
                RegistrationKind = xmlDocument.GetElementsByTagName("registration_kind")[0].InnerText,
                RegistrationNumber = xmlDocument.GetElementsByTagName("registration_number")[0].InnerText,
                X509Certificate = xmlDocument.GetElementsByTagName("X509Certificate")[0].InnerText
            };

            Hashtable deals = new Hashtable();
            deals.Add("454001000000", "Сделки об отчуждении объектов недвижимости");
            deals.Add("454010000000", "Передача жилья в собственность граждан(приватизация жилья)");
            deals.Add("454011000000", "Договор концессии");
            deals.Add("454001001000", "Договор купли-продажи");
            deals.Add("454012000000", "Соглашение об уступке прав по договору аренды");
            deals.Add("454001002000", "Договор дарения");
            deals.Add("454001003000", "Договор мены");
            deals.Add("454001004000", "Договор ренты");
            deals.Add("454001005000", "Договор пожизненного содержания с иждивением");
            deals.Add("454002000000", "Сделки, на основании которых возникают ограничения(обременения) прав");
            deals.Add("454002001000", "Договор аренды(субаренды)");
            deals.Add("454002002000", "Договор безвозмездного срочного пользования земельным / лесным участком");
            deals.Add("454002003000", "Договор об ипотеке(залоге)");
            deals.Add("454002004000", "Договор найма жилого помещения");
            deals.Add("454002005000", "Соглашение о восстановлении или замене погибшего(уничтоженного) или поврежденного имущества");
            deals.Add("454002006000", "Безвозмездное пользование(ссуда)");
            deals.Add("454003000000", "Договор участия в долевом строительстве");
            deals.Add("454004000000", "Соглашение об изменении условий договора");
            deals.Add("454005000000", "Соглашение об уступке требований (переводе долга) по договору");
            deals.Add("454006000000", "Соглашение о расторжении договора");
            deals.Add("454007000000", "Соглашение об изменении содержания закладной");
            deals.Add("454008000000", "Соглашение об уступке прав требования по договору участия в долевом строительстве");
            deals.Add("454009000000", "Дополнительное соглашение");
            deals.Add("454099000000", "Иная сделка");

            string dateInput = Content.RegistrationDate.Substring(0,Content.RegistrationDate.IndexOf("T"));
            string format = "yyyy-mm-dd";
            DateTime result=new DateTime();
            CultureInfo provider = CultureInfo.CreateSpecificCulture("ru-RU");
            try
            {
                result = DateTime.ParseExact(dateInput,format,provider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Content.RegistrationDate=result.ToString("D");

            Byte[] rawData = Convert.FromBase64String(Content.X509Certificate);
            X509Certificate2 certificate = new X509Certificate2(rawData);
            Console.WriteLine(certificate.ToString());
            string certData = certificate.ToString();

            //Организация
            string organisationPattern = @"CN=(.+?),";
            Regex organisationRegex = new Regex(organisationPattern, RegexOptions.IgnoreCase);
            Match organisationMatch = organisationRegex.Match(certData);
            Content.Organisation= organisationMatch.Value.Substring(3, organisationMatch.Value.Length - 4);

            //Email
            string emailPattern = @"E=(.+?),";
            Regex emailRegex = new Regex(emailPattern, RegexOptions.IgnoreCase);
            Match emailMatch = emailRegex.Match(certData);
            Content.Email = emailMatch.Value.Substring(2, emailMatch.Value.Length - 3);

            //ФИО
            string fPattern = @"SN=(.+?),";
            Regex fRegex = new Regex(fPattern, RegexOptions.IgnoreCase);
            Match fMatch = fRegex.Match(certData);
            string f = fMatch.Value.Substring(3, fMatch.Value.Length-4);
            string ioPattern = @"G=(.+?),";
            Regex ioRegex = new Regex(ioPattern, RegexOptions.IgnoreCase);
            Match ioMatch = ioRegex.Match(certData);
            string io = ioMatch.Value.Substring(2, ioMatch.Value.Length - 3);
            Content.FIO = f +" "+ io;

            //Адрес
            string sPattern = @"S=(.+?),";
            Regex sRegex = new Regex(sPattern, RegexOptions.IgnoreCase);
            Match sMatch = sRegex.Match(certData);
            string s = sMatch.Value.Substring(2, sMatch.Value.Length - 3);

            string lPattern = @"L=(.+?),";
            Regex lRegex = new Regex(lPattern, RegexOptions.IgnoreCase);
            Match lMatch = lRegex.Match(certData);
            string l = lMatch.Value.Substring(2, lMatch.Value.Length - 3);

            string streetPattern = "STREET=\"(.+?)\"";
            Regex streetRegex = new Regex(streetPattern, RegexOptions.IgnoreCase);
            Match streetMatch = streetRegex.Match(certData);
            string street = streetMatch.Value.Substring(8, streetMatch.Value.Length - 9);
            
            Content.Address = s+", "+l+". "+street;

            //ОГРН
            string ogrnPattern = @"OID.1.2.643.100.1=(.+?),";
            Regex ogrnRegex = new Regex(ogrnPattern, RegexOptions.IgnoreCase);
            Match ogrnMatch = ogrnRegex.Match(certData);
            Content.OGRN = ogrnMatch.Value.Substring(18, ogrnMatch.Value.Length - 19);

            //ИНН
            string innPattern = @"OID.1.2.643.3.131.1.1=(.+?),";
            Regex innRegex = new Regex(innPattern, RegexOptions.IgnoreCase);
            Match innMatch = innRegex.Match(certData);
            Content.INN = innMatch.Value.Substring(22, innMatch.Value.Length - 23);

            //СНИЛС
            string snilsPattern = @"OID.1.2.643.100.3=\d*";
            Regex snilsRegex = new Regex(snilsPattern, RegexOptions.IgnoreCase);
            Match snilsMatch = snilsRegex.Match(certData);
            Content.SNILS = snilsMatch.Value.Substring(18, snilsMatch.Value.Length - 18);

            //Серийный номер
            Content.SerialNumber = certData.Substring(certData.IndexOf("[Serial Number]")+19, 
                certData.IndexOf("[Not Before]")- certData.IndexOf("[Serial Number]")-22);

            //Действителен с
            Content.NotBefore = certData.Substring(certData.IndexOf("[Not Before]") + 16,
                certData.IndexOf("[Not After]") - certData.IndexOf("[Not Before]") - 23);

            //По
            Content.NotAfter = certData.Substring(certData.IndexOf("[Not After]") + 15,
                certData.IndexOf("[Thumbprint]") - certData.IndexOf("[Not After]") - 22);
            Console.WriteLine();

            Console.WriteLine("Произведена государственная регистрация: " + deals[Content.RegistrationKind]);
            Console.WriteLine("Дата регистрации: " + Content.RegistrationDate);
            Console.WriteLine("Номер регистрации: " + Content.RegistrationNumber);
            Console.WriteLine("Организация: " + Content.Organisation);
            Console.WriteLine("Владелец: " + Content.FIO);
            Console.WriteLine("ОГРН: " + Content.OGRN);
            Console.WriteLine("ИНН: " + Content.INN);
            Console.WriteLine("СНИЛС: " + Content.SNILS);
            Console.WriteLine("Email: " + Content.Email);
            Console.WriteLine("Адрес: " + Content.Address);
            Console.WriteLine("Cерийный номер: " + Content.SerialNumber);
            Console.WriteLine("Действителен: с " + Content.NotBefore+" по "+ Content.NotAfter);


            PdfFont font = PdfFontFactory.CreateFont("C:\\Users\\Таня\\Desktop\\Работа\\XmlParserConsole\\XmlParserConsole\\tnr.ttf", PdfEncodings.IDENTITY_H);
            PdfWriter writer = new PdfWriter("D:\\demo.pdf");
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);
            Paragraph header = new Paragraph("Управление федеральной службы государственной регистрации, кадастра и картографии.")
               .SetTextAlignment(TextAlignment.CENTER).SetFont(font)
               .SetFontSize(10);

            Table table = new Table(5, false);
            Cell cell11 = new Cell(1, 5)
               .SetTextAlignment(TextAlignment.CENTER)
               .Add(header);
            Cell cell21 = new Cell(1, 3)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph("Произведена государственная регистрация"));
            Cell cell22 = new Cell(1, 2)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph(deals[Content.RegistrationKind].ToString()));
            
            Cell cell31 = new Cell(1, 3)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph("Дата регистрации"));
            Cell cell32 = new Cell(1, 2)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph(Content.RegistrationDate));

            Cell cell41 = new Cell(1, 3)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph("Номер регистрации"));
            Cell cell42 = new Cell(1, 2)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph(Content.RegistrationNumber));

            Cell cell51 = new Cell(1, 3)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph("Государственный регистратор Прав"));
            Cell cell52 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph("____________\n(подпись)"));
            Cell cell53 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFont(font)
               .SetFontSize(10)
               .Add(new Paragraph(Content.FIO+"\n(Ф.И.О)"));

            //Cell cell51 = new Cell(1, 1)
            //   .SetTextAlignment(TextAlignment.CENTER)
            //   .Add(new Paragraph("Sacramento"));

            table.AddCell(cell11);
            table.AddCell(cell21);
            table.AddCell(cell22);
            table.AddCell(cell31);
            table.AddCell(cell32);
            table.AddCell(cell41);
            table.AddCell(cell42);
            table.AddCell(cell51);
            table.AddCell(cell52);
            table.AddCell(cell53);
            document.Add(table);

            Table signTable = new Table(1, false);
            signTable.SetWidth(200);
            Color purpleColor = new DeviceRgb(180, 3, 200);  //Create Darkgray color
            Border b1 = new DashedBorder(purpleColor, 0);

            Cell cell1 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFont(font)
               //.SetBorder(b1)
               .SetFontSize(8)
               .SetFontColor(purpleColor)
               .Add(new Paragraph("Документ подписан\n усиленной квалифицированной\n электронной подписью"));
            Cell cell2 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(7)
               //.SetBorder(b1)
               .SetFontColor(purpleColor)
               .Add(new Paragraph("Серийный номер: "+Content.SerialNumber));
            Cell cell3 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(7)
               //.SetBorder(b1)
               .SetFontColor(purpleColor)
               .Add(new Paragraph("Владелец: "+Content.FIO+". Организация: "+Content.Organisation+". ОГРН: "+
               Content.OGRN+". ИНН: "+Content.INN+". СНИЛС: "+Content.SNILS+". Email: "+Content.Email+". Адрес: "+Content.Address));
            Cell cell4 = new Cell(1,1 )
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFont(font)
               .SetFontSize(7)
               //.SetBorder(b1)
               .SetFontColor(purpleColor)
               .Add(new Paragraph("Действителен: с "+Content.NotBefore+" по "+Content.NotAfter));

            signTable.AddCell(cell1);
            signTable.AddCell(cell2);
            signTable.AddCell(cell3);
            signTable.AddCell(cell4);


            document.Add(new Paragraph("\n\n\n"));
            document.Add(signTable);
            document.Close();


        }
    }
}

