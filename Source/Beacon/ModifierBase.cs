using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace SitecoreTools
{
	public abstract class ModifierBase
	{
		protected ModifierSettings _settings;
		protected Dictionary<ID, ProcessingResult> _processingResults = new Dictionary<ID, ProcessingResult>();
		protected Action<Item> _action;

		public bool IsEditMode
		{
			get
			{
				return _settings.EditMode;
			}
		}

		protected ModifierBase(ModifierSettings settings, Action<Item> action)
		{
			Assert.ArgumentNotNull(settings, "settings");
			_settings = settings;
		}

		internal void ProcessTree(Item item)
		{
			Assert.ArgumentNotNull(item, "item");

			var pr = new ProcessingResult() { Path = item.Paths.ContentPath, Updated = false };
			if (!_processingResults.ContainsKey(item.ID))
			{
				_processingResults.Add(item.ID, pr);
			}

			ProcessItem(item);

			if (_settings.ProcessDescendants)
			{
				ProcessChildren(item);
			}
		}

		internal abstract bool CanProcessItem(Item item);

		private void ProcessItem(Item item)
		{
			Assert.ArgumentNotNull(item, "item");

			if (!CanProcessItem(item))
			{
				return;
			}

			_action(item);
		}

		private void ProcessChildren(Item parent)
		{
			if (parent.Children == null)
			{
				return;
			}

			foreach (Item i in parent.Children)
			{
				ProcessTree(i);
			}
		}

	}
}
