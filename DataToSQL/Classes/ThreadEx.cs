using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataToSQL
{
    public class ThreadEx
    {
        static void ActualMethodWrapper(Action method, Action callBackMethod)
        {
            try
            {
                method.Invoke();
            }
            catch (ThreadAbortException)
            {
                //Console.WriteLine("Method aborted early");
            }
            finally
            {
                if (callBackMethod != null)
                    callBackMethod.Invoke();
            }
        }

        public static bool CallTimedOutMethodAsync(Action method, int milliseconds, Action callBackMethod = null)
        {
            bool isGood = true;
            new Thread(new ThreadStart(() =>
            {
                Thread actionThread = new Thread(new ThreadStart(() =>
                {
                    ActualMethodWrapper(method, callBackMethod);
                }));

                actionThread.Start();
                Thread.Sleep(milliseconds);
                if (actionThread.IsAlive)
                {
                    actionThread.Abort();
                    isGood = false;
                }
            })).Start();
            return isGood;
        }

        public static bool CallTimedOutMethodSync(Action method, int milliseconds)
        {
            bool isGood = true;
            var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            var task = Task.Factory.StartNew(method);

            if (!task.Wait(milliseconds, token))
                isGood = false;
            return isGood;
        }
    }
}
