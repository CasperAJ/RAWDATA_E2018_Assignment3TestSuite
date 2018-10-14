using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RAWServer.Models;


/*

    Mikkel Pedersen
    Mert Ince
    Kadir Yorulmaz
    Casper Jørgensen

*/
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

            // init - state
            var cat1 = new Category() { cid = 1, name = "Beverages" };
            var cat2 = new Category() { cid = 2, name = "Condiments" };
            var cat3 = new Category() { cid = 3, name = "Confections" };

            var categories = new List<Category>() { cat1, cat2, cat3 };

            var typesOfMethods = new List<string>() { "create", "update", "delete", "read", "echo" };

            while (true)
            {
                var client = server.AcceptTcpClient();

                Task.Run(() =>
                {


                    var expmsg = "";

                    Console.WriteLine("client connected!");
                    var strm = client.GetStream();

                    var buffer = new byte[client.ReceiveBufferSize];

                    var reqContent = strm.Read(buffer, 0, buffer.Length);

                    var request = Encoding.UTF8.GetString(buffer, 0, reqContent);




                    expmsg = CheckMissing(request);



                    if (!string.IsNullOrEmpty(expmsg))
                    {
                        Respond(strm, HandleException(RDJTPStatus.Bad_Request, expmsg));
                        strm.Close();
                        return;
                    }



                    // parsing request
                    var rdjtpReq = ParseRequest(request);


                    expmsg = CheckDate(rdjtpReq);

                    if (!string.IsNullOrEmpty(expmsg))
                    {
                        Respond(strm, HandleException(RDJTPStatus.Bad_Request, expmsg));
                        strm.Close();
                        return;
                    }




                    if (!CheckRequest(rdjtpReq))
                    {
                        var msg = CheckResource(rdjtpReq);
                        Respond(strm, HandleException(RDJTPStatus.Bad_Request, $"missing resource {msg}"));
                        strm.Close();
                        return;
                    }



                    if (!typesOfMethods.Contains(rdjtpReq.Method))
                    {
                        Respond(strm, HandleException(RDJTPStatus.Bad_Request, "illegal method"));
                        strm.Close();
                        return;
                    }

                    ////////////



                    if (!CheckRoute(rdjtpReq))
                    {

                        var expresponse = HandleException(RDJTPStatus.Not_Found);

                        var path = rdjtpReq.Path.Split("/");

                        if (path.Length < 3)
                        {
                            expresponse = HandleException(RDJTPStatus.Bad_Request);
                            Respond(strm, expresponse);
                            strm.Close();
                            return;
                        }

                        if (path[1] != "api" || path[2] != "categories")
                        {
                            expresponse = HandleException(RDJTPStatus.Bad_Request);
                            Respond(strm, expresponse);
                            strm.Close();
                            return;
                        }

                        if (rdjtpReq.Body == null)
                        {
                            expresponse.Status += " missing body";
                        }

                        Respond(strm, expresponse);
                        strm.Close();
                        return;
                    }



                    // success up until now
                    var response = new RDJTPResponse();
                    switch (rdjtpReq.Method)
                    {
                        case "create":
                            response = HandleCreate(rdjtpReq, categories);
                            break;
                        case "update":
                            response = HandleUpdate(rdjtpReq, categories);
                            break;
                        case "delete":
                            response = HandleDelete(rdjtpReq, categories);
                            break;
                        case "echo":
                            response = HandleEcho(rdjtpReq);
                            break;
                        case "read":
                            response = HandleRead(rdjtpReq, categories);
                            break;
                        default:
                            response = HandleException(RDJTPStatus.Error);
                            break;
                    }
                    Respond(strm, response);
                    strm.Close();
                });

            }

        }

        static string CheckDate(RDJTPRequest req){
            var msg = "";
            if (string.IsNullOrEmpty(req.Date))
            {
                msg = "illegal date";
            }

            
            var tmpdate = new double();
            if (!double.TryParse(req.Date, out tmpdate))
            {
                msg = "illegal date";
            }

            return msg;
        }

        static string CheckMissing(string content){
            var msg = "";

            if (!content.Contains("date"))
            {

                msg = "missing date";

            }

            if (!content.Contains("method"))
            {
                msg += " missing method";


            }

            return msg;
        }


        static bool CheckRequest(RDJTPRequest req)
        {




            if (req.Method == "create" || req.Method == "update" || req.Method == "echo")
            {
                if (req.Body == null) return false;
            }

            if (req.Method != "echo")
            {

                if (req.Path == null) return false;
            }


            return true;
        }


        static string CheckResource(RDJTPRequest req)
        {
            var msg = "";

            if (req.Method == "create" || req.Method == "update" || req.Method == "echo")
            {
                if (req.Body == null) msg += "missing body";
            }

            if (req.Method != "echo")
            {

                if (req.Path == null) msg += "missing path";
            }

            return msg;
        }


        static void Respond(NetworkStream strm, RDJTPResponse resp)
        {
            var jsonresponse = JsonConvert.SerializeObject(resp);
            var payload = Encoding.UTF8.GetBytes(jsonresponse);

            strm.Write(payload, 0, payload.Length);
        }


        static RDJTPRequest ParseRequest(string content)
        {

            var request = new RDJTPRequest();

            request = JsonConvert.DeserializeObject<RDJTPRequest>(content);

            return request;
        }


        static bool CheckRoute(RDJTPRequest req)
        {
            if (req.Method == "echo")
            {
                return true;
            }
            
            if (!req.Path.Contains("/")) return false;


            var path = req.Path.Split("/");



            if (path[1] != "api") return false;


            if (path[2] != "categories") return false; 



            return true;

        }




        static RDJTPResponse HandleDelete(RDJTPRequest req, List<Category> categories)
        {

            var path = req.Path.Split("/");

            if (path.Length < 4) return HandleException(RDJTPStatus.Bad_Request);

            var elm = categories.Find(x => x.cid == Convert.ToInt32(path[3]));


            if (elm == null) return HandleException(RDJTPStatus.Not_Found);

            categories.Remove(elm);

            return new RDJTPResponse() { Status = "1 OK" };
        }

        static RDJTPResponse HandleCreate(RDJTPRequest req, List<Category> categories)
        {


            var newElement = JsonConvert.DeserializeObject<Category>(req.Body);

            if (string.IsNullOrEmpty(newElement.name))
            {
                return HandleException(RDJTPStatus.Bad_Request);
            }

            var listlength = categories.Count;

            newElement.cid = listlength + 1;

            categories.Add(newElement);

            var body = JsonConvert.SerializeObject(newElement);

            return new RDJTPResponse() { Status = "2 Created", Body = body };
        }

        static RDJTPResponse HandleUpdate(RDJTPRequest req, List<Category> categories)
        {

            var path = req.Path.Split("/");
            if (path.Length < 4) return HandleException(RDJTPStatus.Bad_Request);


            var newElement = new Category();



            try
            {
                newElement = JsonConvert.DeserializeObject<Category>(req.Body);
            }
            catch (Exception)
            {
                return new RDJTPResponse() { Status = "4 Bad Request Illegal Body" };
            }



            var elm = categories.Find(x => x.cid == Convert.ToInt32(path[3]));

            if (elm == null)
            {
                return HandleException(RDJTPStatus.Not_Found);
            }

            categories[categories.IndexOf(elm)] = newElement;

            return new RDJTPResponse() { Status = "3 Updated" };
        }

        static RDJTPResponse HandleRead(RDJTPRequest req, List<Category> categories)
        {
            var response = new RDJTPResponse();

            var path = req.Path.Split("/");

            if (path.Length < 4)
            {
                response.Status = "1 Ok";
                response.Body = JsonConvert.SerializeObject(categories);
                return response;
            }
            else
            {

                response.Status = "1 Ok";
                int cid;

                if (!int.TryParse(path[3], out cid)) return HandleException(RDJTPStatus.Bad_Request);

                var element = categories.Find(x => x.cid == cid);

                if (element == null) return HandleException(RDJTPStatus.Not_Found);

                response.Body = JsonConvert.SerializeObject(element);
                return response;
            }


        }

        static RDJTPResponse HandleEcho(RDJTPRequest req)
        {
            var response = new RDJTPResponse() { Status = "1 OK", Body = req.Body };
            return response;
        }

        static RDJTPResponse HandleException(RDJTPStatus state, string msg = "")
        {
            var response = new RDJTPResponse();
            switch (state)
            {
                case RDJTPStatus.Not_Found:
                    response.Status = "5 Not Found";
                    break;
                case RDJTPStatus.Bad_Request:
                    if (msg != "")
                    {

                        response.Status = $"4 Bad Request {msg}";
                    }
                    else
                    {
                        response.Status = "4 Bad Request";
                    }

                    break;
                case RDJTPStatus.Error:
                    response.Status = "6 Error";
                    break;
                default:
                    response.Status = "6 Error";
                    break;
            }

            return response;
        }
    }
}
