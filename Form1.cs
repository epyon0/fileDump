using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using fileDump.Properties;

namespace fileDump
{
    public partial class Form1 : Form
    {

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
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            TreeNode currNode = null;

            foreach (DriveInfo drive in allDrives)
            {
                toolStripStatusLabel1.Text = $"Drive [{drive.Name}], Type: {drive.DriveType}";
                if (drive.IsReady)
                {
                    currNode = treeView1.Nodes.Add(drive.Name);
                    currNode.ImageIndex = 0; // disk.png
                    currNode.ToolTipText = $"[{drive.VolumeLabel}] {(float)(drive.TotalFreeSpace - drive.AvailableFreeSpace) / (float)drive.TotalFreeSpace}% Used [{drive.DriveFormat}] [{drive.DriveType}]";
                }
            }
            treeView1.SelectedNode = treeView1.Nodes[0];
            toolStripStatusLabel1.Text = "Finished Gathering Drives";
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
            toolStripStatusLabel1.Text = $"{File.GetAttributes(e.Node.FullPath).ToString()}";
            if ((File.GetAttributes(e.Node.FullPath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                enumerateDrive(e.Node);
            }
            else 
            {
                // do hex dump
                using (FileStream fs = new FileStream(e.Node.FullPath, FileMode.Open, FileAccess.Read))
                {
                    toolStripStatusLabel1.Text = $"{fs.Name} ({File.GetAttributes(e.Node.FullPath).ToString()}) [{fs.Length} Bytes]";
                    if (fs.CanRead)
                    {
                        int byteValue;
                        while ((byteValue = fs.ReadByte()) != -1)
                        {
                            richTextBox1.AppendText($"{byteValue:X2} ");
                        }
                    }
                }
            }
        }
    }
}
