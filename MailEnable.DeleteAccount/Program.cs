using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailEnable.DeleteAccount.MeAddressMapService;

using MailEnable.DeleteAccount.MeLoginService;
using MailEnable.DeleteAccount.MeMailBoxService;
using MailEnable.DeleteAccount.MePostOfficeService;

namespace MailEnable.DeleteAccount
{
    class Program
    {
        static void Main(string[] args)
        {

            MePostOfficeService.PostOfficeSoapClient client = new PostOfficeSoapClient("PostOfficeSoap");
            MeMailBoxService.MailboxSoapClient mailBoxClient = new MailboxSoapClient();
            MeAddressMapService.AddressMapSoapClient addressMapClient = new AddressMapSoapClient();
            MeLoginService.LoginSoapClient loginClient = new LoginSoapClient();
           
            Console.WriteLine("Dosya İsmini Yazın Dosya direk programın çalıştığı dizinde olmalıdır.");
            var fileName = Console.ReadLine();

            Console.WriteLine(@"Lütfen Root Folder Giriniz örn : D:\PostOffice ");
            var rootFolder = Console.ReadLine();



            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
    (se, cert, chain, sslerror) =>
    {
        return true;
    };

            if (fileName != string.Empty)
            {
                try
                {
                    string[] lines = System.IO.File.ReadAllLines(fileName);

                    foreach (string line in lines)
                    {

                        if(line != "")
                        {
                            //GetMailBoxes
                            Mailbox[] mailBoxes =null;

                            mailBoxClient.ListMailbox(ref mailBoxes, line.Trim(), 0, 100);
                            if (mailBoxes != null)
                            {
                                foreach (var mailbox in mailBoxes)
                                {
                                    long mbRemove = mailBoxClient.RemoveMailbox(line.Trim(), mailbox.MailboxName);
                                    if (mbRemove == 1)
                                    {

                                        AddressMap map = new AddressMap();
                                        map.Account = line.Trim();
                                        // [SMTP: test@360tour.com.tr][SF:360tour.com.tr/test] 360tour.com.tr 1

                                        map.DestinationAddress = "[SF:" + line.Trim() + "/" + mailbox.MailboxName + "]";
                                        map.SourceAddress = "";
                                        map.Scope = "";

                                        long mapStatus = addressMapClient.RemoveAddressMap(map.Account, map.SourceAddress, map.DestinationAddress, map.Scope, 1);

                                        long loginStatus = loginClient.RemoveLogin(mailbox.Postoffice,
                                               mailbox.MailboxName + "@" + line.Trim());
                                    }
                                }



                                long p = client.RemovePostoffice("", line.Trim());

                                if (p == 1)
                                {
                                    var path = rootFolder + "\\" + line.Trim();
                                    bool directoryExists = Directory.Exists(path);
                                    if (directoryExists)
                                    {
                                        Directory.Delete(path, true);
                                        Console.WriteLine("Folder : " + line.Trim() + " Silindi");
                                    }
                                    Console.WriteLine(line.Trim() + " Silindi");
                                }
                                else
                                {
                                    Console.WriteLine("\t" + line.Trim() + " Silinemedi !!!!");
                                }

                            }
                            else
                            {
                                Console.WriteLine("\t MAILBOX BULUNAMADI !!!!");

                                client.RemovePostoffice("", line.Trim());
                                Console.WriteLine("PostOffice : " + line.Trim() + " SİLİNDİ");
                                var path = rootFolder + "\\" + line.Trim();
                                bool directoryExists = Directory.Exists(path);
                                if (directoryExists)
                                {
                                    Directory.Delete(path, true);
                                    Console.WriteLine("FOLDER : " + line.Trim() + " SİLİNDİ");
                                }
                                Console.WriteLine("MAILBOX'LAR HARİÇ "+ line.Trim() + " SİLİNDİ");

                            }
                        }
                           



                    }
                  
                }
                catch (Exception ex)
                {

                    Console.WriteLine("\t" + ex.Message);
                }

            }
            else
            {

                Console.WriteLine("\t Dosya adı boş olamaz");
            }


            Console.WriteLine("\t <--------------------------- ERAVSE ---------------------->");
            Console.WriteLine("\t <--------------------------- ><)))O> ---------------------->");

            Console.ReadLine();

        }
    }
}
