
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
public class TelecomClient
{
    public bool connected;
    public string serverIP;
    public int serverTCPPort;
    public int serverUDPPort;
    public TcpClient tcpClient;
    private NetworkStream tcpStream;
    public UdpClient udpClient;
    public string privateIP;
    private string userName;
    private int count;
    private string connectionString;
    private string message;
    private string myPublicIP;
    private string myPrivateIP;
    private bool isUDPListenerStarted = false;
    public TelecomClient(String serverIP)
    {
        count = 0;
        connected = false;
        //serverIP = "192.168.1.8"
        this.serverIP = serverIP;
        serverTCPPort = 5090;
        serverUDPPort = 5100;

        string host = System.Net.Dns.GetHostName();
        privateIP = GetLocalIPAddress();
        Console.WriteLine(privateIP.ToString());

        bool created = false;
        int privatePort = 5000;
        while (created == false)
        {
            try
            {
                udpClient = new UdpClient(privatePort);
                created = true;
            }
            catch (Exception ex)
            {
                privatePort += 1;
            }
        }
    }

    public void sendMessage(string content, string ip, int port)
    {
        udpClient.Connect(ip, port);
        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(content);
        udpClient.Send(sendBytes, sendBytes.Length);
    }

    public void sendMessageTCP(string message)
    {
        this.message = message;
        Thread trd = new Thread(sendMessageTCP_trd);
        trd.Start();
    }

    private void sendMessageTCP_trd()
    {
        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(this.message + Environment.NewLine);
        NetworkStream stream = tcpClient.GetStream();
        stream.Write(sendBytes, 0, sendBytes.Length);
    }

    public void connect(string userName)
    {
        this.userName = userName;
        Thread trd = new Thread(connect_UDP);
        trd.Start();
    }

    public void connect2()
    {
        Thread trd = new Thread(connect_TCP);
        trd.Start();
    }

    public void startUDP_listener()
    {
        Thread trd = new Thread(UDP_listener);
        trd.Start();
    }

    private void UDP_listener()
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Console.WriteLine("UDP Listener started");
        while (true)
        {
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = System.Text.Encoding.ASCII.GetString(receiveBytes);
            Console.WriteLine("UDP Received:" + returnData);
            //this.mainForm.txtChat.Text += returnData;
        }
    }

    private void connect_UDP()
    {
        // This constructor arbitrarily assigns the local port number.
        if (connected == true)
        {
            return;
        }
        if (count >= 5)
        {
            //connected = True
            count = 0;
            return;
        }
        count += 1;

        try
        {
            udpClient.Connect(serverIP, serverUDPPort);
            IPEndPoint ipEndpoint = (IPEndPoint)udpClient.Client.LocalEndPoint;
            string port = ipEndpoint.Port.ToString();
            //Dim ip As String = ipEndpoint.AddressFamily.ToString()

            // Sends a message to the host to which you have connected.
            string identity = "{\"userName\":\"" + userName + "\",publicIp\":\"0\"," + "\"privateIp\":\"" + privateIP + "\",\"privatePort\":\"" + port + "\",\"publicPort\":\"0\"}";
            Console.WriteLine(identity);
            Byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(identity);
            //sendBytes(0) = 0

            udpClient.Send(sendBytes, sendBytes.Length);

            // Sends message to a different host using optional hostname and port parameters.
            //Dim udpClientB As New UdpClient()
            //udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", 11000)

            // IPEndPoint object will allow us to read datagrams sent from any source.
            //Dim RemoteIpEndPoint As New IPEndPoint(New IPAddress(Encoding.ASCII.GetBytes(serverIP)), serverPort)
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //Dim RemoteIpEndPoint As New IPEndPoint(New IPAddress(serverIP), serverPort)

            // UdpClient.Receive blocks until a message is received from a remote host.
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = System.Text.Encoding.ASCII.GetString(receiveBytes);
            // Which one of these two hosts responded?
            this.connectionString = returnData.ToString();
            Console.WriteLine(("This is the message you received " + this.connectionString));
            Console.WriteLine(("This message was sent from " + RemoteIpEndPoint.Address.ToString() + " on their port number " + RemoteIpEndPoint.Port.ToString()));
            if ((returnData.ToString() == "SUCCESS"))
            {
                connected = true;
            }
            this.connected = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public void connect_TCP()
    {
        try
        {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.

            //Dim identity As String = "username:" + Me.userName + vbNewLine
            string identity = this.connectionString;
            //Dim publicIP As String = IpAddress()
            //Console.WriteLine(publicIP)
            //Return

            Byte[] data = System.Text.Encoding.UTF8.GetBytes(identity);
            //Data(0) = 0
            //Console.WriteLine(data(0))
            Console.WriteLine(identity);

            tcpClient = new TcpClient(serverIP, serverTCPPort);
            Console.WriteLine(identity);
            //udpClient = New UdpClient(serverIP, serverPort)
            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();
            //Dim bw As System.IO.StreamWriter = New System.IO.StreamWriter()

            tcpStream = tcpClient.GetStream();
            // Translate the passed message into ASCII and store it as a Byte array.
            Console.WriteLine(identity);
            // Send the message to the connected TcpServer. 
            tcpStream.Write(data, 0, data.Length);
            Console.WriteLine(identity);
            //stream.Close()
            //Console.WriteLine("Sent: {0}", "identity:" + identity)
            // Read the first batch of the TcpServer response bytes.
            System.IO.BufferedStream br = new System.IO.BufferedStream(tcpStream);
            System.IO.StreamReader streamReader = new System.IO.StreamReader(br);
            while ((streamReader.EndOfStream == false))
            {
                string currentLine = streamReader.ReadLine();
                Console.WriteLine("Received:" + currentLine);
                Console.WriteLine("In busy waiting");
                //this.mainForm.txtMain.Text += currentLine + Constants.vbNewLine;
            }
            //stream.Read(data, 0, 2)
            //Dim count As Integer
            //br.Read(data, 0, 2)
            //Dim bytes As Int32 = stream.ReadByte()
            Console.Write("TPC closed");
            tcpStream.Close();
            // Close everything.
            tcpClient.Close();
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
    }

    public void close()
    {
        if ((connected == true))
        {
            udpClient.Close();
        }

        tcpClient.Close();
        Console.Write("Client is closed");
    }

    private Dictionary<string, string> parseResult(string message)
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();

        return ret;
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("Local IP Address Not Found!");
    }
}