using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RailMLNeural.UI.RailML.Render
{
    public class RenderContainer : FrameworkElement
    {
        private VisualCollection _children;
        //public readonly DependencyProperty VisualChildren = DependencyProperty.Register("Children", typeof(VisualCollection), typeof(RenderContainer));

        public VisualCollection Children
        {
            get { return _children; }
            set 
            {
                if (_children == value) { return; }
                _children = value;
                //SetValue(VisualChildren, value);
            }
        }
       
        public RenderContainer()
        {
            _children = new VisualCollection(this);
        }

        protected override int VisualChildrenCount
        {
            get 
            {
                //_children = (VisualCollection)GetValue(VisualChildren);
                return _children.Count; 
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            //_children = (VisualCollection)GetValue(VisualChildren);
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return _children[index];
        }
    }
}
