using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VsBlox.WinForms.AutoAdjustTabOrder
{
  /// <summary>
  /// Windows Forms extension methods.
  /// </summary>
  public static class FormExtensions
  {
    /// <summary>
    /// Adjusts the tab order of the controls of a Windows Form.
    /// It operates like this:
    /// 
    /// Collect all controls and sort on the Y coordinate of the mid of the control..
    /// Process controls in rows. All control with the same Y coordinate (+/- threshold)
    /// belong to the same row. Because all controls are sorted on their Y coordinate,
    /// only the Y coordinate of the previous control is significant in the row ordering.
    /// Finally all controls in the same row are sorted on their X coordinate.
    /// The tab order is from top to bottom, from left to right. Processing is recursive. 
    /// </summary>
    /// <param name="form">The windows form.</param>
    /// <example>
    /// Usage:
    /// <code>
    /// public MyForm()
    /// {
    ///     ...
    ///     Load += (sender, e) =>
    ///     {
    ///         ...
    ///         this.AdjustTabOrder();
    ///         ...
    ///     }
    ///     ...
    /// };
    /// </code>
    /// </example>
    public static void AdjustTabOrder(this Form form)
    {
      if (form.Controls.Count > 0) AdjustTabOrderHelper(form);
    }

    /// <summary>
    /// Helper function to reorder the controls tab order.
    /// </summary>
    /// <param name="rootControl">The root control.</param>
    private static void AdjustTabOrderHelper(Control rootControl)
    {
      // Verzamel alle control en sorteer op Y.
      var controls = new List<Control>();

      foreach (Control childControl in rootControl.Controls)
      {
        if (childControl.Controls.Count > 0) AdjustTabOrderHelper(childControl);

        if (childControl is TabPage) continue;

        controls.Add(childControl);
      }

      const int bandWidthY = 10;

      // Sort on midpoint of the height of the control.
      // Create rows of controls with (more or less) the same midpoint y value.
      var rows = new List<List<Control>>();
      var lastY = -bandWidthY;

      foreach (var control in controls.OrderBy(c => c.Location.Y))
      {
        var currentY = control.Location.Y;

        // Start a new row when the deviation is greater then the threshold/bandwidth.
        if (Math.Abs(currentY - lastY) >= bandWidthY || rows.Count == 0) rows.Add(new List<Control>());

        // Add control to the current row.
        rows[rows.Count - 1].Add(control);

        lastY = currentY;
      }

      // Loop through the rows.
      // For each row: sort on X
      // For X: set the correct tab order.
      var tabIndex = 0;

      foreach (var control in rows.Select(row => row.OrderBy(c => c.Location.X)).SelectMany(rowControls => rowControls))
      {
        if ((control is RichTextBox && !control.Enabled) || control is Label)
        {
          control.TabIndex = 0;
          control.TabStop = false;
          continue;
        }

        control.TabIndex = tabIndex++;
      }
    }
  }
}
