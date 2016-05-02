using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NDesk.Options;

namespace dpdeploy
{
    class Program
    {
        static internal OptionSet Options { get; private set; }
        static internal string Hostname { get; private set; }
        static internal string Username { get; private set; }
        static internal string Password { get; private set; }
        static internal NetworkCredential Credentials { get; private set; }

        /// <summary>
        /// Show the Help string
        /// </summary>
        static void ShowHelp()
        {
            Console.WriteLine("Usage is: dpdeploy <options> <verb> <arguments to the verb>");
            Console.WriteLine("options are :-");
            Options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine("verbs are :-");
            Console.WriteLine("\tinfo         : get info from target device");
            Console.WriteLine("\tlist-apps    : get list of installed apps from target device");
            Console.WriteLine("\tinstall      : install appx to the target device");
            Console.WriteLine("\tuninstall    : uninstall app from the target device");
            Console.WriteLine("\tstart        : start app on the target device");
            Console.WriteLine("\tstop         : stop app on the target device");
            Console.WriteLine("\trestart-dev  : restart the target device");
            Console.WriteLine("\tshutdown-dev : shutdown the target device");
            Environment.Exit(0);
        } // end ShowHelp

        /// <summary>
        /// Main function
        /// </summary>
        /// <param name="_args">command line arguments</param>
        static void Main(string[] _args)
        {
            Options = new OptionSet()
                .Add("?|help", "display help usage", v => ShowHelp())
                .Add("h=|hostname=", "host to target", (string v) => Hostname = v)
                .Add("u=|username=", "username for target", (string v) => Username = v)
                .Add("p=|password=", "password for target", (string v) => Password = v)
                ;


            List<string> args = Options.Parse(_args);
            Credentials = new NetworkCredential(Username, Password);
            string verb = (args.Count > 0) ? args[0] : "info";
            switch (verb)
            {
                case "info":
                    {
                        string uri = string.Format("http://{0}/api/os/info", Hostname);
                        string responseString = HttpGetExpect200(uri);
                        Console.WriteLine(responseString);
                    } // end block
                    break;

                case "list-apps":
                    {
                        string uri = string.Format("http://{0}/api/app/packagemanager/packages", Hostname);
                        string responseString = HttpGetExpect200(uri);
                        Console.WriteLine(responseString);
                    } // end block
                    break;

                case "install":
                    {
                        bool fShowHelp = false;
                        if ((args.Count == 2) && (args[1] == "help"))
                        {
                            fShowHelp = true;
                        } // end if
                        else
                            if (args.Count == 2)
                            {
                                string uri = string.Format("http://{0}/api/app/packagemanager/package", Hostname);
                                Dictionary<string, string> aa = new Dictionary<string, string>();
                                aa.Add("package", Path.GetFileName(args[1]));
                                List<string> filesToSend = new List<string>(args.GetRange(1, args.Count - 1));
                                string responseString = InstallPackage( uri, aa, filesToSend );
                                Console.WriteLine(responseString);
                                fShowHelp = false;
                            } // end if
                            else
                            {
                                fShowHelp = true;
                            } // end else

                        if (fShowHelp)
                        {
                            Console.WriteLine("install <appx-file-to-install> [<certificate file>] [<other-dependency-files>]*");
                        } // end else
                    } // end block
                    break;

                case "uninstall":
                    {
                        bool fShowHelp = false;
                        if ((args.Count == 2) && (args[1] == "help"))
                        {
                            fShowHelp = true;
                        } // end if
                        else
                            if (args.Count == 2)
                            {
                                string uri = string.Format("http://{0}/api/app/packagemanager/package", Hostname);
                                Dictionary<string, string> aa = new Dictionary<string, string>();
                                aa.Add("package", Convert.ToBase64String(Encoding.UTF8.GetBytes(args[1] /*"Project1-native-gmx_1.0.0.0_arm__ry4dcfwkadf3m"*/)));
                                string responseString = HttpDeleteExpect200(uri, aa);
                                Console.WriteLine(responseString);
                                fShowHelp = false;
                            } // end if
                            else
                            {
                                fShowHelp = true;
                            } // end else

                        if (fShowHelp)
                        {
                            Console.WriteLine("install <package-full-name>");
                        } // end else
                    } // end block
                    break;

                case "start":
                    {
                        bool fShowHelp = false;
                        if ((args.Count == 2) && (args[1] == "help"))
                        {
                            fShowHelp = true;
                        } // end if
                        else
                            if (args.Count == 3)
                            {
                                string uri = string.Format("http://{0}/api/taskmanager/app", Hostname);
                                Dictionary<string, string> aa = new Dictionary<string, string>();
                                aa.Add("appid", Convert.ToBase64String(Encoding.UTF8.GetBytes(args[1] /*"Project1-native-gmx_ry4dcfwkadf3m!App"*/)));
                                aa.Add("package", Convert.ToBase64String(Encoding.UTF8.GetBytes(args[2] /*"Project1-native-gmx_1.0.0.0_arm__ry4dcfwkadf3m"*/)));
                                string responseString = HttpPostExpect200(uri, aa);
                                Console.WriteLine(responseString);
                                fShowHelp = false;
                            } // end if
                            else
                            {
                                fShowHelp = true;
                            } // end else

                        if (fShowHelp)
                        {
                            Console.WriteLine("start <appid (PRAID)> <package-full-name>");
                        } // end else
                    } // end block
                    break;

                case "stop":
                    {
                        bool fShowHelp = false;
                        if ((args.Count == 2) && (args[1] == "help"))
                        {
                            fShowHelp = true;
                        } // end if
                        else
                            if (args.Count >= 2)
                            {
                                string uri = string.Format("http://{0}/api/taskmanager/app", Hostname);
                                Dictionary<string, string> aa = new Dictionary<string, string>();
                                aa.Add("forcestop", Convert.ToBase64String(Encoding.UTF8.GetBytes((args.Count >= 3) ? args[2] : "yes"  /*"Project1-native-gmx_ry4dcfwkadf3m!App"*/)));
                                aa.Add("package", Convert.ToBase64String(Encoding.UTF8.GetBytes(args[1] /*"Project1-native-gmx_1.0.0.0_arm__ry4dcfwkadf3m"*/)));
                                string responseString = HttpDeleteExpect200(uri, aa);
                                Console.WriteLine(responseString);
                                fShowHelp = false;
                            } // end if
                            else
                            {
                                fShowHelp = true;
                            } // end else

                        if (fShowHelp)
                        {
                            Console.WriteLine("stop <package-full-name> <forcestop (yes/no) defaults to yes>");
                        } // end else
                    } // end block
                    break;

                case "restart-dev":
                    {
                        string uri = string.Format("http://{0}/api/control/restart", Hostname);
                        string responseString = HttpPostExpect200(uri, null);
                        Console.WriteLine(responseString);
                    } // end block
                    break;

                case "shutdown-dev":
                    {
                        string uri = string.Format("http://{0}/api/control/shutdown", Hostname);
                        string responseString = HttpPostExpect200(uri, null);
                        Console.WriteLine(responseString);
                    } // end block
                    break;
            } // end switch
        }

        // =================================================================================================================
        /// <summary>
        /// Send a GET http request to the server
        /// </summary>
        /// <param name="_uri">URI of the server</param>
        /// <returns>response string from the server</returns>
        private static string HttpGetExpect200(string _uri)
        {
            int size = 10 * 1024;
            MemoryStream ms = new MemoryStream();
            string responseString = string.Empty;
            try
            {
                HttpWebRequest request = WebRequest.Create(_uri) as HttpWebRequest;
                CredentialCache mycache = new CredentialCache();
                mycache.Add(request.RequestUri, "Basic", Credentials);
                request.Credentials = mycache;
                request.Method = WebRequestMethods.Http.Get;
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[size];
                    int bytesRead = responseStream.Read(buffer, 0, size);
                    while (bytesRead > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                        bytesRead = responseStream.Read(buffer, 0, size);
                    } // end while
                } // end using
                responseString = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (WebException _ex)
            {
                responseString = _ex.ToString();
            } // end catch

            return responseString;
        }

        // =================================================================================================================
        /// <summary>
        /// Send a POST http request to the server
        /// </summary>
        /// <param name="_uri">URI of the server</param>
        /// <param name="_parameters">parameters to be added to the URI</param>
        /// <param name="_payload">payload to send to the server</param>
        /// <returns>response string from the server</returns>
        private static string HttpPostExpect200(string _uri, Dictionary<string, string> _header, byte[] _payload = null)
        {
            int size = 10 * 1024;
            MemoryStream ms = new MemoryStream();
            string responseString = string.Empty;
            try
            {
                StringBuilder postData = new StringBuilder();
                if (_header != null)
                {
                    int count = 0;
                    foreach (KeyValuePair<string, string> kvp in _header)
                    {
                        if (count > 0)
                        {
                            postData.Append("&");
                        } // en dif
                        postData.AppendFormat("{0}={1}", HttpUtility.UrlEncode(kvp.Key), HttpUtility.UrlEncode(kvp.Value));
                        ++count;
                    } // end foreach
                } // end if
                _uri = _uri + "?" + postData.ToString();
                HttpWebRequest request = WebRequest.Create(_uri) as HttpWebRequest;
                CredentialCache mycache = new CredentialCache();
                mycache.Add(request.RequestUri, "Basic", Credentials);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Credentials = mycache;
                request.Method = WebRequestMethods.Http.Post;
                byte[] postDataArray = (_payload != null) ? _payload : new byte[0]; // Encoding.UTF8.GetBytes(postData.ToString());
                request.ContentLength = postDataArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(postDataArray, 0, postDataArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[size];
                    int bytesRead = responseStream.Read(buffer, 0, size);
                    while (bytesRead > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                        bytesRead = responseStream.Read(buffer, 0, size);
                    } // end while
                } // end using
                responseString = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (WebException _ex)
            {
                responseString = _ex.ToString();
            } // end catch

            return responseString;
        }

        // =================================================================================================================
        /// <summary>
        /// Send a DELETE http request to the server
        /// </summary>
        /// <param name="_uri">URI of the server</param>
        /// <param name="_parameters">parameters to be added to the URI</param>
        /// <returns>response string from the server</returns>
        private static string HttpDeleteExpect200(string _uri, Dictionary<string, string> _parameters)
        {
            int size = 10 * 1024;
            MemoryStream ms = new MemoryStream();
            string responseString = string.Empty;
            try
            {
                StringBuilder postData = new StringBuilder();
                if (_parameters != null)
                {
                    int count = 0;
                    foreach (KeyValuePair<string, string> kvp in _parameters)
                    {
                        if (count > 0)
                        {
                            postData.Append("&");
                        } // en dif
                        postData.AppendFormat("{0}={1}", HttpUtility.UrlEncode(kvp.Key), HttpUtility.UrlEncode(kvp.Value));
                        ++count;
                    } // end foreach
                } // end if
                _uri = _uri + "?" + postData.ToString();
                HttpWebRequest request = WebRequest.Create(_uri) as HttpWebRequest;
                CredentialCache mycache = new CredentialCache();
                mycache.Add(request.RequestUri, "Basic", Credentials);
                request.Credentials = mycache;
                request.Method = "DELETE";
                byte[] postDataArray = new byte[0]; // Encoding.UTF8.GetBytes(postData.ToString());
                request.ContentLength = postDataArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(postDataArray, 0, postDataArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[size];
                    int bytesRead = responseStream.Read(buffer, 0, size);
                    while (bytesRead > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                        bytesRead = responseStream.Read(buffer, 0, size);
                    } // end while
                } // end using
                responseString = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (WebException _ex)
            {
                responseString = _ex.ToString();
            } // end catch

            return responseString;
        }

        // =================================================================================================================
        /// <summary>
        ///     Install the package
        /// </summary>
        /// <param name="_uri">URI to send to</param>
        /// <param name="_parameters">parameters to be added to the uri</param>
        /// <param name="_files">list of filenames to send</param>
        /// <returns>response string</returns>
        private static string InstallPackage( string _uri, Dictionary<string,string> _parameters, List<string> _files )
        {
            string originalUri=_uri;
            StringBuilder postData = new StringBuilder();
            if (_parameters != null)
            {
                int count = 0;
                foreach (KeyValuePair<string, string> kvp in _parameters)
                {
                    if (count > 0)
                    {
                        postData.Append("&");
                    } // en dif
                    postData.AppendFormat("{0}={1}", HttpUtility.UrlEncode(kvp.Key), HttpUtility.UrlEncode(kvp.Value));
                    ++count;
                } // end foreach
            } // end if
            _uri = _uri + "?" + postData.ToString();
            string responseString = "";

            try
            {
                using (var client = new HttpClient())
                {

                    client.DefaultRequestHeaders.ExpectContinue = false;
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Username, Password))));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
                    using (var content = new MultipartFormDataContent())
                    {
                        foreach (string f in _files)
                        {
                            var streamContent = new StreamContent(new StreamReader(f).BaseStream, 1024);
                            FileInfo fI = new FileInfo(f);
                            streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { 
                                Name = "\"" + fI.Name + "\"", 
                                FileName = "\"" + fI.Name + "\"" 
                            };
                            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                            content.Add(streamContent);
                        } // end foreach

                        // this is the magic bit of code to fix sending the multipart form data properly...
                        var boundaryValue = content.Headers.ContentType.Parameters.FirstOrDefault(p => p.Name == "boundary");
                        boundaryValue.Value = boundaryValue.Value.Replace("\"", String.Empty); 

                        Task<HttpResponseMessage> tmessage = client.PostAsync( _uri, content);
                        tmessage.Wait();
                        HttpResponseMessage message = tmessage.Result;
                        message.EnsureSuccessStatusCode();
                        var input = message.Content.ReadAsStringAsync();
                        responseString = input.Result;
                    } // end using
                } // end using
            } // end try
            catch (Exception _ex)
            {
                responseString = _ex.ToString();
            }

            return responseString;
        } // end InstallPackage


    }
}
