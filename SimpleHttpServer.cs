using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace HttpRemote
{
    public class TestMain
    {
        public static int Main(String[] args)
        {
            HttpServer httpServer;
            if (args.GetLength(0) > 0)
            {
                httpServer = new MyHttpServer(Convert.ToInt16(args[0]));
            }
            else
            {
                httpServer = new MyHttpServer(80);
            }

            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
            return 0;
        }
    }


    public abstract class HttpServer 
    {
        protected int port;
        TcpListener listener;
        bool is_active = true;
       
        public HttpServer(int port) 
        {
            this.port = port;
        }

        public void listen() 
        {
            listener = new TcpListener(port);
            listener.Start();
            while (is_active)
            {
                TcpClient s = listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.process));

                //System.Console.WriteLine("starting new thread...");
                thread.Start();
                Thread.Sleep(5);
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }



    public class MyHttpServer : HttpServer 
    {
        public MyHttpServer(int port) : base(port) 
        {
            //
        }

        public override void handleGETRequest (HttpProcessor p)
		{
            Console.WriteLine("request: {0}", p.http_url);

			if (p.http_url.EndsWith(".png")) 
            {
                Stream fs = File.Open("../../www" + p.http_url, FileMode.Open);
                p.writeImage("image/png", fs.Length);
                p.outputStream.BaseStream.Flush();
				fs.CopyTo(p.outputStream.BaseStream);
				p.outputStream.BaseStream.Flush();
                fs.Close();
			}
            else if (p.http_url.EndsWith(".htm") || p.http_url.EndsWith(".js"))
            {
                StringBuilder sb = new StringBuilder();
                using (StreamReader sr = new StreamReader("../../www" + p.http_url))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        sb.AppendLine(line);
                    }
                }
                string s = sb.ToString();
                p.writeSuccess();
                p.outputStream.WriteLine(s+"\r\n");
				p.outputStream.BaseStream.Flush();
			}
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData) 
        {
            //Console.WriteLine("POST request: {0}", p.http_url);
            string data = inputData.ReadToEnd();
            //Console.WriteLine("<post>{0}</post>", data);
            p.writeSuccess();
            parsePost(data);
        }

        public void parsePost(string data)
        {
            if (data.StartsWith("button")) 
            {
                int n = Convert.ToInt32(data.Split(' ')[1]);
                int v = Convert.ToInt32(data.Split(' ')[2]);

                Console.WriteLine("click thing " + n + " " + v);
                WinAPI.clickThing("Pedals", n, v);
            }
            else if (data.StartsWith("swipe"))
            {
                int x = Convert.ToInt32(data.Split(' ')[1]);
                int y = Convert.ToInt32(data.Split(' ')[2]);
                WinAPI.SetCursorPos(x, y);
                //Mouse.MoveTo(x, y);
            }
            else if (data.StartsWith("tap")) 
            {
                WinAPI.POINT p;
                WinAPI.GetCursorPos(out p);
                WinAPI.LeftMouseClick(p.X, p.Y);
            }
        }

    }
}



