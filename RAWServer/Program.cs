using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RAWServer.Models;

namespace RAWServer
{
    class Program
    {
        static void Main(string[] args)
        {

            var port = 5000;
            var ip = IPAddress.Parse("127.0.0.1");


            var server = new TcpListener(ip, port);

            server.Start();
            Console.WriteLine("server started...");


            

            while(true)
            {
                var client = server.AcceptTcpClient();

                    Task.Run(() => {
                        Console.WriteLine("client connected!");
                        var strm = client.GetStream();

                        var buffer = new byte[client.ReceiveBufferSize];

                        var reqContent = strm.Read(buffer, 0, buffer.Length);

                        var request = Encoding.UTF8.GetString(buffer, 0, reqContent);

                        strm.Close();
                    });


            }







           




        }



        

        static RDJTPRequest ParseRequest(string content){


            return new RDJTPRequest();
        }


        static RDJTPResponse HandleDelete(){
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleCreate(){
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleUpdate(){
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleRead()
        {
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleEcho()
        {
            return new RDJTPResponse();
        }
    }
}
