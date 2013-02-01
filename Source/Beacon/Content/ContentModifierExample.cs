using System;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace SitecoreTools.Content
{
	public class ContentModifierExample
	{
		private const string _FieldName = "field";
		private const string _ContentPathOriginal = "/sitecore/content/original";
		private const string _ContentPathNew = "/sitecore/content/new";

		public void Example()
		{
			var cms = new ModifierSettings() {EditMode = false, ProcessDescendants = true};
			var action = new Action<Item>(UpdateItem);

			var cm = new ContentModifier(cms, action);
		}

		public void UpdateItem(Item item)
		{
			Assert.ArgumentNotNull(item, "item");

			MultilistField field = item.Fields[_FieldName];
			if (field != null)
			{
				foreach (ID id in field.TargetIDs)
				{
					Item targetItem = Sitecore.Context.Database.Items[id];
					if (targetItem != null)
					{
						string path = targetItem.Paths.ContentPath;
						string newPath = path.Replace(_ContentPathOriginal, _ContentPathNew);

						if (!string.IsNullOrEmpty(newPath))
						{
							var relatedItem = Sitecore.Context.Database.Items[newPath];
							if (relatedItem != null)
							{
								SwapMultilistFieldValue(item, field, targetItem.ID, relatedItem.ID);
							}
						}
					}
				}
			}
		}

		private void SwapMultilistFieldValue(Item item, MultilistField field, ID targetItemId, ID newItemId)
		{
			Assert.ArgumentNotNull(item, "item");
			Assert.ArgumentNotNull(field, "field");
			Assert.ArgumentNotNull(targetItemId, "targetItemId");
			Assert.ArgumentNotNull(newItemId, "newItemId");

			using (new Sitecore.Data.Items.EditContext(item))
			{
				field.Remove(targetItemId.ToString());
				field.Add(newItemId.ToString());
			}
		}
	}
}