using FileDeleteExample.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileDeleteExample
{
    public partial class Form1 : Form
    {
        private const int MEGA_BYTES = (1024 * 1024);

        private int m_filesToGenerate = 20;
        private int m_fileSize = 10 * MEGA_BYTES; // 10 megabytes each file

        private string m_filesPath = "./Files/";

        private DeleteFileTask task;

        public Form1()
        {
            InitializeComponent();

            // assure the files dir exists
            if (!Directory.Exists(m_filesPath))
                Directory.CreateDirectory(m_filesPath);

           /// GenerateFiles();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GenerateFiles();
        }

        private void GenerateFiles()
        {
            // generate files..
            for (int i = 0; i <= m_filesToGenerate; i++)
            {
                string filePath = string.Format("{0}file_{1}.bin", m_filesPath, i);
            //    string filePath = $"{m_filesPath}file_{i}.bin";
                using (Stream s = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    s.SetLength(m_fileSize);
                }
            }

            MessageBox.Show("Files created!");
        }
        [BrowsableAttribute(false)]
        public string[] FileNames { get; set; }
        public List<string> Files { get; set; } 
   
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            
            Files.AddRange(openFileDialog1 .FileNames );
            foreach (string file in Files)
            {
                listBox1.Items.Add(file);
            }

            //SetButtonEnable(button1, false);
            //SetButtonEnable(button3, true);
            
          

        
        }
   
        private void DeleteTask_TaskCompleted(object sender, AsyncCompletedEventArgs e)
        {
         //   SetButtonEnable(button1, true);
         //   SetButtonEnable(button3, false);

            DeleteFileTask senderTask = sender as DeleteFileTask;

            if (e.Cancelled) {
                MessageBox.Show(
                    "Task got cancelled!", 
                    "Delete File Task",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);

                return;
            }

            TaskResult result = senderTask.Result;
        
        //    MessageBox.Show(  string .Format ("Done deleting the files.\n{result.FilesLeft.Count} files left.","Delete File Task", MessageBoxButtons.OK,  result.DeleteAllFilesSuccesfully ? MessageBoxIcon.Information : MessageBoxIcon.Error) );
               
                
               
              
        }

        private void SetProgressBarValue(int value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(SetProgressBarValue), value);
                return;
            }

            Progress.Value = value;

            lblProgress.Text = Convert.ToString(value) + "%";
            
        }

        private void SetButtonEnable(Button button, bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Button, bool>(SetButtonEnable), button, enabled);
                return;
            }

            button.Enabled = enabled;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (task == null)
                return;

            task.Cancel();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Files = new List<string>();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            task = new DeleteFileTask(Files);
            task.TaskProgressChanged += (o, progress) =>
            {
                SetProgressBarValue(progress.ProgressPercentage);
            };

            task.TaskCompleted += DeleteTask_TaskCompleted;

            task.Start(); // runs in different thread
        }

        private void button5_Click(object sender, EventArgs e)
        {
         
        }
    }
}
