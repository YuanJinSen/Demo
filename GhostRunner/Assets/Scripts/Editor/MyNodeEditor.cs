using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MyEditor
{
    public class MyNodeEditor : EditorWindow
    {
        private List<MyNode> nodes;
        public List<Connection> connections;
        public ConnectionPoint selectingPoint;

        [MenuItem("Window/NodeEditor")]
        public static void OpenWindow()
        {
            GetWindow<MyNodeEditor>();
        }

        private void OnEnable()
        {
            //���ô��ڵı��⣺
            titleContent = new GUIContent("My Node Editor");

            //�����ڵ��б����
            nodes = new List<MyNode>();
            //���������б����
            connections = new List<Connection>();

            //��ʱ��Щ���ԣ�
            {
                return;
                //��ʱ��һ�������ýڵ�
                MyNode n1 = new MyNode(this, new Vector2(0, 0));
                nodes.Add(n1);
                //��ʱ�ӵڶ��������ýڵ�
                MyNode n2 = new MyNode(this, new Vector2(200, 200));
                nodes.Add(n2);

                //��ʱ��һ�����Ե�����
                connections.Add(new Connection(n2.inPoint, n1.outPoint));
            }
        }

        private void OnGUI()
        {
            DrawNodes();
            DrawConnection();
            DrawPendingConnection(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        private void DrawNodes()
        {
            foreach (var item in nodes)
            {
                item.Draw();
            }
        }

        private void DrawConnection()
        {
            foreach (var item in connections)
            {
                item.Draw();
            }
        }

        private void DrawPendingConnection(Event e)
        {
            if (selectingPoint != null)
            {
                Vector3 start = selectingPoint.type == ConnectionPointType.In ? selectingPoint.rect.center : e.mousePosition;
                Vector3 end = selectingPoint.type == ConnectionPointType.In ? e.mousePosition : selectingPoint.rect.center;

                Handles.DrawBezier(
                    start,
                    end,
                    start + Vector3.left * 50f,
                    end - Vector3.right * 50f,
                    Color.white,
                    null,
                    2f);
                GUI.changed = true;
            }
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        selectingPoint = null;
                    }
                    if (e.button == 1)
                    {
                        RightMouseMuse(e.mousePosition);
                    }
                    break;
                case EventType.MouseUp:
                    break;
            }
            //���������нڵ���¼���֮���Խ�������Ϊ�󻭵Ľڵ㽫��ʾ�ڸ��ϲ㣩
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                //����ÿ���ڵ���¼������Ƿ�������ק
                bool DragHappend = nodes[i].ProcessEvents(e);
                //����������ק����ʾGUI�����仯
                if (DragHappend)
                    GUI.changed = true;
            }
        }

        private void RightMouseMuse(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add node"), false, () => ProcessAddNode(mousePosition));
            genericMenu.ShowAsContext();
        }

        private void ProcessAddNode(Vector2 mousePosition)
        {
            nodes.Add(new MyNode(this, mousePosition));
        }
    }
}