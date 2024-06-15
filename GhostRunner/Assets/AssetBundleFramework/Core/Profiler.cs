using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace AssetBundleFramework
{
    public class Profiler
    {
        private static readonly Stopwatch ms_Stopwatch = Stopwatch.StartNew();
        private static readonly StringBuilder ms_StringBuilder = new StringBuilder();
        private static readonly List<Profiler> ms_Stack = new List<Profiler>();

        private List<Profiler> m_Children;
        private string m_Name;
        private int m_Level;
        private long m_Timeclamp;
        private long m_Time;
        private int m_Count;

        public Profiler(string name)
        {
            m_Children = null;
            m_Name = name;
            m_Level = 0;
            m_Timeclamp = -1;
            m_Time = 0;
            m_Count = 0;
        }

        public Profiler(string name, int level) : this(name)
        {
            m_Level = level;
        }

        public Profiler CreateChild(string name)
        {
            if (m_Children == null) m_Children = new List<Profiler>();
            Profiler child = new Profiler(name, m_Level + 1);
            m_Children.Add(child);
            return child;
        }

        public void Start()
        {
            if (m_Timeclamp != -1)
            {
                throw new Exception($"{nameof(Profiler)}.{nameof(Start)} error, repeat start, name : {m_Name}");
            }
            m_Timeclamp = ms_Stopwatch.ElapsedTicks;
        }

        public void Stop()
        {
            if (m_Timeclamp == -1)
            {
                throw new Exception($"{nameof(Profiler)}.{nameof(Stop)} error, repeat stop, name : {m_Name}");
            }
            m_Time = ms_Stopwatch.ElapsedTicks - m_Timeclamp;
            m_Count += 1;
            m_Timeclamp = -1;
        }

        public void Format()
        {
            ms_StringBuilder.AppendLine();

            for (int i = 0; i <= m_Level; i++)
            {
                ms_StringBuilder.Append(i == m_Level ? "|--" : "|  ");
            }

            ms_StringBuilder.Append(m_Name);
            if (m_Count <= 0) return;
            //[Count: 1, Time: 1Ãë]
            ms_StringBuilder.Append("[");
            ms_StringBuilder.Append("Count: ");
            ms_StringBuilder.Append(m_Count);
            ms_StringBuilder.Append(", Time:");
            ms_StringBuilder.Append($"{(float)m_Time / TimeSpan.TicksPerMillisecond}ms");
            ms_StringBuilder.Append("]");
        }

        public override string ToString()
        {
            ms_StringBuilder.Clear();
            ms_Stack.Clear();
            ms_Stack.Add(this);

            while (ms_Stack.Count > 0)
            {
                int idx = ms_Stack.Count - 1;
                Profiler profiler = ms_Stack[idx];
                ms_Stack.RemoveAt(idx);

                profiler.Format();

                List<Profiler> children = profiler.m_Children;
                if (children == null) continue;
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    ms_Stack.Add(children[i]);
                }
            }

            return ms_StringBuilder.ToString();
        }
    }
}