using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace can_analyzer_csharp
{
    // Multi-select ComboBox with checkboxes.
    //
    // Design (mirrors the Qt example mycombobox.cpp shipped with this SDK):
    //   - Looks like a normal ComboBox.
    //   - Clicking it pops up a CheckedListBox with one row per item plus a
    //     "(Select All)" row at the top.
    //   - Clicking an item toggles only that item; clicking "(Select All)"
    //     toggles every item.
    //   - The combo box text shows a summary of the current selection:
    //       - "(None)"           when nothing is selected
    //       - "(All Selected)"   when every item is selected
    //       - "A, B, C"          joined names for partial selection
    public class ComCheckBoxList : ComboBox
    {
        public delegate void CheckBoxListItemClick(object sender, ItemCheckEventArgs e);

        public event CheckBoxListItemClick ItemClick;

        // Real data: the items shown in the dropdown. We do NOT store them in
        // ComboBox.Items because the base collection drives the native dropdown
        // which we don't use.
        private List<string> itemTexts = new List<string>();
        private List<bool> checkedState = new List<bool>();
        private ToolStripDropDown dropDown;

        // Hide the base Items with a slim collection that mimics the original
        // third-party control's API (Add / Clear), so callers don't change.
        public new ItemCollection Items { get; private set; }

        // Display strings.
        private const string SelectAllLabel = "(Select All)";
        private const string NoneLabel = "(None)";
        private const string AllSelectedLabel = "(All Selected)";

        public ComCheckBoxList()
        {
            this.Items = new ItemCollection(this);
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            // Keep exactly one entry in the base Items: the summary line that
            // the ComboBox renders as its current text.
            base.Items.Add(NoneLabel);
            this.SelectedIndex = 0;
            // Use MouseDown rather than Click. With Click, the first click on an
            // unfocused ComboBox is swallowed by the framework to give the control
            // focus, and the popup only opens on the second click. MouseDown fires
            // on WM_LBUTTONDOWN regardless of focus state, so the popup opens
            // immediately on the first click.
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) ShowCheckedPopup();
            };
            // Keyboard support: Alt+Down or F4 opens the popup, Esc closes it
            // is handled by the ToolStripDropDown itself.
            this.KeyDown += (s, e) =>
            {
                if ((e.Alt && e.KeyCode == Keys.Down) || e.KeyCode == Keys.F4)
                {
                    ShowCheckedPopup();
                    e.Handled = true;
                }
            };
        }

        public bool IsItemChecked(int index)
        {
            if (index < 0 || index >= checkedState.Count) return false;
            return checkedState[index];
        }

        internal void SetItemChecked(int index, bool isChecked)
        {
            if (index < 0 || index >= checkedState.Count) return;
            checkedState[index] = isChecked;
            UpdateSummary();
        }

        // Swallow CB_SHOWDROPDOWN so the native dropdown never opens. We
        // trigger the popup from MouseDown instead, but ComboBox still emits
        // this message on WM_LBUTTONUP; returning without calling base keeps
        // the native empty list from flashing on screen.
        protected override void WndProc(ref Message m)
        {
            const int CB_SHOWDROPDOWN = 0x014F;
            if (m.Msg == CB_SHOWDROPDOWN && m.WParam != IntPtr.Zero)
            {
                return;
            }
            base.WndProc(ref m);
        }

        private void ShowCheckedPopup()
        {
            if (itemTexts.Count == 0) return;

            int width = Math.Max(this.Width, 200);
            // +1 row for the "(Select All)" entry.
            int height = Math.Min(260, (itemTexts.Count + 1) * 18 + 4);

            CheckedListBox list = new CheckedListBox
            {
                CheckOnClick = true,
                IntegralHeight = false,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(width, height),
            };

            // Row 0 is "(Select All)", checked iff every real item is checked.
            list.Items.Add(SelectAllLabel, AllItemsChecked());

            // Rows 1..N are the real items.
            for (int i = 0; i < itemTexts.Count; i++)
            {
                list.Items.Add(itemTexts[i], checkedState[i]);
            }

            // Reentrancy guard: programmatic SetItemChecked calls below trigger
            // ItemCheck too, and without this flag they would re-enter the
            // handler and cascade (e.g. unchecking one item would uncheck every
            // other item via the Select-All branch).
            bool suppress = false;

            list.ItemCheck += (s, e) =>
            {
                if (suppress) return;

                if (e.Index == 0)
                {
                    // "(Select All)" clicked: flip every real item.
                    bool newCheck = e.NewValue == CheckState.Checked;
                    // Capture the event to a local before the null check so a
                    // concurrent unsubscribe can't turn it null between the
                    // check and the invocation. Also avoids C# 6.0 null-
                    // conditional syntax, keeping the file buildable on the
                    // C# 4.0 compiler shipped with VS 2010.
                    CheckBoxListItemClick handler = ItemClick;
                    for (int i = 0; i < checkedState.Count; i++)
                    {
                        if (checkedState[i] != newCheck)
                        {
                            checkedState[i] = newCheck;
                            if (handler != null)
                            {
                                handler(this, new ItemCheckEventArgs(
                                    i,
                                    newCheck ? CheckState.Checked : CheckState.Unchecked,
                                    newCheck ? CheckState.Unchecked : CheckState.Checked));
                            }
                        }
                    }
                    // Sync the visible checkboxes on the next message loop pass.
                    // Calling SetItemChecked from inside ItemCheck can re-enter
                    // the event and confuse the CheckedListBox state machine.
                    // The callback may run after the popup was closed and the
                    // list was disposed (user clicked Select All then immediately
                    // clicked outside); guard against that.
                    list.BeginInvoke((Action)(() =>
                    {
                        if (list.IsDisposed) return;
                        suppress = true;
                        try
                        {
                            for (int i = 1; i < list.Items.Count; i++)
                            {
                                list.SetItemChecked(i, newCheck);
                            }
                        }
                        finally { suppress = false; }
                    }));
                }
                else
                {
                    // A real item was clicked. e.Index includes the "(Select All)"
                    // row at position 0, so shift by one to map to our data.
                    int realIndex = e.Index - 1;
                    // Defensive: e.Index should always be in [1, itemTexts.Count]
                    // because that's how we filled the list, but guard anyway so
                    // a future code change can't crash here with an out-of-range
                    // access on checkedState / itemTexts.
                    if (realIndex < 0 || realIndex >= checkedState.Count) return;
                    checkedState[realIndex] = e.NewValue == CheckState.Checked;
                    CheckBoxListItemClick handler = ItemClick;
                    if (handler != null)
                    {
                        handler(this, new ItemCheckEventArgs(realIndex, e.NewValue, e.CurrentValue));
                    }

                    // Keep "(Select All)" in sync: check it when every real item
                    // is checked, uncheck it otherwise. Run on the next message
                    // loop pass for the same reason as above, and guard with
                    // suppress so the triggered ItemCheck doesn't cascade.
                    list.BeginInvoke((Action)(() =>
                    {
                        if (list.IsDisposed) return;
                        bool allChecked = AllItemsChecked();
                        if (list.GetItemChecked(0) != allChecked)
                        {
                            suppress = true;
                            try { list.SetItemChecked(0, allChecked); }
                            finally { suppress = false; }
                        }
                    }));
                }
            };

            // Both the host and the dropdown default to AutoSize=true, which
            // collapses the hosted control to its PreferredSize and produces
            // a tiny or invisible popup. Turn it off and pin the sizes.
            ToolStripControlHost host = new ToolStripControlHost(list)
            {
                Margin = new Padding(0),
                Padding = new Padding(0),
                AutoSize = false,
                Size = new Size(width, height),
            };

            // Defensive cleanup of a previous popup that somehow stayed alive.
            if (dropDown != null)
            {
                dropDown.Closed -= OnDropDownClosed;
                dropDown.Dispose();
                dropDown = null;
            }

            dropDown = new ToolStripDropDown
            {
                Padding = new Padding(0),
                AutoSize = false,
                Size = new Size(width, height),
            };
            dropDown.Items.Add(host);
            dropDown.Closed += OnDropDownClosed;
            // Without explicit focus, the first click inside the popup just
            // focuses the CheckedListBox and doesn't toggle the checkbox;
            // the user has to click twice. Force focus on open so the first
            // click toggles immediately.
            dropDown.Opened += (s, e) => list.Focus();

            // Place the popup below the control; if it would overflow the
            // screen bottom, flip it to above.
            Point below = this.PointToScreen(new Point(0, this.Height));
            Point above = this.PointToScreen(new Point(0, 0));
            above.Y -= height;
            Screen screen = Screen.FromPoint(below);
            Point pt = (below.Y + height > screen.WorkingArea.Bottom) ? above : below;
            dropDown.Show(pt);
        }

        private void OnDropDownClosed(object sender, EventArgs e)
        {
            UpdateSummary();
            // Do NOT Dispose the dropdown inline. ToolStripDropDown still has
            // in-flight window messages at this point and disposing synchronously
            // raises ObjectDisposedException. Push the dispose to the next loop.
            // Capture the field into a local: if the user reopens the popup
            // before this callback runs, ShowCheckedPopup assigns a new
            // ToolStripDropDown to dropDown, and we must not dispose the new one.
            ToolStripDropDown dd = dropDown;
            dropDown = null;
            this.BeginInvoke((Action)(() =>
            {
                if (dd != null)
                {
                    dd.Closed -= OnDropDownClosed;
                    dd.Dispose();
                }
            }));
        }

        private void UpdateSummary()
        {
            string summary;
            if (checkedState.Count == 0)
            {
                summary = NoneLabel;
            }
            else if (AllItemsChecked())
            {
                summary = AllSelectedLabel;
            }
            else
            {
                List<string> selected = new List<string>();
                for (int i = 0; i < itemTexts.Count; i++)
                {
                    if (checkedState[i]) selected.Add(itemTexts[i]);
                }
                summary = selected.Count == 0 ? NoneLabel : string.Join(", ", selected);
            }
            if (base.Items.Count == 0)
                base.Items.Add(summary);
            else
                base.Items[0] = summary;
            this.SelectedIndex = 0;
        }

        private bool AllItemsChecked()
        {
            foreach (bool c in checkedState) { if (!c) return false; }
            return checkedState.Count > 0;
        }

        public class ItemCollection
        {
            private ComCheckBoxList owner;

            public ItemCollection(ComCheckBoxList owner)
            {
                this.owner = owner;
            }

            public void Add(string item)
            {
                owner.itemTexts.Add(item);
                owner.checkedState.Add(false);
                owner.UpdateSummary();
            }

            public void Clear()
            {
                owner.itemTexts.Clear();
                owner.checkedState.Clear();
                owner.UpdateSummary();
            }

            public int Count { get { return owner.itemTexts.Count; } }

            public string this[int index] { get { return owner.itemTexts[index]; } }
        }
    }
}
