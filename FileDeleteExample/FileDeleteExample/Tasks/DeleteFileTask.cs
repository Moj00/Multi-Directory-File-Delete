using MatthiWare.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FileDeleteExample.Tasks
{
    public class DeleteFileTask : AsyncTaskBase<TaskResult>
    {
        //public DeleteFileTask()
        //{
           
          
        //}

        public List<string> Files { get; set; } 
        public DeleteFileTask(IEnumerable<string> items)
        {
            Files = new List<string>();
            Files.AddRange(items);
        }

        /// <summary>
        /// Work here will be done asynchronously 
        /// </summary>
        protected override void DoWork()
        {
            Result = new TaskResult(); // 'Result' is the result of the actual task
            int progress = 0;

            foreach (string file in Files)
            {
                if (IsCancelled) // check if taks got cancelled
                    return;

                try
                {
                    Thread.Sleep(1000); // slow it down on purpose..

                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Result.FilesLeft.Add(file);
                }

                OnTaskProgressChanged(++progress, Files.Count);
            }
        }
    }
}
