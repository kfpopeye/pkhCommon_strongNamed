// original idea by Josh Smith from http://www.codeproject.com/Articles/16342/WPF-JoshSmith
namespace pkhCommon.WPF
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    ///     An adorner that allows adornment of one <see cref="UIElement" /> with another
    ///     <see cref="UIElement" /> .
    /// </summary>
    public class UIElementAdorner : Adorner
    {
        private readonly UIElement child;
        private double offsetLeft;
        private double offsetTop;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UIElementAdorner" /> class.
        /// </summary>
        /// <param name="adornedElement"> The adorned element. </param>
        /// <param name="childElement"> The child element to be used as the content of the adorner. </param>
        public UIElementAdorner(UIElement adornedElement, UIElement childElement)
            : base(adornedElement)
        {
            if (childElement == null)
            {
                throw new ArgumentNullException("childElement");
            }

            this.child = childElement;
            this.AddLogicalChild(childElement);
            this.AddVisualChild(childElement);
        }

        /// <summary>
        ///     Gets or sets the horizontal offset of the adorner.
        /// </summary>
        public double OffsetLeft
        {
            get
            {
                return this.offsetLeft;
            }
            set
            {
                this.offsetLeft = value;
                this.UpdateLocation();
            }
        }

        /// <summary>
        ///     Gets or sets the vertical offset of the adorner.
        /// </summary>
        public double OffsetTop
        {
            get
            {
                return this.offsetTop;
            }
            set
            {
                this.offsetTop = value;
                this.UpdateLocation();
            }
        }

        /// <summary>
        ///     Gets an enumerator for logical child elements of this element.
        /// </summary>
        /// <returns> An enumerator for logical child elements of this element. </returns>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                return new[] { this.child }.GetEnumerator();
            }
        }

        /// <summary>
        ///     Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns> The number of visual child elements for this element. </returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        ///     Returns a <see cref="Transform" /> for the adorner, based on the transform that is currently
        ///     applied to the adorned element.
        /// </summary>
        /// <param name="transform"> The transform that is currently applied to the adorned element. </param>
        /// <returns> A transform to apply to the adorner. </returns>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(this.OffsetLeft, this.OffsetTop));
            return result;
        }

        /// <summary>
        ///     Updates the location of the adorner in one atomic operation.
        /// </summary>
        /// <param name="left"> The desired left offset. </param>
        /// <param name="top"> The desired top offset </param>
        public void SetOffsets(double left, double top)
        {
            this.offsetLeft = left;
            this.offsetTop = top;
            this.UpdateLocation();
        }

        /// <summary>
        ///     When overridden in a derived class, positions child elements and determines a size for a
        ///     <see cref="FrameworkElement" /> derived class.
        /// </summary>
        /// <param name="finalSize">
        ///     The final area within the parent that this element should use to arrange
        ///     itself and its children.
        /// </param>
        /// <returns> The actual size used. </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <summary>
        ///     Overrides <see cref="GetVisualChild(System.Int32)" /> , and returns a child at the specified
        ///     index from a collection of child elements.
        /// </summary>
        /// <param name="index"> The zero-based index of the requested child element in the collection. </param>
        /// <returns>
        ///     The requested child element. This should not return null; if the provided index is out of
        ///     range, an exception is thrown.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            return this.child;
        }

        /// <summary>
        ///     Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint"> A size to constrain the adorner to. </param>
        /// <returns>
        ///     A <see cref="Size" /> object representing the amount of layout space needed by the adorner.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            this.child.Measure(constraint);
            return this.child.DesiredSize;
        }

        private void UpdateLocation()
        {
            var adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null)
            {
                adornerLayer.Update(this.AdornedElement);
            }
        }
    }
}