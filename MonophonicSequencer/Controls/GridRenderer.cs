using MonophonicSequencer.Resources;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MonophonicSequencer.Controls
{
    public abstract class Renderer<T> where T : UIElement
    {
        protected T element;
        public Renderer(T uiElement) => element = uiElement;
        public abstract void OnRender(DrawingContext dc);
    }

    public class GridRenderer : Renderer<Grid>
    {
        private static readonly HashSet<int> blackKeys = new HashSet<int> { 1, 3, 5, 8, 10 };

        private double actualWidth => element.ActualWidth;
        private double actualHeight => element.ActualHeight;
        private RowDefinitionCollection rowDefinitions => element.RowDefinitions;
        private ColumnDefinitionCollection columnDefinitions => element.ColumnDefinitions;


        public GridRenderer(Grid uiElement) : base(uiElement) { }

        public override void OnRender(DrawingContext dc)
        {
            RenderBlackKeys(dc);
            RenderLightGrayLine(dc);
            RenderGrayLine(dc);
            RenderBlackLine(dc);

            var rect = new Rect(0, 0, actualWidth, actualHeight);
            dc.DrawRectangle(Brushes.Transparent, Pens.Black, rect); // 全体囲む四角 黒
        }

        private void RenderBlackKeys(DrawingContext dc) // 黒鍵 極薄グレー
        {
            var index = 0;
            foreach(var row in rowDefinitions)
            {
                if(!blackKeys.Contains((index++ % 12) - 1)) continue;

                var rect = new Rect(0, row.Offset, actualWidth, row.ActualHeight);
                dc.DrawRectangle(Brushes.WhiteSmoke, Pens.Transparent, rect);
            }
        }
        private void RenderLightGrayLine(DrawingContext dc) // 基本区切り線 薄グレー
        {
            foreach(var row in rowDefinitions)
                DrawLine(dc, Pens.LightGray, 0, row.Offset, actualWidth, row.Offset);

            foreach(var column in columnDefinitions)
                DrawLine(dc, Pens.LightGray, column.Offset, 0, column.Offset, actualHeight);
        }
        private void RenderGrayLine(DrawingContext dc) // 4分音符区切り線 グレー
        {
            var index = 0;
            foreach(var column in columnDefinitions)
            {
                if(index++ % 4 != 0) continue;

                DrawLine(dc, Pens.Gray, column.Offset, 0, column.Offset, actualHeight);
            }
        }
        private void RenderBlackLine(DrawingContext dc) // 小節・オクターブ区切り線 黒
        {
            var index = 0;
            foreach(var column in columnDefinitions)
            {
                if(index++ % 16 != 0) continue;

                DrawLine(dc, Pens.Black, column.Offset, 0, column.Offset, actualHeight);
            }

            index = 0;
            foreach(var row in rowDefinitions)
            {
                if(index++ % 12 != 1) continue;

                DrawLine(dc, Pens.Black, 0, row.Offset, actualWidth, row.Offset);
            }
        }
        private void DrawLine(DrawingContext dc, Pen pen, double x1, double x2, double y1, double y2)
            => dc.DrawLine(pen, new Point(x1, x2), new Point(y1, y2));
    }
}
