using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
namespace Server
{

    public partial class Form1 : Form
    {
        private UdpClient udpServer;
        private bool isRunning = true;
        IPAddress clientIP = IPAddress.Parse("127.0.0.1");

        int orientation = 0;
        Color backCol = Color.White;
        List<PixelParameters> pixels = new List<PixelParameters>();
        List<LineParameters> lines = new List<LineParameters>();
        List<RectangleParameters> rectangles = new List<RectangleParameters>();
        List<EllipseParameters> ellipses = new List<EllipseParameters>();
        List<CircleParameters> circles = new List<CircleParameters>();
        List<RoundedRectangleParameters> roundedRectangles = new List<RoundedRectangleParameters>();
        List<Sprite> sprites = new List<Sprite>();

        public class PixelParameters
        {
            public int X { get; set; }
            public int Y { get; set; }
            public Color Color { get; set; }

            public PixelParameters(int x, int y, Color color)
            {
                X = x;
                Y = y;
                Color = color;
            }
        }

        public class LineParameters
        {
            public int X1 { get; set; }
            public int Y1 { get; set; }
            public int X2 { get; set; }
            public int Y2 { get; set; }
            public Color Color { get; set; }

            public LineParameters(int x1, int y1, int x2, int y2, Color color)
            {
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
                Color = color;
            }
        }

        public class RectangleParameters
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public Color Color { get; set; }
            public bool Filled { get; set; }

            public RectangleParameters(int x, int y, int width, int height, Color color, bool filled)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Color = color;
                Filled = filled;
            }
        }

        public class EllipseParameters
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int RadiusX { get; set; }
            public int RadiusY { get; set; }
            public Color Color { get; set; }
            public bool Filled { get; set; }

            public EllipseParameters(int x, int y, int radiusX, int radiusY, Color color, bool filled)
            {
                X = x;
                Y = y;
                RadiusX = radiusX;
                RadiusY = radiusY;
                Color = color;
                Filled = filled;
            }
        }

        public class CircleParameters
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Radius { get; set; }
            public Color Color { get; set; }
            public bool Filled { get; set; }

            public CircleParameters(int x, int y, int radius, Color color, bool filled)
            {
                X = x;
                Y = y;
                Radius = radius;
                Color = color;
                Filled = filled;
            }
        }

        public class RoundedRectangleParameters
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Radius { get; set; }
            public Color Color { get; set; }
            public bool Filled { get; set; }

            public RoundedRectangleParameters(int x, int y, int width, int height, int radius, Color color, bool filled)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Radius = radius;
                Color = color;
                Filled = filled;
            }
        }

        public class Sprite
        {
            public int Index { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public byte[] Data { get; set; }

            public Sprite(int index, int width, int height, byte[] data)
            {
                Index = index;
                Width = width;
                Height = height;
                Data = data;
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeUdpServer();
        }

        private void InitializeUdpServer()
        {
            udpServer = new UdpClient(5155);
            udpServer.BeginReceive(ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5155);
                byte[] receivedBytes = udpServer.EndReceive(ar, ref endPoint);
                string receivedData = Encoding.UTF8.GetString(receivedBytes);

                this.Invoke((Action)(() =>
                {
                    ProcessReceivedData(receivedData);
                }));

                if (isRunning)
                {
                    udpServer.BeginReceive(ReceiveCallback, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при отриманнi даних: " + ex.Message);
            }
        }

        private void ProcessReceivedData(string receivedData)
        {
            string[] parts = receivedData.Split('|');
            if (parts.Length >= 2)
            {
                string command = parts[0];
                string parameters = parts[1];

                labelCommand.Text = "Отримана команда: " + command;
                labelParam.Text = "Отримані параметри: " + parameters;

                RecogniseCommand(command, parameters);
            }
            else if (parts.Length == 1)
            {
                string command = parts[0];

                labelCommand.Text = "Отримана команда: " + command;

                switch(command)
                {
                    case "get width":
                        GetWidth();
                        break;
                    case "get height":
                        GetHeight();
                        break;
                    default:
                        Console.WriteLine("Невідома команда: " + command);
                        break;
                }
            }
        }

        private void RecogniseCommand(string command, string parameters)
        {
            string[] parts = parameters.Split(' ');

            Color color = Color.White;

            if (parts.Length > 0 && command != "draw image" && command != "set orientation" && command != "load sprite" && command != "show sprite")
            {
                string colorString = parts[0];
                try
                {
                    int colorValue = Convert.ToInt32(colorString, 16);
                    int r = (colorValue >> 11) & 0x1F;
                    int g = (colorValue >> 5) & 0x3F;
                    int b = colorValue & 0x1F;
                    color = Color.FromArgb((r << 3), (g << 2), (b << 3));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка при розбірі кольору: " + ex.Message);
                }
            }

            switch (command)
            {
                case "get time":
                    string stringTime = parts[3] + " " + parts[4];
                    DateTime clientTime = DateTime.Parse(stringTime);
                    GetTime(int.Parse(parts[1]), int.Parse(parts[2]), clientTime, color);
                    break;
                case "clear display":
                    backCol = color;
                    pixels.Clear();
                    lines.Clear();
                    rectangles.Clear();
                    ellipses.Clear();
                    circles.Clear();
                    roundedRectangles.Clear();
                    Draw();
                    break;
                case "draw pixel":
                    pixels.Add(new PixelParameters(int.Parse(parts[1]), int.Parse(parts[2]), color));
                    Draw();
                    break;
                case "draw line":                  
                    lines.Add(new LineParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color));
                    Draw();
                    break;
                case "draw rectangle":
                    rectangles.Add(new RectangleParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color, false));
                    Draw();
                    break;
                case "fill rectangle":
                    rectangles.Add(new RectangleParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color, true));
                    Draw();
                    break;
                case "draw ellipse":
                    ellipses.Add(new EllipseParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color, false));
                    Draw();
                    break;
                case "fill ellipse":
                    ellipses.Add(new EllipseParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), color, true));
                    Draw();
                    break;
                case "draw circle":
                    circles.Add(new CircleParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), color, false));
                    Draw();
                    break;
                case "fill circle":
                    circles.Add(new CircleParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), color, true));
                    Draw();
                    break;
                case "draw rounded rectangle":
                    roundedRectangles.Add(new RoundedRectangleParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), color, false));
                    Draw();
                    break;
                case "fill rounded rectangle":
                    roundedRectangles.Add(new RoundedRectangleParameters(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), color, true));
                    Draw();
                    break;
                case "draw text":
                    DrawText(int.Parse(parts[1]), int.Parse(parts[2]), color, parts[3]);
                    break;
                case "draw image":
                    string imagePath = parts[0];
                    DrawImage(int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), imagePath);
                    break;
                case "set orientation":
                    orientation = int.Parse(parts[0]);
                    Draw();
                    break;
                case "load sprite":
                    byte[] imageBytes = Convert.FromBase64String(parts[3]);
                    sprites.Add(new Sprite(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), imageBytes));
                    break;
                case "show sprite":
                    ShowSprite(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
                    break;
                case "running line":
                    string text = string.Join(" ", parts.Skip(5));
                    RuningLine(color, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), parts[4], text);
                    break;
                default:
                    Console.WriteLine("Невідома команда: " + command);
                    break;
            }
        }

        private void GetTime(int x0, int y0, DateTime clientTime, Color color)
        {
            Label labelTime = new Label();
            labelTime.ForeColor = color;
            labelTime.Text = clientTime.ToString("HH:mm:ss");
            labelTime.Font = new Font("Times New Roman", 30, FontStyle.Bold);
            labelTime.AutoSize = true;
            labelTime.Location = new Point(x0, y0);
            splitContainer1.Panel2.Controls.Add(labelTime);

            Label labelDate = new Label();
            labelDate.Text = clientTime.ToString("dd.MM") + "\n" + clientTime.Year.ToString();
            labelDate.Location = new Point(x0 + 165, y0);
            labelDate.Font = new Font("Times New Roman", 15, FontStyle.Regular);
            labelDate.AutoSize = true;
            splitContainer1.Panel2.Controls.Add(labelDate);

            timer1.Tick += (sender, e) =>
            {
                clientTime = clientTime.AddSeconds(1);
                labelTime.Text = clientTime.ToString("HH:mm:ss");
            };
        }

        private void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            Pen pen = new Pen(color, 2);

            g.DrawLine(pen, x0, y0, x1, y1);

            pen.Dispose();
            g.Dispose();
        }

        private void DrawText(int x0, int y0, Color color, string text)
        {
            int x = x0;
            int y = y0;

            foreach (char letter in text)
            {
                switch (letter)
                {
                    case 'a':
                        DrawLetterA(x, y, color);
                        x += 25;
                        break;
                    case 'b':
                        DrawLetterB(x, y, color);
                        x += 25;
                        break;
                    case 'c':
                        DrawLetterC(x, y, color);
                        x += 25;
                        break;
                    case 'd':
                        DrawLetterD(x, y, color);
                        x += 25;
                        break;
                    case 'e':
                        DrawLetterE(x, y, color);
                        x += 25;
                        break;
                    case 'h':
                        DrawLetterH(x, y, color);
                        x += 25;
                        break;
                    case 'l':
                        DrawLetterL(x, y, color);
                        x += 5;
                        break;
                    case 'o':
                        DrawLetterO(x, y, color);
                        x += 25;
                        break;
                    case 'r':
                        DrawLetterR(x, y, color);
                        x += 20;
                        break;
                    case 'w':
                        DrawLetterW(x, y, color);
                        x += 45;
                        break;
                    case '_':
                        DrawLetter_(x, y, color);
                        x += 25;
                        break;
                }
            }
        }

        private void DrawLetterA(int x, int y, Color color)
        {
            DrawLine(x + 5, y + 10, x + 20, y + 10, color);
            DrawLine(x + 20, y + 10, x + 20, y + 40, color);
            DrawLine(x + 20, y + 40, x + 2, y + 40, color);
            DrawLine(x + 2, y + 40, x + 2, y + 20, color);
            DrawLine(x + 2, y + 20, x + 20, y + 20, color);
        }

        private void DrawLetterB(int x, int y, Color color)
        {
            DrawLine(x, y, x, y + 40, color); 
            DrawLine(x, y + 40, x + 20, y + 40, color); 
            DrawLine(x + 20, y + 40, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x, y + 20, color);
        }

        private void DrawLetterC(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x, y + 20, x, y + 40, color);
            DrawLine(x, y + 40, x + 20, y + 40, color);
        }

        private void DrawLetterD(int x, int y, Color color)
        {
            DrawLine(x + 20, y, x + 20, y + 40, color);
            DrawLine(x + 20, y + 40, x + 2, y + 40, color);
            DrawLine(x + 2, y + 40, x + 2, y + 20, color);
            DrawLine(x + 2, y + 20, x + 20, y + 20, color);
        }

        private void DrawLetterE(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x, y + 40, color);
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 20, y + 30, color);
            DrawLine(x + 20, y + 30, x, y + 30, color);
            DrawLine(x, y + 40, x + 20, y + 40, color);
        }

        private void DrawLetterH(int x, int y, Color color)
        {
            DrawLine(x, y, x, y + 40, color);
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 20, y + 40, color);
        }

        private void DrawLetterL(int x, int y, Color color)
        {
            DrawLine(x, y, x, y + 40, color);
        }

        private void DrawLetterO(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 20, y + 40, color);
            DrawLine(x + 20, y + 40, x, y + 40, color);
            DrawLine(x, y + 40, x, y + 20, color);
        }
        private void DrawLetterR(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x, y + 40, color);
            DrawLine(x, y + 23, x + 15, y + 23, color);
            DrawLine(x + 15, y + 23, x + 15, y + 26, color);
        }
        private void DrawLetterW(int x, int y, Color color)
        {
            DrawLine(x, y + 20, x + 10, y + 40, color);
            DrawLine(x + 10, y + 40, x + 20, y + 20, color);
            DrawLine(x + 20, y + 20, x + 30, y + 40, color);
            DrawLine(x + 30, y + 40, x + 40, y + 20, color);
        }

        private void DrawLetter_(int x, int y, Color color)
        {
            DrawLine(x, y + 40, x + 20, y + 40, color);
        }

        private void DrawImage(int x0, int y0, int width, int height, string imagePath)
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();

            Image image = Image.FromFile(imagePath);
            g.DrawImage(image, x0, y0, width, height);

            image.Dispose();
            g.Dispose();
        }

        private void GetWidth()
        {
            int width = this.Width;
            string response = "Отримана ширина: " + width;
            
            byte[] data = Encoding.UTF8.GetBytes(response);
            udpServer.Send(data, data.Length, new IPEndPoint(clientIP, 5154));
        }

        private void GetHeight()
        {
            int height = this.Height;
            string response = "Отримана висота: " + height;

            byte[] data = Encoding.UTF8.GetBytes(response);
            udpServer.Send(data, data.Length, new IPEndPoint(clientIP, 5154));
        }

        private void Draw()
        {
            Graphics g = splitContainer1.Panel2.CreateGraphics();
            g.Clear(backCol);
            int angle = 0;

            switch(orientation)
            {
                case 1:
                    angle = 90;
                    break;
                case 2:
                    angle = 180;
                    break;
                case 3:
                    angle = 270;
                    break;
                default:
                    angle = 0;
                    break;
            }

            g.TranslateTransform(splitContainer1.Panel2.Width / 2, splitContainer1.Panel2.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-splitContainer1.Panel2.Width / 2, -splitContainer1.Panel2.Height / 2);


            foreach (LineParameters lineParams in lines)
            {
                g.DrawLine(new Pen(lineParams.Color, 2), lineParams.X1, lineParams.Y1, lineParams.X2, lineParams.Y2);
            }
            foreach (PixelParameters pixelParams in pixels)
            {
                g.FillRectangle(new SolidBrush(pixelParams.Color), pixelParams.X, pixelParams.Y, 1, 1);
            }
            foreach (RectangleParameters rectangleParams in rectangles)
            {
                if (rectangleParams.Filled)
                {
                    g.FillRectangle(new SolidBrush(rectangleParams.Color), rectangleParams.X, rectangleParams.Y, rectangleParams.Width, rectangleParams.Height);
                }
                else
                {
                    g.DrawRectangle(new Pen(rectangleParams.Color, 2), rectangleParams.X, rectangleParams.Y, rectangleParams.Width, rectangleParams.Height);
                }
            }
            foreach (EllipseParameters ellipseParams in ellipses)
            {
                if (ellipseParams.Filled)
                {
                    g.FillEllipse(new SolidBrush(ellipseParams.Color), ellipseParams.X - ellipseParams.RadiusX, ellipseParams.Y - ellipseParams.RadiusY, ellipseParams.RadiusX * 2, ellipseParams.RadiusY * 2);
                }
                else
                {
                    g.DrawEllipse(new Pen(ellipseParams.Color, 2), ellipseParams.X - ellipseParams.RadiusX, ellipseParams.Y - ellipseParams.RadiusY, ellipseParams.RadiusX * 2, ellipseParams.RadiusY * 2);
                }
            }
            foreach (CircleParameters circleParams in circles)
            {
                if (circleParams.Filled)
                {
                    g.FillEllipse(new SolidBrush(circleParams.Color), circleParams.X - circleParams.Radius, circleParams.Y - circleParams.Radius, circleParams.Radius * 2, circleParams.Radius * 2);
                }
                else
                {
                    g.DrawEllipse(new Pen(circleParams.Color, 2), circleParams.X - circleParams.Radius, circleParams.Y - circleParams.Radius, circleParams.Radius * 2, circleParams.Radius * 2);
                }
            }
            foreach (RoundedRectangleParameters roundedRectParams in roundedRectangles)
            {
                if (roundedRectParams.Filled)
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddArc(roundedRectParams.X, roundedRectParams.Y, roundedRectParams.Radius, roundedRectParams.Radius, 180, 90);
                    path.AddArc(roundedRectParams.X + roundedRectParams.Width - roundedRectParams.Radius, roundedRectParams.Y, roundedRectParams.Radius, roundedRectParams.Radius, 270, 90);
                    path.AddArc(roundedRectParams.X + roundedRectParams.Width - roundedRectParams.Radius, roundedRectParams.Y + roundedRectParams.Height - roundedRectParams.Radius, roundedRectParams.Radius, roundedRectParams.Radius, 0, 90);
                    path.AddArc(roundedRectParams.X, roundedRectParams.Y + roundedRectParams.Height - roundedRectParams.Radius, roundedRectParams.Radius, roundedRectParams.Radius, 90, 90);
                    path.CloseFigure();
                    g.FillPath(new SolidBrush(roundedRectParams.Color), path);
                }
                else
                {
                    Pen pen = new Pen(roundedRectParams.Color, 2);
                    GraphicsPath path = new GraphicsPath();
                    path.AddArc(roundedRectParams.X, roundedRectParams.Y, roundedRectParams.Radius, roundedRectParams.Radius, 180, 90);
                    path.AddArc(roundedRectParams.X + roundedRectParams.Width - roundedRectParams.Radius, roundedRectParams.Y, roundedRectParams.Radius, roundedRectParams.Radius, 270, 90);
                    path.AddArc(roundedRectParams.X + roundedRectParams.Width - roundedRectParams.Radius, roundedRectParams.Y + roundedRectParams.Height - roundedRectParams.Radius, roundedRectParams.Radius, roundedRectParams.Radius, 0, 90);
                    path.AddArc(roundedRectParams.X, roundedRectParams.Y + roundedRectParams.Height - roundedRectParams.Radius, roundedRectParams.Radius, roundedRectParams.Radius, 90, 90);
                    path.CloseFigure();
                    g.DrawPath(pen, path);
                }
            }
        }

        private void ShowSprite(int index, int x, int y)
        {
            Sprite spriteToDisplay = sprites.FirstOrDefault(sprite => sprite.Index == index);
            if (spriteToDisplay != null)
            {
                byte[] imageBytes = spriteToDisplay.Data;

                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image receivedImage = Image.FromStream(ms);
                    receivedImage = new Bitmap(receivedImage, new Size(spriteToDisplay.Width, spriteToDisplay.Height));

                    Graphics g = splitContainer1.Panel2.CreateGraphics();
                    g.DrawImage(receivedImage, x, y);
                    g.Dispose();
                }
            }
            else
            {
                Console.WriteLine("Спрайту з таким індексом не існує");
            }           
        }

        private void RuningLine(Color color, int x, int y, int speed, string direction, string text)
        {
            Label runningLabel = new Label();
            runningLabel.ForeColor = color;
            runningLabel.Font = new Font("Arial", 20);
            runningLabel.Text = text;
            runningLabel.AutoSize = true;
            runningLabel.Location = new Point(x, y);
            splitContainer1.Panel2.Controls.Add(runningLabel);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = speed;
            timer.Tick += (sender, e) =>
            {
                if (direction == "left")
                {
                    runningLabel.Left -= 1;
                    if (runningLabel.Right < 0)
                    {
                        runningLabel.Left = splitContainer1.Panel2.Width;
                    }
                }
                else if (direction == "right")
                {
                    runningLabel.Left += 1;
                    if (runningLabel.Left > splitContainer1.Panel2.Width)
                    {
                        runningLabel.Left = -runningLabel.Width;
                    }
                }
            };
            timer.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (udpServer != null)
            {
                udpServer.Close();
            }
        }

        private void buttonStop_Click_1(object sender, EventArgs e)
        {
            isRunning = false;
            udpServer.Close();
        }
    }
}