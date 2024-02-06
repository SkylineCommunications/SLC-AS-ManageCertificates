namespace ManageCertificates_1.View
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class TableSelection
	{
		private readonly string[] columns;
		private Dictionary<string, bool> rowStatus;

		public TableSelection(string[] columns)
		{
			this.columns = columns;
		}

		public IEnumerable<string> Selected
		{
			get
			{
				return rowStatus.Where(s => s.Value).Select(s => s.Key);
			}
		}

		public void AddToDialog(Dialog dialog, Dictionary<string, Widget[]> rows, ref int currentRow)
		{
			rowStatus = rows.Keys.ToDictionary(k => k, k => false);
			for (int i = 0; i < columns.Length; i++)
			{
				dialog.AddWidget(new Label(columns[i]), currentRow, i + 1);
			}

			currentRow++;
			foreach (var row in rows)
			{
				var checkbox = new CheckBox();
				checkbox.Tooltip = row.Key;
				checkbox.Changed += OnCheck;
				dialog.AddWidget(checkbox, currentRow, 0);
				for (int i = 0; i < row.Value.Length; i++)
				{
					dialog.AddWidget(row.Value[i], currentRow, i + 1);
				}

				currentRow++;
			}
		}

		private void OnCheck(object sender, EventArgs e)
		{
			var checkbox = sender as CheckBox;
			if (checkbox != null && rowStatus.ContainsKey(checkbox.Tooltip))
			{
				rowStatus[checkbox.Tooltip] = checkbox.IsChecked;
			}
		}
	}
}