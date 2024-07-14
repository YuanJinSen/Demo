using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MyEditor
{
    //连接点之间的连线
    public class Connection
    {
        public ConnectionPoint inPoint;     //进点
        public ConnectionPoint outPoint;    //出点

        //构造函数
        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
        }

        public void Draw()
        {
            Handles.DrawBezier(     //绘制通过给定切线的起点和终点的纹理化贝塞尔曲线
            inPoint.rect.center,    //startPosition	贝塞尔曲线的起点。
            outPoint.rect.center,   //endPosition	贝塞尔曲线的终点。
            inPoint.rect.center + Vector2.left * 50f,   //startTangent	贝塞尔曲线的起始切线。
            outPoint.rect.center - Vector2.left * 50f,  //endTangent	贝塞尔曲线的终点切线。
            Color.white,        //color	    要用于贝塞尔曲线的颜色。
            null,               //texture	要用于绘制贝塞尔曲线的纹理。
            2f                  //width	    贝塞尔曲线的宽度。
            );
        }
    }
}