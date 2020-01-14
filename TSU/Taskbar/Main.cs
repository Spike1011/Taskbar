using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace TSU
{
    public partial class Main : Form
    {
        public static bool pause = false;
        static Thread myThread;

        public Main()
        {
            InitializeComponent();

            //AllocDebug();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            myThread = new Thread(For_thread);
            myThread.Start();

            //using (var serviceController = new ServiceController("TSU"))
            //{
            //    serviceController.Start();
            //    serviceController.WaitForStatus(ServiceControllerStatus.Running);
            //}
        }


        private void MenuItem1_Click(object Sender, EventArgs e)
        {
            myThread.Abort();
            Application.Exit();
        }

        private static void For_thread()
        {
            Boss p = new Boss();
            p.Mod_TCP();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            //using (var serviceController = new ServiceController("TSU"))
            //{
            //    serviceController.Close();
            //    serviceController.WaitForStatus(ServiceControllerStatus.Running);
            //}
            ModBusTcp.Visible = false;
            ModBusTcp.Icon = null;
            Environment.Exit(0);
        }

        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pause)
            {
                //using (var serviceController = new ServiceController("TSU"))
                //{
                //    serviceController.Close();
                //    serviceController.WaitForStatus(ServiceControllerStatus.Running);
                //}
                pause = false;
                startToolStripMenuItem.Text = "Stop";
            }
            else
            {
                //using (var serviceController = new ServiceController("TSU"))
                //{
                //    serviceController.Start();
                //    serviceController.WaitForStatus(ServiceControllerStatus.Running);
                //}
                pause = true;
                startToolStripMenuItem.Text = "Start";
            }
        }
    }
}
