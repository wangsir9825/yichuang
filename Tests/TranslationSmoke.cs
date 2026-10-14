using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class TranslationSmoke
{
    class LoopbackRequest : IWebRequestCreate {
        public WebRequest Create(Uri uri) { return WebRequest.Create(new Uri("http://127.0.0.1:"+uri.Port+uri.PathAndQuery)); }
    }
    static int Main(string[] args) {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(Timer watchdog=new Timer(s=>Environment.Exit(124),null,20000,Timeout.Infinite)) {
            try {
                ServicePointManager.Expect100Continue=false;ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
                WebRequest.DefaultWebProxy=null;
                WebRequest.RegisterPrefix("https://127.0.0.1:",new LoopbackRequest());
                ServicePointManager.ServerCertificateValidationCallback=(s,c,ch,e)=>true; // Test process only, self-signed loopback server.
                TcpListener listener=new TcpListener(IPAddress.Loopback,0);listener.Start();
                int port=((IPEndPoint)listener.LocalEndpoint).Port;
                string requestBody=null,headers=null;
                using(RSA rsa=RSA.Create(2048)) {
                    CertificateRequest certRequest=new CertificateRequest("CN=localhost",rsa,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
                    using(X509Certificate2 generated=certRequest.CreateSelfSigned(DateTimeOffset.Now.AddMinutes(-1),DateTimeOffset.Now.AddMinutes(10)))
                    using(X509Certificate2 certificate=new X509Certificate2(generated.Export(X509ContentType.Pfx),"",X509KeyStorageFlags.EphemeralKeySet)) {
                        Task server=Task.Run(()=>{
                            using(TcpClient client=listener.AcceptTcpClient())using(NetworkStream stream=client.GetStream()) {
                                File.AppendAllText(Path.Combine(output,"progress.txt"),"Client accepted\n");
                                File.AppendAllText(Path.Combine(output,"progress.txt"),"TLS ready\n");
                                var raw=new MemoryStream();int current;
                                while((current=stream.ReadByte())>=0) {
                                    raw.WriteByte((byte)current);if(raw.Length>65536)throw new Exception("Header limit");
                                    byte[] bytes=raw.GetBuffer();int n=(int)raw.Length;
                                    if(n>=4&&bytes[n-4]==13&&bytes[n-3]==10&&bytes[n-2]==13&&bytes[n-1]==10)break;
                                }
                                headers=Encoding.ASCII.GetString(raw.ToArray());int length=0;
                                foreach(string line in headers.Split(new[]{"\r\n"},StringSplitOptions.None))if(line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase))length=int.Parse(line.Substring(15).Trim());
                                byte[] body=new byte[length];int offset=0;while(offset<length){int n=stream.Read(body,offset,length-offset);if(n==0)break;offset+=n;}
                                requestBody=Encoding.UTF8.GetString(body);
                                string response="data: {\"choices\":[{\"delta\":{\"content\":\"试用成功\"},\"finish_reason\":null}]}\n\ndata: {\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n";
                                byte[] payload=Encoding.UTF8.GetBytes(response);
                                byte[] head=Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/event-stream; charset=utf-8\r\nContent-Length: "+payload.Length+"\r\nConnection: close\r\n\r\n");
                                stream.Write(head,0,head.Length);stream.Write(payload,0,payload.Length);stream.Flush();
                            }
                        });
                        string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
                AppDomain.CurrentDomain.AssemblyResolve += (s,e) => {
                    string dll=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");
                    return File.Exists(dll)?Assembly.LoadFile(dll):null;
                };
                        Assembly assembly=Assembly.LoadFrom(exe);
                        MethodInfo translate=assembly.GetType("Xtranslator.ChatGPT").GetMethod("LoadSourceCode4",BindingFlags.Static|BindingFlags.Public,null,new[]{typeof(object),typeof(string),typeof(string),typeof(string)},null);
                        File.AppendAllText(Path.Combine(output,"progress.txt"),"Invoking translator\n");
                        string result=(string)translate.Invoke(null,new object[]{"Translate this test sentence.","127.0.0.1:"+port,"trial-test-key","mock-model"});
                        if(!server.Wait(5000))throw new Exception("Mock server did not finish");listener.Stop();
                        bool ok=result=="试用成功"&&requestBody.Contains("mock-model")&&requestBody.Contains("Translate this test sentence.")&&headers.Contains("Bearer trial-test-key");
                        File.WriteAllText(Path.Combine(output,"translation-test.txt"),"Endpoint=loopback HTTP mock only (test process URL factory override)\nPaid API calls=0\nResult="+result+"\nRequest model, text, authorization valid="+ok+"\n");
                        return ok?0:1;
                    }
                }
            }catch(Exception ex){File.WriteAllText(Path.Combine(output,"translation-test.txt"),ex.ToString());return 1;}
        }
    }
}
