using System;
using System.IO;
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using System.Linq;
using System.Threading.Tasks;

namespace MonoDevelop.VersionControl.Views
{
	public interface ILogView
	{
	}
	
	class LogView : BaseView, ILogView
	{
		LogWidget widget;
		VersionInfo vinfo;
		
		public LogWidget LogWidget {
			get {
				return widget;
			}
		}

		VersionControlDocumentInfo info;
		public LogView (VersionControlDocumentInfo info) : base (GettextCatalog.GetString ("Log"), GettextCatalog.GetString ("Shows the source control log for the current file"))
		{
			this.info = info;
		}
		
		async void CreateControlFromInfo ()
		{
			try {
				var lw = new LogWidget (info);

				widget = lw;
				info.Updated += OnInfoUpdated;
				lw.History = this.info.History;
				vinfo = await this.info.Item.GetVersionInfoAsync ();

				if (WorkbenchWindow != null)
					widget.SetToolbar (WorkbenchWindow.GetToolbar (this));
			} catch (Exception e) {
				LoggingService.LogInternalError (e);
			}
		}

		async void OnInfoUpdated (object sender, EventArgs e)
		{
			try {
				widget.History = this.info.History;
				vinfo = await info.Item.GetVersionInfoAsync ();
			} catch (Exception ex) {
				LoggingService.LogInternalError (ex);
			}
		}

		[Obsolete]
		public LogView (string filepath, bool isDirectory, Revision [] history, Repository vc) 
			: base (Path.GetFileName (filepath) + " Log")
		{
			Task.Run (async () => {
				try {
					this.vinfo = await vc.GetVersionInfoAsync (filepath, VersionInfoQueryFlags.IgnoreCache);
				} catch (Exception ex) {
					MessageService.ShowError (GettextCatalog.GetString ("Version control command failed."), ex);
				}
			});
			
			// Widget setup
			VersionControlDocumentInfo info  =new VersionControlDocumentInfo (null, null, vc);
			info.History = history;
			info.Item.VersionInfo = vinfo;
			var lw = new LogWidget (info);
			
			widget = lw;
			lw.History = history;
		}

		
		public override Control Control { 
			get {
				if (widget == null)
					CreateControlFromInfo ();
				return widget; 
			}
		}

		protected override void OnWorkbenchWindowChanged ()
		{
			base.OnWorkbenchWindowChanged ();
			if (WorkbenchWindow != null && widget != null)
				widget.SetToolbar (WorkbenchWindow.GetToolbar (this));
		}
		
		public override void Dispose ()
		{
			if (widget != null) {
				widget.Destroy ();
				widget = null;
			}
			if (info != null) {
				info.Updated -= OnInfoUpdated;
				info = null;
			}
			base.Dispose ();
		}

		public void Init ()
		{
			if (info != null && !info.Started) {
				widget.ShowLoading ();
				info.Start ();
			}
		}

		protected override void OnSelected ()
		{
			Init ();
		}

		[CommandHandler (MonoDevelop.Ide.Commands.EditCommands.Copy)]
		protected void OnCopy ()
		{
			string data = widget.GetSelectedText ();
			if (data == null) {
				return;
			}

			CopyToClipboard (data);
		}

		internal static void CopyToClipboard (string data)
		{
			var clipboard = Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));
			clipboard.Text = data;
			clipboard = Clipboard.Get (Gdk.Atom.Intern ("PRIMARY", false));
			clipboard.Text = data;
		}
	}

}
