
using MonoDevelop.Components.Commands;
using MonoDevelop.Projects;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.Views;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace MonoDevelop.VersionControl
{
	public enum Commands {
		Update,
		Diff,
		Log,
		Status,
		SolutionStatus,
		Add,
		Remove,
		Publish,
		Checkout,
		Repository,
		Revert,
		Lock,
		Unlock,
		Annotate,
		ShowAnnotations,
		HideAnnotations,
		CreatePatch,
		Ignore,
		Unignore,
		ResolveConflicts
	}
	
	abstract class SolutionVersionControlCommandHandler : CommandHandler
	{
		static VersionControlItemList GetItems ()
		{
			VersionControlItemList list = new VersionControlItemList ();
			
			WorkspaceObject wob;
			Repository repo = null;
			wob = IdeApp.ProjectOperations.CurrentSelectedWorkspaceItem;
			if (wob != null)
				repo = VersionControlService.GetRepository (wob);
			if (repo == null) {
				wob = IdeApp.ProjectOperations.CurrentSelectedSolutionItem;
				if (wob != null)
					repo = VersionControlService.GetRepository (wob);
			}
			if (repo == null || repo.VersionControlSystem == null || !repo.VersionControlSystem.IsInstalled)
				return list;

			list.Add (new VersionControlItem (repo, wob, wob.BaseDirectory, true, null));
			return list;
		}

		protected override void Run ()
		{
			VersionControlItemList items = GetItems ();
			RunCommandAsync (items, false, default).Ignore ();
		}

		protected override async Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			if (VersionControlService.IsGloballyDisabled) {
				info.Visible = false;
				return;
			}

			VersionControlItemList items = GetItems ();
			info.Enabled = items.Count > 0 && await RunCommandAsync (items, true, cancelToken);
		}

		protected abstract Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken);
	}

	class FileVersionControlCommandHandler: CommandHandler
	{
		protected static VersionControlItemList GetItems ()
		{
			VersionControlItemList list = new VersionControlItemList ();
			VersionControlItem it = GetItem ();

			if (it != null)
				list.Add (it);
			return list;
		}
		
		protected static VersionControlItem GetItem ()
		{
			Document doc = IdeApp.Workbench.ActiveDocument;
			if (doc == null || !doc.IsFile)
				return null;
			
			Project project = doc.Project ?? IdeApp.ProjectOperations.CurrentSelectedProject;
			if (project == null)
				return null;
			
			Repository repo = VersionControlService.GetRepository (project);
			if (repo == null || repo.VersionControlSystem == null || !repo.VersionControlSystem.IsInstalled)
				return null;
			
			return new VersionControlItem (repo, project, doc.FileName, false, null);
		}

		protected sealed override void Run ()
		{
			VersionControlItemList items = GetItems ();
			RunCommandAsync (items, false, default);
		}

		protected override async Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			if (VersionControlService.IsGloballyDisabled) {
				info.Visible = false;
				return;
			}

			VersionControlItemList items = GetItems ();
			info.Enabled = items.Count > 0 && await RunCommandAsync (items, true, cancelToken);
		}
		
		protected virtual Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return Task.FromResult (true);
		}
	}	

	class UpdateCommandHandler: SolutionVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return UpdateCommand.UpdateAsync (items, test, cancellationToken);
		}
	}
	
	class StatusCommandHandler: SolutionVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return StatusView.ShowAsync (items, test, true);
		}
	}

	class AddCommandHandler: FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return AddCommand.AddAsync (items, test, cancellationToken);
		}

		protected override Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			info.Text = GettextCatalog.GetString ("Add File");
			return base.UpdateAsync (info, cancelToken);
		}
	}
	
	class RemoveCommandHandler: FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return RemoveCommand.RemoveAsync (items, test, cancellationToken);
		}

		protected override Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			info.Text = GettextCatalog.GetString ("Remove File");
			return base.UpdateAsync (info, cancelToken);
		}
	}
	
	class RevertCommandHandler: FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return RevertCommand.RevertAsync (items, test, cancellationToken);
		}

		protected override Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			info.Text = GettextCatalog.GetString ("Revert File");
			return base.UpdateAsync (info, cancelToken);
		}
	}
	
	class LockCommandHandler: FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return LockCommand.LockAsync (items, test, cancellationToken);
		}

		protected override Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			info.Text = GettextCatalog.GetString ("Lock File");
			return base.UpdateAsync (info, cancelToken);
		}
	}
	
	class UnlockCommandHandler: FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return UnlockCommand.UnlockAsync (items, test, cancellationToken);
		}

		protected override Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			info.Text = GettextCatalog.GetString ("Unlock File");
			return base.UpdateAsync (info, cancelToken);
		}
	}

	class IgnoreCommandHandler : FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return IgnoreCommand.IgnoreAsync (items, test, cancellationToken);
		}

		protected override Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			info.Text = GettextCatalog.GetString ("Add to ignore list");
			return base.UpdateAsync (info, cancelToken);
		}
	}

	class UnignoreCommandHandler : FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			return UnignoreCommand.UnignoreAsync (items, test, cancellationToken);
		}

		protected override Task UpdateAsync (CommandInfo info, CancellationToken cancelToken)
		{
			info.Text = GettextCatalog.GetString ("Remove from ignore list");
			return base.UpdateAsync (info, cancelToken);
		}
	}

	class CurrentFileViewHandler<T> : FileVersionControlCommandHandler
	{
		protected override Task<bool> RunCommandAsync (VersionControlItemList items, bool test, CancellationToken cancellationToken)
		{
			if (test)
				return Task.FromResult (true);

			var window = IdeApp.Workbench.ActiveDocument.Window;
			window.SwitchView (window.FindView<T> ());
			return Task.FromResult (true);
		}
	}

	class CurrentFileDiffHandler : CurrentFileViewHandler<IDiffView>
	{
	}
	
	class CurrentFileBlameHandler : CurrentFileViewHandler<IBlameView>
	{
	}
	
	class CurrentFileLogHandler : CurrentFileViewHandler<ILogView>
	{
	}
}
