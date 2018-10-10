using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RAWServer.Models;

namespace RAWServer
{
    class Program
    {
        static void Main(string[] args)
        {

            // init
            var cat1 = new Category() { Cid = 1, Name = "Beverages" };
            var cat2 = new Category() { Cid = 1, Name = "Condiments" };
            var cat3 = new Category() { Cid = 1, Name = "Confections" };

            var categories = new List<Category>() { cat1, cat2, cat3 };



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

                        var rdjtpReq = ParseRequest(request);
                        

                        switch (rdjtpReq.Method)
                        {
                            case RDJTPMethod.create:
                                HandleCreate(rdjtpReq);
                                break;
                            case RDJTPMethod.update:
                                HandleUpdate(rdjtpReq);
                                break;
                            case RDJTPMethod.delete:
                                HandleDelete(rdjtpReq);
                                break;
                            case RDJTPMethod.echo:
                                HandleEcho(rdjtpReq);
                                break;
                            case RDJTPMethod.read:
                                HandleRead(rdjtpReq);
                                break;
                            default:
                                HandleException(RDJTPStatus.Not_Found);
                                break;
                        }




                        strm.Close();
                    });

            }

        }



        

        static RDJTPRequest ParseRequest(string content){

            var request = JsonConvert.DeserializeObject<RDJTPRequest>(content);

            return request;
        }


        static RDJTPResponse HandleDelete(RDJTPRequest req){
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleCreate(RDJTPRequest req)
        {
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleUpdate(RDJTPRequest req)
        {
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleRead(RDJTPRequest req)
        {
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleEcho(RDJTPRequest req)
        {
            return new RDJTPResponse();
        }

        static RDJTPResponse HandleException(RDJTPStatus state)
        {
            return new RDJTPResponse();
        }
    }
}
