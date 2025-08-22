using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using fileDump.Properties;

namespace fileDump
{
    public partial class Form1 : Form
    {
        public List<byte> data = new List<byte>();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        public TreeNode workingNode;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetDrives();
            treeView1.CollapseAll();
        }
              
        private void GetDrives()
        {
            ImageList imageList = new ImageList();
            imageList.Images.Clear();
            imageList.Images.Add((Image)Properties.Resources.ResourceManager.GetObject("disk"));
            imageList.Images.Add((Image)Properties.Resources.ResourceManager.GetObject("folder"));
            imageList.Images.Add((Image)Properties.Resources.ResourceManager.GetObject("file"));

            treeView1.Nodes.Clear();
            treeView1.ImageList = imageList;

            toolStripStatusLabel1.Text = "Gathering Drives";
            statusStrip1.Update();
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            TreeNode currNode = null;

            foreach (DriveInfo drive in allDrives)
            {
                toolStripStatusLabel1.Text = $"Drive [{drive.Name}], Type: {drive.DriveType}";
                statusStrip1.Update();
                if (drive.IsReady)
                {
                    currNode = treeView1.Nodes.Add(drive.Name);
                    currNode.ImageIndex = 0; // disk.png
                    string driveSize = "0 B";
                    double tmpLong = drive.TotalSize;

                    if (drive.TotalSize < Math.Pow(1024, 1))
                    {
                        driveSize = $"{drive.TotalSize / Math.Pow(1024, 0):0.##} B";
                    }
                    else if (drive.TotalSize < Math.Pow(1024, 2))
                    {
                        driveSize = $"{drive.TotalSize / Math.Pow(1024, 1):0.##} KB";
                    }
                    else if (drive.TotalSize < Math.Pow(1024, 3))
                    {
                        driveSize = $"{drive.TotalSize / Math.Pow(1024, 2):0.##} MB";
                    }
                    else if (drive.TotalSize < Math.Pow(1024, 4))
                    {
                        driveSize = $"{drive.TotalSize / Math.Pow(1024, 3):0.##} GB";
                    }
                    else if (drive.TotalSize < Math.Pow(1024, 5))
                    {
                        driveSize = $"{drive.TotalSize / Math.Pow(1024, 4):0.##} TB";
                    }

                    currNode.ToolTipText = $"[{drive.VolumeLabel}] [{drive.DriveFormat}] [{drive.DriveType}] {driveSize}";
                }
            }
            treeView1.SelectedNode = treeView1.Nodes[0];
            toolStripStatusLabel1.Text = "Finished Gathering Drives";
            statusStrip1.Update();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {

        }

        private void enumerateDrive(TreeNode node)
        {
            if (node.Nodes.Count == 0)
            {
                try
                {
                    DirectoryInfo[] directories = new DirectoryInfo(node.FullPath).GetDirectories();
                    foreach (DirectoryInfo directory in directories)
                    {
                        try
                        {
                            TreeNode newNode = node.Nodes.Add(directory.FullName, directory.Name, 1, 1);
                            toolStripStatusLabel1.Text = $"Adding Directory: {directory.Name}";
                            statusStrip1.Update();
                        }
                        catch
                        {

                        }

                    }

                    FileInfo[] files = new DirectoryInfo(node.FullPath).GetFiles();
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            node.Nodes.Add(file.FullName, file.Name, 2, 2);
                            toolStripStatusLabel1.Text = $"Adding File: {file.Name}";
                            statusStrip1.Update();
                        }
                        catch
                        {

                        }
                    }
                } catch
                {

                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }

            toolStripStatusLabel1.Text = $"{File.GetAttributes(e.Node.FullPath).ToString()}";
            statusStrip1.Update();
            if ((File.GetAttributes(e.Node.FullPath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                enumerateDrive(e.Node);
            }
            else 
            {
                textBox1.Clear();
                textBox1.Text = $"Loading File: {e.Node.FullPath} ...";
                workingNode = e.Node;
                worker.WorkerReportsProgress = true;
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += workerReadFile;
                worker.ProgressChanged += workerProgressChanged;
                worker.RunWorkerCompleted += workerCompleted;
                if (!worker.IsBusy)
                {
                    toolStripStatusLabel1.Text = $"File: {e.Node.FullPath}";
                    statusStrip1.Update();
                    worker.RunWorkerAsync(e.Node);
                }
            }
        }
        private void workerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBox1.Clear();
            textBox1.Text = (string)e.Result;
        }
        private void workerReadFile(object sender, DoWorkEventArgs e)
        {
            TreeNode tmpNode = workingNode;
            string tmpText = "";

            try
            {
                using (FileStream fs = new FileStream(tmpNode.FullPath, FileMode.Open, FileAccess.Read))
                {
                    if (fs.CanRead)
                    {
                        int byteValue;
                        long bytesRead = 0;

                        while ((byteValue = fs.ReadByte()) != -1)
                        {

                            tmpText += $"{byteValue:X2} ";
                            worker.ReportProgress((int)(bytesRead * 100 / fs.Length));
                        }
                    }
                }
                e.Result = tmpText;
            }
            catch
            {
                e.Result = $"Cannot Access File: {tmpNode.FullPath}";
            }
        }
        private void workerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            /*
            toolStripProgressBar1.Value = e.ProgressPercentage;
            toolStripStatusLabel1.Text = $"{e.ProgressPercentage}%";
            statusStrip1.Update();
            */
        }
    }
}
