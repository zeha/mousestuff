using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace MouseStuff
{
    public class GhostMouseScript
    {
        public List<MouseEvent> MouseEvents;

        public GhostMouseScript()
        {
            MouseEvents = new List<MouseEvent>();
        }

        public void appendFromFile(String filename)
        {
            uint timeslot = 0;
            if (MouseEvents.Count > 0) timeslot = MouseEvents.Last().timeslot;
            StreamReader sr = new StreamReader(filename);
            MouseEvent lastMouseEvent = null;
            while (true)
            {
                timeslot++;
                String l = sr.ReadLine();
                if (l == null || l.Length == 0) break;
                MouseEvent mouseEvent = MouseEvent.fromGmsEventString(l, timeslot);
                if (mouseEvent.IsEqual(lastMouseEvent)) continue;
                MouseEvents.Add(mouseEvent);
                lastMouseEvent = mouseEvent;
            }
        }

        public void saveToFile(String filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            String mouseEventString = MouseEvents[0].toGmsEventString();
            uint timeslot = 1;
            foreach (MouseEvent mouseEvent in MouseEvents)
            {
                while (timeslot < mouseEvent.timeslot)
                {
                    timeslot++;
                    sw.WriteLine(mouseEventString);
                }
                mouseEventString = mouseEvent.toGmsEventString();
            }
            // write final timeslots and event
            while (timeslot <= MouseEvents.Last().timeslot)
            {
                timeslot++;
                sw.WriteLine(mouseEventString);
            }
            sw.Close();
        }
    }
}
