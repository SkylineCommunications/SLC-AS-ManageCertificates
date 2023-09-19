namespace ManageCertificates_1.View
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class TableSelection : Section
	{
		private readonly IEngine engine;
		private readonly string[] columns;
		private Dictionary<string, bool> rowStatus;

		public TableSelection(IEngine engine, string[] columns)
		{
			this.engine = engine;
			this.columns = columns;
		}

		public IEnumerable<string> Selected
		{
			get
			{
				return rowStatus.Where(s => s.Value).Select(s => s.Key);
			}
		}

		public void Initialize(Dictionary<string, Widget[]> rows)
		{
			rowStatus = rows.Keys.ToDictionary(k => k, k => false);
			var rowcount = 0;
			for (int i = 0; i < columns.Length; i++)
			{
				AddWidget(new Label(columns[i]), rowcount, i + 1);
			}

			rowcount++;
			foreach (var row in rows)
			{
				var checkbox = new CheckBox();
				checkbox.Tooltip = row.Key;
				checkbox.Changed += OnCheck;
				AddWidget(checkbox, rowcount, 0);
				for (int i = 0; i < row.Value.Length; i++)
				{
					AddWidget(row.Value[i], rowcount, i + 1);
				}

				rowcount++;
			}
		}

		private void OnCheck(object sender, EventArgs e)
		{
			engine.GenerateInformation("Triggered CheckBox");
			var checkbox = sender as CheckBox;
			if (checkbox != null)
			{
				engine.GenerateInformation($"CheckBox not null");
			}

			if (checkbox != null && rowStatus.ContainsKey(checkbox.Tooltip))
			{
				engine.GenerateInformation($"CheckBox: {checkbox.Tooltip}");
				rowStatus[checkbox.Tooltip] = checkbox.IsChecked;
			}
		}
	}
}