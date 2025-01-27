﻿using System;
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
        //static CharInfo[] chars = new CharInfo[255]; 
        //static List<CharInfo> players = new List<CharInfo>();
        static CharInfo[] players = new CharInfo[255];
        //static List<NetworkStream> streams = new List<NetworkStream>();

        //static float[,] clientpos = new float[3, 2];
        //static AnimInfo[] animInfo = new AnimInfo[2];

        public static void Main(string[] args)
        {
            //animInfo[0] = new AnimInfo();
            //animInfo[1] = new AnimInfo(); // shh i know this is bad 
            //IPAddress ip;
            
                //ip = new IPAddress(new byte[] { byte.Parse(args[0]), byte.Parse(args[1]), byte.Parse(args[2]), byte.Parse(args[3]) });
            
            //IPAddress ip = new IPAddress(new byte[] { 127,0,0,1 });
            
            TcpListener server = new TcpListener(IPAddress.Any, 27015);
            

            server.Start();
            ConsoleHelper.Log(ConsoleHelper.MessageType.info, "Server has started, Waiting for a connection...");

            Thread thread = new Thread(() =>
            {
                int i = 0;
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    ConsoleHelper.Log(ConsoleHelper.MessageType.info, "got a connection!");
                    i++; //increment i after the thread starts so that i dont have to race
                    players[i] = new CharInfo();
                    players[i].client = client;
                    players[i].stream = stream;
                    Thread thread2 = new Thread(() => Listening(ref stream, ref client, i));
                    thread2.Start();
                    
                }
            });
            thread.Start();
            

            /*TcpClient client2 = server.AcceptTcpClient();
            NetworkStream stream2 = client2.GetStream();
            ConsoleHelper.Log(ConsoleHelper.MessageType.info,"got another connection!");
            players.Add(new CharInfo());
            Thread thread3 = new Thread(() => Listening(ref stream2, ref client2,1));
            thread3.Start();*/
            while (true)
            {
                
                Thread.Sleep(14);
                
                for (int i = 0; i < 255; i++)
                {
                    if (players[i] != null)
                    {
                        //byte[] bruhbytes = BitConverter.GetBytes(clientpos[i, 0]);
                        //byte[] bruhbytes2 = BitConverter.GetBytes(clientpos[i, 1]);
                        List<byte> bruhbytes = new List<byte> { (byte)players[i].x, (byte)players[i].y, (byte)players[i].map, (byte)players[i].character_name.Length };
                        bruhbytes.AddRange(Encoding.ASCII.GetBytes(players[i].character_name));
                        //bruhbytes.RemoveAt(bruhbytes.Count); //.remove null terminator at the end
                        bruhbytes.Add((byte)players[i].character_index);
                        bruhbytes.Add((byte)players[i].direction);
                        bruhbytes.Add(0x00);
                        
                            if (players[i] != null)
                            {
                                for(int j = 0; j < 255; j++)
                                {
                                    if(j!= i&& players[j] != null)
                                    {
                                    try { 
                                        players[j].stream.Write(bruhbytes.ToArray());
                                    }
                                    catch (System.IO.IOException e)
                                    {
                                        ConsoleHelper.Log(ConsoleHelper.MessageType.net, "player " + j.ToString() + " disconnected.");
                                        players[j] = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void Listening(ref NetworkStream stream, ref TcpClient client,int clientNum)
        {
            while (true)
            {
                if(players[clientNum] == null)
                {
                    break;
                }
                else if (client.Available > 0) //remember that client packets are always complete when processed, not cut off in the middle.
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

                    //String data2;

                    while (bytes.Length > 0)
                    { 
                        switch (byteConvert(ref bytes))
                        {
                            case 0: //debug message
                                string log = "";
                                foreach(byte a in bytes)
                                {
                                    log += a.ToString();
                                    log += " ";
                                }
                                ConsoleHelper.Log(ConsoleHelper.MessageType.net, log);
                                String data = Encoding.UTF8.GetString(bytes);
                                ConsoleHelper.Log(ConsoleHelper.MessageType.net,data);
                                bytes = new byte[0];
                                break;
                            case 1: //this is position processing information.
                                players[clientNum].x = byteConvert(ref bytes); //x
                                players[clientNum].y = byteConvert(ref bytes); //y;
                                break;
                            case 2: //map change
                                players[clientNum].map = byteConvert(ref bytes);
                                break;
                            case 3: // anim change
                                players[clientNum].character_name = Encoding.UTF8.GetString(genericConvert(ref bytes, byteConvert(ref bytes)));
                                players[clientNum].character_index = byteConvert(ref bytes);
                                players[clientNum].direction = byteConvert(ref bytes);
                                break;
                            default:
                                ConsoleHelper.Log(ConsoleHelper.MessageType.error, "unsupported packet type");
                                string log = "";
                                foreach (byte a in bytes)
                                {
                                    log += a.ToString();
                                    log += " ";
                                }
                                ConsoleHelper.Log(ConsoleHelper.MessageType.net, log);
                                String data = Encoding.UTF8.GetString(bytes);
                                ConsoleHelper.Log(ConsoleHelper.MessageType.net, data);
                                bytes = new byte[0];
                                break;
                        }
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
    class CharInfo
    {
        public string character_name;
        public int direction;
        public int character_index;
        public int map;
        public int x;
        public int y;
        public TcpClient client;
        public NetworkStream stream;

        public CharInfo()
        {
            character_name = "";
            direction = 0;
            character_index = 0;
            map = 0;
            x = 0;
            y = 0;
        }
    }
    class ConsoleHelper
    {
        public enum MessageType
        {
            info = 0,
            error = 1,
            net = 2,
        }
        public static void Log(MessageType type, string message)
        {
            Console.Write("[");
            switch (type){
                case MessageType.info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("info");
                    Console.ResetColor();
                    break;
                case MessageType.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR");
                    Console.ResetColor();
                    break;
                case MessageType.net:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("net");
                    Console.ResetColor();
                    break;
            }
            Console.Write("] ");
            Console.WriteLine(message);
        }

    }
}
