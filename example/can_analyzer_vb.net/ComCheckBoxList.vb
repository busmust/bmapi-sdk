Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms

' Multi-select ComboBox with checkboxes. The control keeps the same user
' behavior as the C# CAN Analyzer without requiring an external control assembly.
Public Class ComCheckBoxList
    Inherits ComboBox

    Public Event ItemClick As EventHandler(Of ItemCheckEventArgs)

    Private ReadOnly itemTexts As New List(Of String)()
    Private ReadOnly checkedState As New List(Of Boolean)()
    Private ReadOnly customItems As ItemCollection
    Private popup As ToolStripDropDown

    Private Const SelectAllLabel As String = "(Select All)"
    Private Const NoneLabel As String = "(None)"
    Private Const AllSelectedLabel As String = "(All Selected)"

    Public Sub New()
        customItems = New ItemCollection(Me)
        DropDownStyle = ComboBoxStyle.DropDownList
        MyBase.Items.Add(NoneLabel)
        SelectedIndex = 0
        AddHandler MouseDown, AddressOf OnComboMouseDown
        AddHandler KeyDown, AddressOf OnComboKeyDown
    End Sub

    Public Shadows ReadOnly Property Items As ItemCollection
        Get
            Return customItems
        End Get
    End Property

    Public Function IsItemChecked(index As Integer) As Boolean
        If index < 0 OrElse index >= checkedState.Count Then
            Return False
        End If
        Return checkedState(index)
    End Function

    Public Sub SetItemChecked(index As Integer, isChecked As Boolean)
        If index < 0 OrElse index >= checkedState.Count Then
            Return
        End If
        checkedState(index) = isChecked
        UpdateSummary()
    End Sub

    Protected Overrides Sub WndProc(ByRef message As Message)
        Const CB_SHOWDROPDOWN As Integer = &H14F
        If message.Msg = CB_SHOWDROPDOWN AndAlso message.WParam <> IntPtr.Zero Then
            Return
        End If
        MyBase.WndProc(message)
    End Sub

    Private Sub OnComboMouseDown(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            ShowCheckedPopup()
        End If
    End Sub

    Private Sub OnComboKeyDown(sender As Object, e As KeyEventArgs)
        If (e.Alt AndAlso e.KeyCode = Keys.Down) OrElse e.KeyCode = Keys.F4 Then
            ShowCheckedPopup()
            e.Handled = True
        End If
    End Sub

    Private Sub ShowCheckedPopup()
        If itemTexts.Count = 0 Then
            Return
        End If

        Dim popupWidth As Integer = Math.Max(Width, 200)
        Dim popupHeight As Integer = 260
        Dim list As New CheckedListBox() With {
            .Font = Font,
            .CheckOnClick = True,
            .IntegralHeight = False,
            .BorderStyle = BorderStyle.FixedSingle,
            .Size = New Size(popupWidth, popupHeight)
        }

        ' Keep the visible client area an integral number of actual rows.
        list.CreateControl()
        Dim borderHeight As Integer = list.Height - list.ClientSize.Height
        Dim visibleRows As Integer = Math.Min(itemTexts.Count + 1, Math.Max(1, (popupHeight - borderHeight) \ list.ItemHeight))
        popupHeight = visibleRows * list.ItemHeight + borderHeight
        list.Height = popupHeight

        list.Items.Add(SelectAllLabel, AllItemsChecked())
        For i As Integer = 0 To itemTexts.Count - 1
            list.Items.Add(itemTexts(i), checkedState(i))
        Next

        Dim suppress As Boolean = False
        AddHandler list.ItemCheck,
            Sub(sender As Object, e As ItemCheckEventArgs)
                If suppress Then
                    Return
                End If

                If e.Index = 0 Then
                    Dim newCheck As Boolean = e.NewValue = CheckState.Checked
                    For i As Integer = 0 To checkedState.Count - 1
                        If checkedState(i) <> newCheck Then
                            checkedState(i) = newCheck
                            RaiseEvent ItemClick(
                                Me,
                                New ItemCheckEventArgs(
                                    i,
                                    If(newCheck, CheckState.Checked, CheckState.Unchecked),
                                    If(newCheck, CheckState.Unchecked, CheckState.Checked)))
                        End If
                    Next

                    list.BeginInvoke(New Action(
                        Sub()
                            If list.IsDisposed Then
                                Return
                            End If
                            suppress = True
                            Try
                                For i As Integer = 1 To list.Items.Count - 1
                                    list.SetItemChecked(i, newCheck)
                                Next
                            Finally
                                suppress = False
                            End Try
                        End Sub))
                Else
                    Dim realIndex As Integer = e.Index - 1
                    If realIndex < 0 OrElse realIndex >= checkedState.Count Then
                        Return
                    End If

                    checkedState(realIndex) = e.NewValue = CheckState.Checked
                    RaiseEvent ItemClick(
                        Me,
                        New ItemCheckEventArgs(realIndex, e.NewValue, e.CurrentValue))

                    list.BeginInvoke(New Action(
                        Sub()
                            If list.IsDisposed Then
                                Return
                            End If
                            Dim allChecked As Boolean = AllItemsChecked()
                            If list.GetItemChecked(0) <> allChecked Then
                                suppress = True
                                Try
                                    list.SetItemChecked(0, allChecked)
                                Finally
                                    suppress = False
                                End Try
                            End If
                        End Sub))
                End If
            End Sub

        Dim host As New ToolStripControlHost(list) With {
            .Margin = New Padding(0),
            .Padding = New Padding(0),
            .AutoSize = False,
            .Size = New Size(popupWidth, popupHeight)
        }

        If popup IsNot Nothing Then
            RemoveHandler popup.Closed, AddressOf HandlePopupClosed
            popup.Dispose()
            popup = Nothing
        End If

        popup = New ToolStripDropDown() With {
            .Padding = New Padding(0),
            .AutoSize = False,
            .Size = New Size(popupWidth, popupHeight)
        }
        popup.Items.Add(host)
        AddHandler popup.Closed, AddressOf HandlePopupClosed
        AddHandler popup.Opened, Sub(sender As Object, e As EventArgs) list.Focus()

        Dim below As Point = PointToScreen(New Point(0, Height))
        Dim above As Point = PointToScreen(New Point(0, 0))
        above.Y -= popupHeight
        Dim screen As Screen = Screen.FromPoint(below)
        Dim popupPoint As Point = If(
            below.Y + popupHeight > screen.WorkingArea.Bottom,
            above,
            below)
        popup.Show(popupPoint)
    End Sub

    Private Sub HandlePopupClosed(sender As Object, e As EventArgs)
        UpdateSummary()
        Dim closedPopup As ToolStripDropDown = popup
        popup = Nothing
        BeginInvoke(New Action(
            Sub()
                If closedPopup IsNot Nothing Then
                    RemoveHandler closedPopup.Closed, AddressOf HandlePopupClosed
                    closedPopup.Dispose()
                End If
            End Sub))
    End Sub

    Private Sub UpdateSummary()
        Dim summary As String
        If checkedState.Count = 0 Then
            summary = NoneLabel
        ElseIf AllItemsChecked() Then
            summary = AllSelectedLabel
        Else
            Dim selected As New List(Of String)()
            For i As Integer = 0 To itemTexts.Count - 1
                If checkedState(i) Then
                    selected.Add(itemTexts(i))
                End If
            Next
            summary = If(selected.Count = 0, NoneLabel, String.Join(", ", selected.ToArray()))
        End If

        If MyBase.Items.Count = 0 Then
            MyBase.Items.Add(summary)
        Else
            MyBase.Items(0) = summary
        End If
        SelectedIndex = 0
    End Sub

    Private Function AllItemsChecked() As Boolean
        For Each isChecked As Boolean In checkedState
            If Not isChecked Then
                Return False
            End If
        Next
        Return checkedState.Count > 0
    End Function

    Public Class ItemCollection
        Private ReadOnly owner As ComCheckBoxList

        Public Sub New(owner As ComCheckBoxList)
            Me.owner = owner
        End Sub

        Public Sub Add(item As String)
            owner.itemTexts.Add(item)
            owner.checkedState.Add(False)
            owner.UpdateSummary()
        End Sub

        Public Sub Clear()
            owner.itemTexts.Clear()
            owner.checkedState.Clear()
            owner.UpdateSummary()
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return owner.itemTexts.Count
            End Get
        End Property

        Default Public ReadOnly Property Item(index As Integer) As String
            Get
                Return owner.itemTexts(index)
            End Get
        End Property
    End Class
End Class
