using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Code.Scheduler.ClientUtils.Impl
{
    internal static class SOAPRequestHelper
    {
        public static Tuple<Uri, Binding> GetConfigurationBinding(string endpointName)
        {
            ClientSection clientSection = (WebConfigurationManager.GetSection("system.serviceModel/client") as ClientSection);
            ChannelEndpointElement endpoint = null;
            foreach (ChannelEndpointElement cee in clientSection.Endpoints)
            {
                if (cee.Name == endpointName)
                {
                    endpoint = cee;
                    break;
                }
            }

            Type t = Assembly.GetAssembly(typeof(Binding)).GetTypes().First(p => p.Name.ToLower() == endpoint.Binding.ToLower());
            if (t == null)
                throw new ArgumentException($"No binding type found {endpoint.Binding}");

            return new Tuple<Uri, Binding>(endpoint.Address, (Binding)Activator.CreateInstance(t, endpoint.BindingConfiguration));
        }
        public static string GetSOAPMessage<T>(Expression<Action<T>> call, Binding binding)
        {
            string _soapMessage = null;

            MethodCallExpression mc = call.Body as MethodCallExpression;
            if (mc == null)
                throw new ArgumentException("The expression must be ServiceContract method call");

            List<object> parameters = mc.Arguments.Select(p => Expression.Lambda(p).Compile().DynamicInvoke()).ToList();

            MethodInfo method = typeof(T).GetMethod(mc.Method.Name);
            Random rnd = new Random((int)DateTime.Now.ToFileTime());
            int port = rnd.Next(20000, 40000);

            TcpListener server = new TcpListener(IPAddress.Loopback, port);
            server.Start();
            try
            {
                Task tmp = Task.Factory.StartNew(() =>
                {
                    using (TcpClient client = server.AcceptTcpClient())
                    using (NetworkStream stream = client.GetStream())
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        StringBuilder b = new StringBuilder();
                        string x = null;
                        int cntLength = 0;
                        while (x != "")
                        {
                            x = sr.ReadLine();
                            Match m = Regex.Match(x, "Content-Length:\\s*(\\d+)");
                            if (m.Success)
                                cntLength = Convert.ToInt32(m.Groups[1].ToString());
                            b.AppendLine(x);
                        }

                        byte[] msg = new byte[cntLength];
                        for (int i = 0; i < cntLength; i++)
                        {
                            msg[i] = (byte)sr.Read();
                        }
                        b.Append(Encoding.UTF8.GetString(msg));

                        _soapMessage = b.ToString();
                    }
                });
                using (ChannelFactory<T> ch = new ChannelFactory<T>(binding))
                {
                    bool org = System.Net.ServicePointManager.Expect100Continue;
                    try
                    {
                        System.Net.ServicePointManager.Expect100Continue = false;
                        T svc = ch.CreateChannel(new EndpointAddress($"http://localhost:{port}"));
                        method.Invoke(svc, parameters.ToArray());
                    }
                    catch (Exception e) { } //serveper evitare il "coonection interrupted exception"
                    finally
                    {
                        System.Net.ServicePointManager.Expect100Continue = org;
                    }
                }

                tmp.Wait();
            }
            finally
            {
                server.Stop();
            }

            return _soapMessage;
        }

        public static string GetMangeldRequest(string originalSoapMessage, Uri adderss)
        {
            string[] lines = originalSoapMessage.Split('\n');
            lines[0] = Regex.Replace(lines[0], "(\\w+)(\\s+)(\\S+)(\\s+)(.*)", m =>
            $"{m.Groups[1].ToString()}{m.Groups[2].ToString()}{adderss.PathAndQuery}{m.Groups[4].ToString()}{m.Groups[5].ToString()}");
            for (int i = 1; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i], "Host:"))
                {
                    lines[i] = Regex.Replace(lines[i], "(Host:\\s*)(\\S+)(\\s*)", m => $"{m.Groups[1].ToString()}{adderss.Host}{m.Groups[3].ToString()}");
                    break;
                }
            }

            return string.Join("\n", lines);
        }
    }
}
