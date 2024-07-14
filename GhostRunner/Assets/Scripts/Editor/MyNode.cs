using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyEditor
{
    public class MyNode
    {
        public string name = "MyNode";
        public Rect rect;
        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;
        public MyNodeEditor owner;

        private bool isDragged;

        public MyNode(MyNodeEditor owner, Vector2 pos)
        {
            this.owner = owner;
            rect = new Rect(pos.x, pos.y, 160, 40);
            inPoint = new ConnectionPoint(owner, this, ConnectionPointType.In);
            outPoint = new ConnectionPoint(owner, this, ConnectionPointType.Out);
        }

        public void Draw()
        {
            GUI.Box(rect, name);
            inPoint.Draw();
            outPoint.Draw();
        }

        public void ProcessDrag(Vector2 delta)
        {
            rect.position += delta;
        }

        //此节点处理事件，返回是否发生拖拽
        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                //鼠标按下：
                case EventType.MouseDown:
                    if (e.button == 0)//按下左键
                    {
                        if (rect.Contains(e.mousePosition)) //是否按在了节点范围内
                            isDragged = true;   //标志着进入拖拽状态

                        GUI.changed = true; //提示GUI变化
                    }
                    break;
                //鼠标松开：
                case EventType.MouseUp:
                    isDragged = false;          //标志着离开拖拽状态
                    break;
                //鼠标拖拽：
                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged) //是否按下鼠标右键且在拖拽状态
                    {
                        ProcessDrag(e.delta);   //处理拖拽
                        e.Use();                //标志着这个事件已经被处理过了，其他GUI元素之后将忽略它
                        return true;            //返回true，表示发生了拖拽
                    }
                    break;
            }
            return false;   //如果最后没有任何拖拽发生，则返回false
        }

    }
}