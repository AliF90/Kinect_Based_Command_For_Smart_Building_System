//Ali Farahmand
//Kinnect Project
//Smart Building Lab

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Media;
using Microsoft.Kinect;

namespace KinnectProject
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        double temp = 25;
        double light;
        string ligtTXT = "MID";

        public float DistanceCalc(Joint first, Joint second)
        {
            float dX = first.Position.X - second.Position.X;
            float dY = first.Position.Y - second.Position.Y;
            float dZ = first.Position.Z - second.Position.Z;
            return (float)Math.Sqrt((dX * dX) + (dY * dY) + (dZ * dZ));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private CoordinateMapper coordinateMapper = null;
        private Body[] bodies = null;


        public MainWindow()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.kinectSensor.Open();
            this.DataContext = this;
            this.InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                foreach (Body body in this.bodies)
                {
                    if (body.IsTracked)
                    {
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        //body.Joints[JointType.HandTipRight].Position.Y
                        if (DistanceCalc(body.Joints[JointType.HandRight], body.Joints[JointType.Head]) < 0.2)
                        {
                            this.temp = Math.Round((body.Joints[JointType.HandLeft].Position.Y - body.Joints[JointType.ShoulderLeft].Position.Y)*14) + 25;
                        }

                        if (DistanceCalc(body.Joints[JointType.HandLeft], body.Joints[JointType.Head]) < 0.2)
                        {
                                this.light = Math.Round((body.Joints[JointType.HandRight].Position.Y - body.Joints[JointType.ShoulderRight].Position.Y) * 4 + 2);
                                switch (this.light)
                                {
                                    case 0: this.ligtTXT = "OFF";
                                        break;
                                    case 1: this.ligtTXT = "LOW";
                                        break;
                                    case 2:
                                        this.ligtTXT = "MID";
                                        break;
                                    case 3:
                                        this.ligtTXT = "HIGH";
                                        break;
                                    case 4:
                                        this.ligtTXT = "MAX";
                                        break;

                            }
                            
                        }
                    }
                }
            }

            TempSlider.Value = this.temp;
            TempText.Text = this.temp.ToString();

            lightSlider.Value = this.light;
            LightText.Text = this.ligtTXT;
        }
    }
}
