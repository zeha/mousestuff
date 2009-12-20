using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MouseStuff
{
    public class MouseEvent
    {
        public uint posX { get; set; }
        public uint posY { get; set; }
        public bool button1 { get; set; }
        public bool button2 { get; set; }
        public uint timeslot { get; set; }

        public String toString()
        {
            return String.Format("x={0} y={1} b1={2} b2={3}", posX, posY, button1, button2);
        }

        public String toGmsEventString()
        {
            return String.Format("{0} {1} {2} {3}", posX, posY, button1?1:0, button2?1:0);
        }

        public bool IsEqual(MouseEvent other)
        {
            if (other == null) return false;
            return (other.posX == posX && other.posY == posY && other.button1 == button1 && other.button2 == button2);
        }

        public static MouseEvent fromGmsEventString(String gmsEventString, uint timeslot) {
            MouseEvent m = new MouseEvent();
            String[] parts = gmsEventString.Split(' ');
            m.posX = UInt32.Parse(parts[0]);
            m.posY = UInt32.Parse(parts[1]);
            m.button1 = (parts[2] == "1" || parts[2] == "True");
            m.button2 = (parts[3] == "1" || parts[3] == "True");
            m.timeslot = timeslot;
            return m;
        }
    }
}
