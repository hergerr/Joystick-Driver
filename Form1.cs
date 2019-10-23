using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.DirectInput;
using System.Runtime.InteropServices;

namespace PadPS4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetSticks();
            sticks = GetSticks();

            // uruchomienie timera umozliwia odswiezanie
            timer1.Enabled = true;
        }

        // obiekt directInput
        DirectInput Input = new DirectInput();

        // obiekt joysticka
        SlimDX.DirectInput.Joystick stick;
        Joystick[] sticks;
        bool mouseClicked = false;

        int yValue = 0;
        int xValue = 0;
        int zValue = 0;

        Rectangle rect;
        Point LocationXY;
        Point LocationX1Y1;
        bool isMouseDown = false;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern void mouse_event(uint flag, uint _x, uint _y, uint btn, uint exInfo);
        private const int MOUSEEVENT_LEFTDOWN = 0x02;
        private const int MOUSEEVENT_LEFTUP = 0x04;
        

        // szuka podlaczonego do komputera kontrolera/joisticka
        public Joystick[] GetSticks()
        {
            List<SlimDX.DirectInput.Joystick> sticks = new List<SlimDX.DirectInput.Joystick>();
            foreach(DeviceInstance device in Input.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                try
                {
                    stick = new SlimDX.DirectInput.Joystick(Input, device.InstanceGuid);
                    stick.Acquire();

                    foreach(DeviceObjectInstance deviceObject in stick.GetObjects())
                    {
                        if((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                        {
                            stick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-100, 100);
                        }
                    }
                    sticks.Add(stick);
                }
                catch (DirectInputException)
                {
                }

            }
            // kopiuje elementy z listy do tablicy
            return sticks.ToArray();
        }

        // pobiera stan kontrolera i ustawia wartosci przyciskow w zmiennych
        void stickHandle(Joystick stick, int id)
        {
            JoystickState state = new JoystickState();
            state = stick.GetCurrentState();

            yValue = state.Y;
            xValue = state.X;
            zValue = state.Z;
            MouseMoves(xValue, yValue);

            bool[] buttons = state.GetButtons();

            // przycisk x na ps4
            if (id == 0)
            {
                // true, jesli jest nacisniety
                if (buttons[0])
                {
                    if (mouseClicked == false)
                    {
                        // wywolanie klika myszki i ustawienie odpowiedniej flagi klikniecia
                        mouse_event(MOUSEEVENT_LEFTDOWN, 0, 0, 0, 0);
                        mouseClicked = true;
                    }
                } else if (buttons[1]) // kolejne guziki
                {
                    Console.WriteLine("XXXXXXXXX");
                }
                else
                {
                    if (mouseClicked == true)
                    {
                        mouse_event(MOUSEEVENT_LEFTUP, 0, 0, 0, 0);
                        mouseClicked = false;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           // Joystick[] joystick = GetSticks();
        }

        // obsluga pozycji kursora
        public void MouseMoves(int posx, int posy) {
            Cursor.Position = new Point(Cursor.Position.X + posx/3, Cursor.Position.Y + posy/3);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < sticks.Length; i++)
            {
                Console.WriteLine(sticks.Length);
                stickHandle(sticks[i], i);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            LocationXY = e.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                LocationX1Y1 = e.Location;
                Refresh();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                LocationX1Y1 = e.Location;
                isMouseDown = false;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if(rect != null && radioButton1.Checked)
            {
                e.Graphics.DrawRectangle(Pens.Red, GetRect());
            } else if (rect != null && radioButton2.Checked)
            {
                e.Graphics.DrawEllipse(Pens.Black, GetRect());
            }
        }

        private Rectangle GetRect()
        {
            rect = new Rectangle();
            rect.X = Math.Min(LocationXY.X, LocationX1Y1.X);
            rect.Y = Math.Min(LocationXY.Y, LocationX1Y1.Y);
            rect.Width = Math.Abs(LocationXY.X - LocationX1Y1.X);
            rect.Height = Math.Abs(LocationXY.Y - LocationX1Y1.Y);

            return rect;
        }
    }
}
