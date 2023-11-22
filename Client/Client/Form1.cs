using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public partial class Form1 : Form
    {
        UdpClient udpClient = new UdpClient(5154);
        IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        int serverPort = 5155;
        public Form1()
        {
            InitializeComponent();            
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (textBoxCommand.Text == "")
            {
                labelError.Text = "Не обрано команду!";
            }
            else if (textBoxCommand.Text == "get time")
            {
                string command = textBoxCommand.Text;
                string parameters = textBoxParams.Text + " " + DateTime.Now;
                string message = command + "|" + parameters;
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, new IPEndPoint(serverIP, serverPort));
                textBoxSend.AppendText(command + Environment.NewLine + parameters + Environment.NewLine);
                textBoxCommand.Text = "";
                textBoxParams.Text = "";
                labelError.Text = "";
            }
            else if (textBoxCommand.Text == "get width" || textBoxCommand.Text == "get height")
            {
                string command = textBoxCommand.Text;
                string message = command;
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, new IPEndPoint(serverIP, serverPort));
                textBoxSend.AppendText(command + Environment.NewLine);
                textBoxCommand.Text = "";
                textBoxParams.Text = "";
                labelError.Text = "";
            }
            else if (textBoxCommand.Text == "load sprite")
            {
                string[] parts = textBoxParams.Text.Split(' ');

                string imagePath = parts[3];
                Bitmap bitmap = new Bitmap(imagePath);
                byte[] imageBytes;
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    imageBytes = stream.ToArray();
                }

                string command = textBoxCommand.Text;
                string parameters = $"{parts[0]} {parts[1]} {parts[2]} {Convert.ToBase64String(imageBytes)}";
                string message = command + "|" + parameters;
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, new IPEndPoint(serverIP, serverPort));
                textBoxSend.AppendText(command + Environment.NewLine + textBoxParams.Text + Environment.NewLine);
                textBoxCommand.Text = "";
                textBoxParams.Text = "";
                labelError.Text = "";
            }
            else
            {
                string command = textBoxCommand.Text;
                string parameters = textBoxParams.Text;
                string message = command + "|" + parameters;
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, new IPEndPoint(serverIP, serverPort));
                textBoxSend.AppendText(command + Environment.NewLine + parameters + Environment.NewLine);
                textBoxCommand.Text = "";
                textBoxParams.Text = "";
                labelError.Text = "";
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5154);
                byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);
                string receivedData = Encoding.UTF8.GetString(receivedBytes);

                this.Invoke((Action)(() =>
                {
                    labelRecieve.Text = receivedData;
                }));

                udpClient.BeginReceive(ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при отриманні даних: " + ex.Message);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            udpClient.Close();
        }
    }
}