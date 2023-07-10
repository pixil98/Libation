﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using LibationFileManager;

namespace LibationWinForms
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			this.RestoreSizeAndLocation(Configuration.Instance);
			this.FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);

			// this looks like a perfect opportunity to refactor per below.
			// since this loses design-time tooling and internal access, for now I'm opting for partial classes
			//   var modules = new ConfigurableModuleBase[]
			//   {
			//       new PictureStorageModule(),
			//       new BackupCountsModule(),
			//       new VisibleBooksModule(),
			//       // ...
			//   };
			//   foreach(ConfigurableModuleBase m in modules)
			//       m.Configure(this);

			// these should do nothing interesting yet (storing simple var, subscribe to events) and should never rely on each other for order.
			// otherwise, order could be an issue.
			// eg: if one of these init'd productsGrid, then another can't reliably subscribe to it
			Configure_BackupCounts();
			Configure_ScanAuto();
			Configure_ScanNotification();
			Configure_VisibleBooks();
			Configure_QuickFilters();
			Configure_ScanManual();
			Configure_RemoveBooks();
			Configure_Liberate();
			Configure_Export();
			Configure_Settings();
			Configure_ProcessQueue();
			Configure_Filter();
			Configure_Upgrade();
			// misc which belongs in winforms app but doesn't have a UI element
			Configure_NonUI();

			// Configure_Grid(); // since it's just this, can keep here. If it needs more, then give grid it's own 'partial class Form1'
			{
				LibraryCommands.LibrarySizeChanged += (_, __) => Invoke(() => productsDisplay.DisplayAsync());
			}
			Shown += Form1_Shown;
		}

		private async void Form1_Shown(object sender, EventArgs e)
		{
			if (Configuration.Instance.FirstLaunch)
			{
				var result = MessageBox.Show(this, "Would you like a guided tour to get started?", "Libation Walkthrough", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

				if (result is DialogResult.Yes)
				{
					await new Walkthrough(this).RunAsync();
				}

				Configuration.Instance.FirstLaunch = false;
			}
		}

		public async Task InitLibraryAsync(List<LibraryBook> libraryBooks)
		{
			runBackupCountsAgain = true;
			updateCountsBw.RunWorkerAsync(libraryBooks.Where(b => !b.Book.IsEpisodeParent()));
			await productsDisplay.DisplayAsync(libraryBooks);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;
			// I'm leaving this empty call here as a reminder that if we use this, it should probably be after DesignMode check
		}
	}
}
