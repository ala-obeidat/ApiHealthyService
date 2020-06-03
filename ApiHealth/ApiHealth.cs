using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ApiHealth
{
    public partial class ApiHealth : ServiceBase
    {
        private static bool serviceWorking = false;
        private readonly TimeSpan interval = new TimeSpan(0, Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]), 0);

        private readonly string[] apiUrlArray = ConfigurationManager.AppSettings["URL"].Split(';');

        private readonly string errorLogPath = ConfigurationManager.AppSettings["LogPath"];

        private readonly MailAddress emailFrom = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"]);
        private readonly string emailPassword = ConfigurationManager.AppSettings["EmailPassword"];

        private readonly string[] emailTos = ConfigurationManager.AppSettings["EmailTo"].Split(';');
        private readonly string emailSMTP = ConfigurationManager.AppSettings["EmailSMTP"];
        private readonly int emailPort = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);

        public ApiHealth()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
           
            StartingService();
        }

        protected override void OnStop()
        {
            serviceWorking = false;
        }
        public void StartingService()
        {
            serviceWorking = true;
            Task.Run(async () =>
            {
                while (serviceWorking)
                {
                    try
                    {
                        await CallApiWithManager();
                    }
                    catch (Exception e)
                    {
                        HandleIException(e);
                    }
                    finally
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        await Task.Delay(interval);
                    }

                }
            });
        }

        private void HandleIException(Exception e)
        {
            List<string> messages = new List<string>();
            messages.Add(DateTime.Now.ToString("HH:mm:ss:ffff"));
            Exception temp = e;
            do
            {
                messages.Add(temp.Message);
                temp = temp.InnerException;

            } while (temp != null);
            try
            {
                if (!Directory.Exists(errorLogPath))
                {
                    Directory.CreateDirectory(errorLogPath);
                }
                string nowFileName = Path.Combine(errorLogPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm") + ".txt");
                File.AppendAllLines(nowFileName, messages);
            }
            catch { }
        }

        private async Task CallApiWithManager()
        {
            Dictionary<string, string> urlErrorReponse = new Dictionary<string, string>();
            for (int i = 0; i < apiUrlArray.Length; i++)
            {
                try
                {
                    var request = System.Net.WebRequest.Create(apiUrlArray[i] + "?key=UEtTXCszXTQhYg1jXWMzNVxk2bgw2JE22aY42YxjUjXZtzTZvGXZsjPZrjHZpDnZjGQ");
                    request.Headers.Add("ApplicationKey", "UEtTXCszXTQhYg1jXWMzNVxk2bgw2JE22aY42YxjUjXZtzTZvGXZsjPZrjHZpDnZjGQ=");
                    request.Headers.Add("ClientId", "e4dd40062e484d45ab6f5c3661518c9bf17c178380364e029c9b901edd4ec1c7");
                    request.Headers.Add("SecretKey", "sdkfjskleydvwgvtdv3672734773kllsdklfkdjfkjerqqqq0007sdbhshfhsdsqjkdfjgfgierjgioerigierjgijirjijasdasjd34534534fj34f834falsdadlaks__dkasd");
                    // call refrech epi as 'GET'.
                    using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            urlErrorReponse.Add(apiUrlArray[i], response.StatusCode.ToString());
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    urlErrorReponse.Add(apiUrlArray[i], e.Message);
                    HandleIException(e);
                }
                finally
                {
                    await Task.Delay(200);
                }
            }
            if (urlErrorReponse.Any())
            {
                await SendEmail(urlErrorReponse);
            }
        }

        private async Task SendEmail(Dictionary<string, string> urlErrorReponse)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = emailFrom;
                foreach (var address in emailTos)
                {
                    mail.To.Add(address);
                }
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                using (SmtpClient client = new SmtpClient(emailSMTP, emailPort))
                {
                    client.EnableSsl = emailPort == 587;
                    client.UseDefaultCredentials = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Credentials = new System.Net.NetworkCredential(emailFrom.Address, emailPassword);
                    mail.Subject = "Health Error";
                    foreach (var item in urlErrorReponse)
                    {
                        mail.Body = $"Api ({item.Key}) reponse is: {item.Value}";
                        client.Send(mail);
                        await Task.Delay(200);
                    }
                }
                
            }
        }

    }
}
