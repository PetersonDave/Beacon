using System;
using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace SitecoreTools.Content
{
	public class ContentModifier : ModifierBase
	{
		private ID _templateId;

		public ContentModifier(ModifierSettings contentModifierSettings, Action<Item> action) : base(contentModifierSettings, action) { }

		public Dictionary<ID, ProcessingResult> UpdateFieldsOfTemplateType(Item startItem, ID tempalteId)
		{
			Assert.IsNotNull(startItem, "startItem");
			Assert.IsNotNull(tempalteId, "tempalteId");

			_templateId = tempalteId;

			ProcessTree(startItem);

			return _processingResults;
		}

		internal override bool CanProcessItem(Item item)
		{
			return item.TemplateID == _templateId;
		}
	}
}
