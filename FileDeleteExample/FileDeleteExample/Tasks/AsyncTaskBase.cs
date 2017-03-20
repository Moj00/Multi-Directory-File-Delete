/*
MIT License

Copyright (c) 2016 MatthiWare

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

https://github.com/MatthiWare/UpdateLib/blob/master/UpdateLib/UpdateLib/Tasks/AsyncTaskBase.cs
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace MatthiWare.Tasks
{
    /// <summary>
    /// Base class for all Tasks that need to be run Async
    /// </summary>
    public abstract class AsyncTaskBase
    {

        #region private fields

#if DEBUG
        public Stopwatch m_sw = new Stopwatch();
#endif

        private readonly Queue<WaitHandle> waitQueue = new Queue<WaitHandle>();
        private WaitHandle mainWait;
        private readonly object sync = new object();

        private bool cancelled = false;

        #endregion

        #region events

        /// <summary>
        /// Raises when this <see cref="AsyncTaskBase"/> is completed. 
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> TaskCompleted;
        /// <summary>
        /// Raises when the <see cref="AsyncTaskBase"/> progress changed. 
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> TaskProgressChanged;

        #endregion

        #region properties

        /// <summary>
        /// Gets if the current <see cref="AsyncTaskBase"/> is cancelled. 
        /// </summary>
        public bool IsCancelled
        {
            get
            {
                lock (sync)
                    return cancelled;
            }
        }

        #endregion
        

        /// <summary>
        /// Starts the task
        /// </summary>
        public void Start()
        {
            Exception taskException = null;

            Action worker = new Action(() =>
            {
                try
                {
                    DoWork();
                }
                catch (Exception ex)
                {
                    taskException = ex;
                    Console.WriteLine(ex);
                }
                finally
                {
                    AwaitWorkers();
                }
            });

#if DEBUG
            m_sw.Reset();
            m_sw.Start();
#endif

            mainWait = worker.BeginInvoke(new AsyncCallback((IAsyncResult r) =>
            {
#if DEBUG
                m_sw.Stop();
#endif
                worker.EndInvoke(r);
#if DEBUG
                Console.WriteLine($"Completed in {m_sw.ElapsedMilliseconds}ms");
#endif
                OnTaskCompleted(taskException, IsCancelled);

            }), null).AsyncWaitHandle;
        }

        /// <summary>
        /// The worker method.
        /// </summary>
        protected abstract void DoWork();

        /// <summary>
        /// Cancels the current <see cref="AsyncTaskBase"/>
        /// Check <see cref="IsCancelled"/> in the worker code to see if the <see cref="AsyncTaskBase"/> got cancelled.  
        /// </summary>
        public virtual void Cancel()
        {
            lock (sync)
                cancelled = true;
        }

        /// <summary>
        /// Adds a new wait object to the queue
        /// </summary>
        /// <param name="waitHandle">The wait object</param>
        public void Enqueue(WaitHandle waitHandle)
        {
            lock (sync)
                waitQueue.Enqueue(waitHandle);
        }

        /// <summary>
        /// Blocks the calling thread until the complete task is done.
        /// DO NOT call this in the worker method use <see cref="AwaitWorkers"/> method instead. 
        /// </summary>
        public void AwaitTask()
        {
            if (mainWait != null)
            {
                mainWait.WaitOne();
                mainWait.Close();
                mainWait = null;
            }
        }

        /// <summary>
        /// Blocks the calling thread until all the workers are done.
        /// </summary>
        protected void AwaitWorkers()
        {
            while (waitQueue.Count > 0)
            {
                WaitHandle wh = null;
                lock (sync)
                    wh = waitQueue.Dequeue();

                wh.WaitOne();
                wh.Close();
            }
        }

        /// <summary>
        /// Raises the <see cref="TaskProgressChanged"/> event.  
        /// </summary>
        /// <param name="done">The amount of work that is done.</param>
        /// <param name="total">The total amount of work.</param>
        protected virtual void OnTaskProgressChanged(int done, int total)
        {
            int progress = (done * 100) / total;
                if (TaskProgressChanged!=null)
      TaskProgressChanged(this, new ProgressChangedEventArgs(progress, null));
           // TaskProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress, null));
        }

        /// <summary>
        /// Raises the <see cref="TaskProgressChanged"/> event.  
        /// </summary>
        /// <param name="percent">The percentage of work that is done.</param>
        protected virtual void OnTaskProgressChanged(int percent)
        {
                if (TaskProgressChanged!=null)
      TaskProgressChanged(this, new ProgressChangedEventArgs(percent, null));
            //TaskProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percent, null));
        }

        /// <summary>
        /// Raises the <see cref="TaskProgressChanged"/> event.  
        /// </summary>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> event.</param>
        protected virtual void OnTaskProgressChanged(ProgressChangedEventArgs e)
        {
             if (TaskProgressChanged!=null)
      TaskProgressChanged(this, e);
          //  TaskProgressChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="TaskCompleted"/> event. 
        /// </summary>
        /// <param name="e">If an <see cref="Exception"/> occured pass the <see cref="Exception"/> object.</param>
        /// <param name="cancelled">Indicates whether the <see cref="AsyncTaskBase"/> got cancelled.</param>
        protected virtual void OnTaskCompleted(Exception e, bool cancelled = false)
        {
              if (TaskCompleted!=null)
      TaskCompleted(this, new AsyncCompletedEventArgs(e, cancelled, null));
        //    TaskCompleted?.Invoke(this, new AsyncCompletedEventArgs(e, cancelled, null));
        }

        /// <summary>
        /// Raises the <see cref="TaskCompleted"/> event. 
        /// </summary>
        /// <param name="e">The <see cref="AsyncCompletedEventArgs"/> event.</param>
        protected virtual void OnTaskCompleted(AsyncCompletedEventArgs e)
        {
                if (TaskCompleted!=null)
      TaskCompleted(this, e);
            //TaskCompleted?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Base class for all Tasks that need to be run Async
    /// </summary>
    /// <typeparam name="T">The type of the Result object</typeparam>
    public abstract class AsyncTaskBase<T> : AsyncTaskBase
    {
        /// <summary>
        /// Gets or sets the result <see cref="T"/> 
        /// </summary>
        public virtual T Result { get; protected set; }

        /// <summary>
        /// Blocks the calling thread until the complete task is done.
        /// DO NOT call this in the worker method use <see cref="AsyncTaskBase.AwaitWorkers"/> method instead. 
        /// </summary>
        /// <returns><see cref="Result"/></returns>
        public new T AwaitTask()
        {
            base.AwaitTask();
            return Result;
        }
    }
}
