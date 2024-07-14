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

        //�˽ڵ㴦���¼��������Ƿ�����ק
        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                //��갴�£�
                case EventType.MouseDown:
                    if (e.button == 0)//�������
                    {
                        if (rect.Contains(e.mousePosition)) //�Ƿ����˽ڵ㷶Χ��
                            isDragged = true;   //��־�Ž�����ק״̬

                        GUI.changed = true; //��ʾGUI�仯
                    }
                    break;
                //����ɿ���
                case EventType.MouseUp:
                    isDragged = false;          //��־���뿪��ק״̬
                    break;
                //�����ק��
                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged) //�Ƿ�������Ҽ�������ק״̬
                    {
                        ProcessDrag(e.delta);   //������ק
                        e.Use();                //��־������¼��Ѿ���������ˣ�����GUIԪ��֮�󽫺�����
                        return true;            //����true����ʾ��������ק
                    }
                    break;
            }
            return false;   //������û���κ���ק�������򷵻�false
        }

    }
}