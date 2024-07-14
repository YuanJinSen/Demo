using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyEditor
{
    public enum ConnectionPointType
    {
        In,
        Out
    }

    public class ConnectionPoint
    {
        public Rect rect;
        public ConnectionPointType type;

        private MyNode owner;
        private MyNodeEditor window;

        public ConnectionPoint(MyNodeEditor window, MyNode owner, ConnectionPointType type)
        {
            this.window = window;
            this.owner = owner;
            this.type = type;
            rect = new Rect(0, 0, 10, 10);
        }

        public void Draw()
        {
            rect.y = owner.rect.y + owner.rect.height * 0.5f - rect.height * 0.5f;
            switch (type)
            {
                case ConnectionPointType.In:    //���ڽ������ӵ㣬�����ڽڵ����ࣺ
                    rect.x = owner.rect.x - rect.width;
                    break;

                case ConnectionPointType.Out:   //���ڳ������ӵ㣬�����ڽڵ���Ҳࣺ
                    rect.x = owner.rect.x + owner.rect.width;
                    break;
            }

            if (GUI.Button(rect, ""))
            {
                if (window.selectingPoint == null)
                {
                    window.selectingPoint = this;
                }
                else
                {
                    if (window.selectingPoint.type != this.type)
                    {
                        //�����Լ���������������������ʱ������˳��
                        if (this.type == ConnectionPointType.In)
                            window.connections.Add(new Connection(this, window.selectingPoint));
                        else
                            window.connections.Add(new Connection(window.selectingPoint, this));

                        //���Ӵ���������SelectingPoint��Ϊ��
                        window.selectingPoint = null;
                    }
                }
            }
        }

    }
}