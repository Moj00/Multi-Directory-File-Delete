using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileDeleteExample.Tasks
{
    public class TaskResult
    {
public TaskResult ()
{
    FilesLeft = new List<string>();
}
        public List<string> FilesLeft { get; set; }

        public bool DeleteAllFilesSuccesfully { get { return FilesLeft.Count == 0; } }
    }
}
