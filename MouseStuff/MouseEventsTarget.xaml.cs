using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MouseStuff
{
    public delegate bool UpdateMouseEventsTargetStatus(GhostMouseScript gms, int currentId);

    /// <summary>
    /// Interaction logic for MouseEventsTarget.xaml
    /// </summary>
    public partial class MouseEventsTarget : Window
    {

        private List<MouseEvent> mouseEvents;

        private Dictionary<ulong, MouseEvent> hashedClickEvents;
        private MouseEvent selectedMouseEvent = null;
        private Rectangle selectedRectangle = null;
        List<Rectangle> nearRectangles = new List<Rectangle>();

        SolidColorBrush brushSelected = new SolidColorBrush();

        public MouseEventsTarget()
        {
            InitializeComponent();
            brushSelected.Color = Color.FromArgb(255, 255, 0, 0);
        }

        public ulong makeKey(uint posX, uint posY)
        {
            return posX * (posY+50000);
        }

        public void DrawMouseEventsPath(GhostMouseScript gms, UpdateMouseEventsTargetStatus UpdateStatus)
        {
            // prepare colors
            SolidColorBrush brushMove = new SolidColorBrush();
            brushMove.Color = Color.FromArgb(255, 128, 128, 0);
            SolidColorBrush brushClick = new SolidColorBrush();
            brushClick.Color = Color.FromArgb(255, 0, 0, 255);

            // clear canvas, store events for later use
            canvas1.Children.Clear();
            Rectangle r = new Rectangle();
            r.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            r.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            r.Margin = new Thickness(0, 0, 0, 0);
            r.Fill = this.Background;
            canvas1.Children.Add(r);
            mouseEvents = gms.MouseEvents;
            

            // state var
            MouseEvent lastMouseEvent;

            /* draw moves */
            lastMouseEvent = gms.MouseEvents[0];
            PathFigure pathFigure = new PathFigure();
            foreach (MouseEvent mouseEvent in gms.MouseEvents)
            {
                if (mouseEvent.IsEqual(lastMouseEvent)) continue;

                LineSegment seg = new LineSegment(new Point(mouseEvent.posX, mouseEvent.posY), true);
                pathFigure.Segments.Add(seg);

                lastMouseEvent = mouseEvent;
            }
            PathFigureCollection pathFigureCollection = new PathFigureCollection();
            pathFigureCollection.Add(pathFigure);
            PathGeometry pathGeometry = new PathGeometry();
            Path p = new Path();
            p.Stroke = brushMove;
            p.StrokeThickness = 1;
            pathGeometry.Figures = pathFigureCollection;
            p.Data = pathGeometry;
            canvas1.Children.Add(p);
            if ((bool)UpdateStatus(gms, 0)) return;

            /* draw clicks */
            int i = 0;
            int ri = 0;
            hashedClickEvents = new Dictionary<ulong, MouseEvent>();
            foreach (MouseEvent mouseEvent in gms.MouseEvents)
            {
                i++;
                //if (i > 500) break;
                if (!mouseEvent.button1) continue;
                if (hashedClickEvents.ContainsKey(makeKey(mouseEvent.posX,mouseEvent.posY))) continue;

                hashedClickEvents.Add(makeKey(mouseEvent.posX , mouseEvent.posY), mouseEvent);

                r = new Rectangle();
                int strokewidth = 1;
                r.Margin = new Thickness(mouseEvent.posX, mouseEvent.posY, mouseEvent.posX + strokewidth, mouseEvent.posY + strokewidth);
                r.Height = strokewidth;
                r.Width = strokewidth;
                r.Fill = brushClick;
                ri++;
                canvas1.Children.Add(r);

                if (ri % 10 == 0)
                {
                    if ((bool)UpdateStatus(gms, i)) break;
                }
            }

        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            // Retrieve the coordinate of the mouse position.
            Point pt = e.GetPosition((UIElement)sender);

            this.Title = String.Format("{0} {1}", pt.X, pt.Y);

            // Perform the hit test against a given portion of the visual object tree.
            HitTestResult result = VisualTreeHelper.HitTest(canvas1, pt);

            // go
            uint radius = 30;
            uint startX = (uint)pt.X - radius;
            uint endX = (uint)pt.X + radius;
            uint startY = (uint)pt.Y - radius;
            uint endY = (uint)pt.Y + radius;
            List<Rectangle> newNearRectangles = new List<Rectangle>();
            for (uint thisX = startX; thisX < endX; thisX++)
            {
                for (uint thisY = startY; thisY < endY; thisY++)
                {
                    MouseEvent m = null;
                    if (hashedClickEvents.TryGetValue(makeKey(thisX , thisY), out m))
                    {
                        Rectangle r = new Rectangle();
                        int strokewidth = 6;
                        if (strokewidth < 0) strokewidth = strokewidth / (-1);

                        r.Margin = new Thickness(m.posX - (strokewidth / 2), m.posY - (strokewidth / 2), m.posX + strokewidth, m.posY + strokewidth);
                        r.Height = strokewidth;
                        r.Width = strokewidth;
                        r.Fill = brushSelected;
                        newNearRectangles.Add(r);
                    }
                }
            }
            foreach (Rectangle r in nearRectangles)
            {
                canvas1.Children.Remove(r);
            }
            nearRectangles = newNearRectangles;
            foreach (Rectangle r in nearRectangles)
            {
                canvas1.Children.Add(r);
            }

            if (result != null)
            {
                // Perform action on hit visual object.
                this.Title += " " + result.VisualHit.ToString();
                if (result.VisualHit.ToString() == "System.Windows.Shapes.Path")
                    return;

                if (selectedRectangle != null)
                {
                    canvas1.Children.Remove(selectedRectangle);
                }

                selectedMouseEvent = null;
                foreach (MouseEvent mouseEvent in mouseEvents)
                {
                    if (mouseEvent.posX == pt.X && mouseEvent.posY == pt.Y) 
                    {
                        if (mouseEvent.button1)
                        {
                            selectedMouseEvent = mouseEvent;
                            break;
                        }
                    }
                }

                if (selectedMouseEvent == null)
                    return;

                selectedRectangle = new Rectangle();
                int strokewidth = 4;
                selectedRectangle.Margin = new Thickness(selectedMouseEvent.posX, selectedMouseEvent.posY, selectedMouseEvent.posX + strokewidth, selectedMouseEvent.posY + strokewidth);
                selectedRectangle.Height = strokewidth;
                selectedRectangle.Width = strokewidth;
                selectedRectangle.Fill = brushSelected;
                canvas1.Children.Add(selectedRectangle);

            }
        }


        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the coordinate of the mouse position.
            Point pt = e.GetPosition((UIElement)sender);

            this.Title = String.Format("down: x={0} y={1}", pt.X, pt.Y);

            // Perform the hit test against a given portion of the visual object tree.
            HitTestResult result = VisualTreeHelper.HitTest(canvas1, pt);

            if (result != null)
            {
                // Perform action on hit visual object.
                //this.Title += " " + result.VisualHit.ToString();
            }
        }

    }
}
