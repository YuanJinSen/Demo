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
                case ConnectionPointType.In:    //对于进的连接点，绘制在节点的左侧：
                    rect.x = owner.rect.x - rect.width;
                    break;

                case ConnectionPointType.Out:   //对于出的连接点，绘制在节点的右侧：
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
                        //根据自己的类型来决定创建连接时参数的顺序
                        if (this.type == ConnectionPointType.In)
                            window.connections.Add(new Connection(this, window.selectingPoint));
                        else
                            window.connections.Add(new Connection(window.selectingPoint, this));

                        //连接创建结束后将SelectingPoint置为空
                        window.selectingPoint = null;
                    }
                }
            }
        }

    }
}