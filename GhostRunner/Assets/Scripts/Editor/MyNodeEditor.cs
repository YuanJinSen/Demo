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
            //设置窗口的标题：
            titleContent = new GUIContent("My Node Editor");

            //创建节点列表对象
            nodes = new List<MyNode>();
            //创建连接列表对象
            connections = new List<Connection>();

            //临时加些测试：
            {
                return;
                //临时加一个测试用节点
                MyNode n1 = new MyNode(this, new Vector2(0, 0));
                nodes.Add(n1);
                //临时加第二个测试用节点
                MyNode n2 = new MyNode(this, new Vector2(200, 200));
                nodes.Add(n2);

                //临时加一个测试的连线
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
            //降序处理所有节点的事件（之所以降序是因为后画的节点将显示在更上层）
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                //处理每个节点的事件并看是否发生了拖拽
                bool DragHappend = nodes[i].ProcessEvents(e);
                //若发生了拖拽则提示GUI发生变化
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