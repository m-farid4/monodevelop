﻿//
//
// Author:
//   Jose Medrano
//

//
// Copyright (C) 2918 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if MAC

using System;
using Foundation;
using AppKit;
using ObjCRuntime;
using Gtk;
using System.Collections.Generic;
using Gdk;
using CoreGraphics;

namespace MonoDevelop.Components.Docking
{
	class MacSplitterWidget : NSView, ISplitterWidget
	{
		DockGroup dockGroup;
		int dockIndex;
		Widget parent;
		public MacSplitterWidget (Widget parent)
		{
			this.parent = parent;
		}

		NSTrackingArea trackingArea;

		public Widget Parent => parent;

		public bool Visible {
			get => !Hidden;
			set => Hidden = !value;
		 }

		bool focused;

		public override bool ResignFirstResponder ()
		{
			focused = false;
			NSCursor.ArrowCursor.Set ();
			return base.ResignFirstResponder ();
		}

		public override bool BecomeFirstResponder ()
		{
			focused = true;
			NSCursor.OpenHandCursor.Set ();
			return base.BecomeFirstResponder ();
		}

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null) {
				RemoveTrackingArea (trackingArea);
				trackingArea.Dispose ();
			}
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea (Bounds, options, this, null);
			AddTrackingArea (trackingArea);
		}

		public void Init (DockGroup grp, int index)
		{
			dockGroup = grp;
			dockIndex = index;
		}

		int dragPos;
		int dragSize;

		public override void MouseDown (NSEvent theEvent)
		{
			NSCursor.ClosedHandCursor.Set ();
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			base.MouseMoved (theEvent);
		}

		public override void MouseUp (NSEvent theEvent)
		{
			dragging = false;

			if (focused) {
				NSCursor.OpenHandCursor.Set ();
			}
			base.MouseUp (theEvent);
		}

		CGPoint point;
		bool dragging;
		public override void MouseDragged (NSEvent theEvent)
		{
			dragging = true;
			//point = ConvertPointFromView (theEvent.LocationInWindow, null);
			////moving
			////dragin started
			//dragPos = (dockGroup.Type == DockGroupType.Horizontal) ? (int)point.X : (int)point.Y;
			//var obj = dockGroup.VisibleObjects [dockIndex];
			//dragSize = (dockGroup.Type == DockGroupType.Horizontal) ? obj.Allocation.Width : obj.Allocation.Height;

			base.MouseDragged (theEvent);
		}

		public void SetSize (Rectangle rect)
		{
			Frame = new CGRect (0, 0, rect.Width, rect.Height);
		}

		public void Hide ()
		{
			Hidden = true;
		}

		public void Show ()
		{
			Hidden = false;
		}
	}

}

#endif