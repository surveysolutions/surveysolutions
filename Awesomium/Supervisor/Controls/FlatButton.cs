using System.Drawing;
using System.Windows.Forms;

namespace Browsing.Supervisor.Controls
{
    public partial class FlatButton : Button
    {
        #region Constructor

        public FlatButton()
            : base()
        {
            FlatAppearance.BorderSize = 0;
            FlatStyle = FlatStyle.Flat;
        }

        #endregion

        #region Methods

        public override void NotifyDefault(bool value)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen pen = new Pen(FlatAppearance.BorderColor, 1);
            Rectangle rectangle = new Rectangle(0, 0, Size.Width - 1, Size.Height - 1);
            e.Graphics.DrawRectangle(pen, rectangle);
        }

        #endregion
    }
}
