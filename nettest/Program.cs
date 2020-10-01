using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

namespace nettest
{
    class Program
    {
        static float[,] clientpos = new float[3, 2];
        static AnimInfo[] animInfo = new AnimInfo[2];
        public static void Main(string[] args)
        {
            animInfo[0] = new AnimInfo();
            animInfo[1] = new AnimInfo(); // shh i know this is bad 
            //IPAddress ip;
            
                //ip = new IPAddress(new byte[] { byte.Parse(args[0]), byte.Parse(args[1]), byte.Parse(args[2]), byte.Parse(args[3]) });
            
            //IPAddress ip = new IPAddress(new byte[] { 127,0,0,1 });
            
            TcpListener server = new TcpListener(IPAddress.Any, 27015);
            

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);

            TcpClient client = server.AcceptTcpClient();

            NetworkStream stream = client.GetStream();
            Console.WriteLine("got a connection!");
            Thread thread2 = new Thread(() => Listening(ref stream, ref client,0));
            thread2.Start();

            TcpClient client2 = server.AcceptTcpClient();
            NetworkStream stream2 = client2.GetStream();
            Console.WriteLine("got another connection!");
            Thread thread3 = new Thread(() => Listening(ref stream2, ref client2,1));
            thread3.Start();
            while (true)
            {
                
                Thread.Sleep(14);
                
                for (int i = 0; i < clientpos.GetLength(1); i++)
                {
                    //byte[] bruhbytes = BitConverter.GetBytes(clientpos[i, 0]);
                    //byte[] bruhbytes2 = BitConverter.GetBytes(clientpos[i, 1]);
                    List<byte> bruhbytes = new List<byte> { (byte)(int)clientpos[0, i], (byte)(int)clientpos[1, i], (byte)(int)clientpos[2, i], (byte)animInfo[i].character_name.Length };
                    bruhbytes.AddRange(Encoding.ASCII.GetBytes(animInfo[i].character_name));
                    //bruhbytes.RemoveAt(bruhbytes.Count); //.remove null terminator at the end
                    bruhbytes.Add((byte)animInfo[i].character_index);
                    bruhbytes.Add((byte)animInfo[i].direction);
                    bruhbytes.Add(0x00);
                    if (i == 0)
                    {
                        stream2.Write(bruhbytes.ToArray());
                        //stream.Write(bruhbytes2);
                    }
                    else
                    {
                        stream.Write(bruhbytes.ToArray());
                        //stream2.Write(bruhbytes2);
                    }

                }
            }
        }
        public static void Messages(NetworkStream stream)
        {

        }
        public static void Listening(ref NetworkStream stream, ref TcpClient client,int clientNum)
        {
            while (true)
            {
                Byte[] bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);
                // List<byte> byteList = new List<byte>(bytes);
                //foreach(byte b in bytes)
                //{
                // Console.WriteLine(b.ToString());
                //  Console.WriteLine("line");
                //}
                //translate bytes of request to string
                //String data = Encoding.UTF8.GetString(bytes);
                //String data2;
                //Console.WriteLine(data);

                while (bytes.Length > 0)
                { 
                    switch (byteConvert(ref bytes))
                    {
                        case 0: //this should never happen
                            Console.WriteLine("something is very bad"); break;
                        case 1: //this is position processing information.
                            clientpos[0, clientNum] = byteConvert(ref bytes); //x
                            clientpos[1, clientNum] = byteConvert(ref bytes); //y;
                            break;
                        case 2: //map change
                            clientpos[2, clientNum] = byteConvert(ref bytes);
                            break;
                        case 3: // anim change
                            animInfo[clientNum].character_name = Encoding.UTF8.GetString(genericConvert(ref bytes, byteConvert(ref bytes)));
                            animInfo[clientNum].character_index = byteConvert(ref bytes);
                            animInfo[clientNum].direction = byteConvert(ref bytes);
                            break;
                        default:
                            Console.WriteLine("unsupported packet type");
                            break;
                    }
                }
            }
        }
        static byte[] genericConvert(ref byte[] array, int length)
        {
            byte[] ary = new byte[length];
            byte[] array2 = new byte[array.Length - length];
            Buffer.BlockCopy(array, 0, ary, 0, length);
            Buffer.BlockCopy(array, length, array2, 0, array.Length - length);
            array = array2;
            return ary;
        }
        static int byteConvert(ref byte[] array) //this one takes 1 element out when its done reading
        {
            int num = (int)array[0] * 256 + (int)array[1];
            byte[] array2 = new byte[array.Length - 2];
            Buffer.BlockCopy(array, 2, array2, 0, array.Length - 2);
            array = array2;
            return num;
        }
        static int byteConvert(byte[] array, int index)
        {
            return (int)array[index] * 256 + (int)array[index + 1];
        }
        static int findNthOccur(String str, char ch, int N)
        {
            int occur = 0;

            // Loop to find the Nth 
            // occurence of the character 
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ch)
                {
                    occur += 1;
                }
                if (occur == N)
                    return i;
            }
            return -1;
        }
    }
    class AnimInfo {
        public string character_name;
        public int direction;
        public int character_index;

        public AnimInfo()
        {
            character_name = "";
            direction = 0;
            character_index = 0;
        }
    }
}
