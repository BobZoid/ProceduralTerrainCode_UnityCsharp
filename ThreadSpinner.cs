using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class ThreadSpinner : MonoBehaviour
{
    static ThreadSpinner spinner;
   Queue<ThreadInfo> dataQue = new Queue<ThreadInfo>();

    private void Awake() {
        spinner = FindObjectOfType<ThreadSpinner>(); 
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            spinner.DataThread(generateData, callback);
        };
        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();
        //lock gör så att när en Thread når denna punk så låses denna bit kod till denna Thread för att förhindra 
        //att olika Threads hamnar i konflikt
        lock (dataQue)
        {
            dataQue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    private void Update()
    {
        if (dataQue.Count > 0)
        {
            for (int i = 0; i < dataQue.Count; i++)
            {
                //Kanske bör dessa låsas med "lock"?
                ThreadInfo threadInfo = dataQue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

    }
    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
