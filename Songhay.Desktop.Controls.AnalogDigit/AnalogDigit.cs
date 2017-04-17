using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Songhay.Desktop.Controls
{
    /// <summary>
    /// Defines a “rolling digit” used by analog computers
    /// such as the ones used by gasoline pumps in the 1940s.
    /// </summary>
    /// <remarks>
    /// This custom control pattern is based on an article by Emil Stoychev:
    /// “Creating a Silverlight Custom Control - The Basics”
    /// [http://www.silverlightshow.net/items/Creating-a-Silverlight-Custom-Control-The-Basics.aspx]
    /// </remarks>
    public class AnalogDigit : Control
    {
        /// <summary>
        /// Initializes the <see cref="AnalogDigit"/> class.
        /// </summary>
        static AnalogDigit()
        {
            InnerCanvasTopPositions = new double[10];
            InnerCanvasTopPositions[0] = 0;
            InnerCanvasTopPositions[1] = -51;
            InnerCanvasTopPositions[2] = -102;
            InnerCanvasTopPositions[3] = -153;
            InnerCanvasTopPositions[4] = -204;
            InnerCanvasTopPositions[5] = -255;
            InnerCanvasTopPositions[6] = -306;
            InnerCanvasTopPositions[7] = -357;
            InnerCanvasTopPositions[8] = -408;
            InnerCanvasTopPositions[9] = -459;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogDigit"/> class.
        /// </summary>
        public AnalogDigit()
            : base()
        {
            this.InnerCanvasTopPosition = 0;
            base.DefaultStyleKey = typeof(AnalogDigit);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code
        /// or internal processes (such as a rebuilding layout pass)
        /// call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>.
        /// In simplest terms, this means the method is called just before a UI element displays in an application.
        /// For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //TODO: re-factor into Extension Method.
            this._layoutRoot = this.GetTemplateChild("LayoutRoot") as Viewbox;
            Debug.Assert(this._layoutRoot != null, "The expected template child is not here.");
        }

        public static double[] InnerCanvasTopPositions { get; private set; }

        #region dependency properties:

        /// <summary>
        /// Registers member <see cref="AnalogDigit.DigitValue"/> as a Dependency Property.
        /// </summary>
        [Description("Gets or sets the digit value for the rolling digits.")]
        public static readonly DependencyProperty DigitValueProperty = DependencyProperty.Register(
            "DigitValue",
            typeof(byte),
            typeof(AnalogDigit),
            new PropertyMetadata(default(byte), new PropertyChangedCallback(AnalogDigit.OnDigitValuePropertyChange))
        );

        static void OnDigitValuePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var currentControl = d as AnalogDigit;
            var old = Convert.ToDouble(e.OldValue);
            var @new = Convert.ToDouble(e.NewValue);
            currentControl.DoAnimation(old, @new, "InnerCanvasTopPosition");
        }

        /// <summary>
        /// Gets or sets the digit value.
        /// </summary>
        /// <value>The digit value.</value>
        [Description("Gets or sets the digit value for the rolling digits.")]
        public byte DigitValue
        {
            get { return (byte)this.GetValue(DigitValueProperty); }
            set { base.SetValue(DigitValueProperty, value); }
        }

        /// <summary>
        /// Registers member <see cref="AnalogDigit.Fill"/> as a Dependency Property.
        /// </summary>
        [Description("Gets or sets the fill for the rolling digits.")]
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill",
            typeof(Brush),
            typeof(AnalogDigit),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)))
        );

        /// <summary>
        /// Gets or sets the fill for the rolling digits.
        /// </summary>
        /// <value>The fill.</value>
        [Description("Gets or sets the fill for the rolling digits.")]
        public Brush Fill
        {
            get { return (Brush)this.GetValue(FillProperty); }
            set { base.SetValue(FillProperty, value); }
        }

        /// <summary>
        /// Registers member <see cref="AnalogDigit.InnerCanvasTopPosition"/> as a Dependency Property.
        /// </summary>
        public static readonly DependencyProperty InnerCanvasTopPositionProperty = DependencyProperty.Register(
            "InnerCanvasTopPosition",
            typeof(double),
            typeof(AnalogDigit),
            new PropertyMetadata(default(double))
        );

        /// <summary>
        /// Gets or sets the strip top.
        /// </summary>
        /// <value>The strip top.</value>
        public double InnerCanvasTopPosition
        {
            get { return (double)this.GetValue(InnerCanvasTopPositionProperty); }
            set { base.SetValue(InnerCanvasTopPositionProperty, value); }
        }

        #endregion

        void DoAnimation(double fromValue, double toValue, string propertyPath)
        {
            if (fromValue < 0) return;
            if (InnerCanvasTopPositions.Length <= fromValue) return;
            if (toValue < 0) return;
            if (InnerCanvasTopPositions.Length <= toValue) return;

            //Convert this.DigitValue to inner Canvas Top value:
            fromValue = InnerCanvasTopPositions[(int)fromValue];
            toValue = InnerCanvasTopPositions[(int)toValue];

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.InnerCanvasTopPosition = toValue;
                return;
            }

            DoubleAnimation animation;

            if (this._story == null)
            {
                this._story = new Storyboard();

                //TODO: register configurable duration and easing function properties.
                animation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(1)),
                    From = fromValue,
                    To = toValue,
                };

                Storyboard.SetTarget(animation, this);
                Storyboard.SetTargetProperty(animation, new PropertyPath(propertyPath));
                this._story.Children.Add(animation);
            }
            else
            {
                animation = this._story.Children.OfType<DoubleAnimation>().First();
                animation.From = fromValue;
                animation.To = toValue;
            }
            this._story.Begin();
        }

        Viewbox _layoutRoot;
        Storyboard _story;
    }
}
