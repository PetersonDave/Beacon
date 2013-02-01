using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Links;
using SitecoreTools.Renderings.Base;

namespace SitecoreTools.Renderings
{
	public class RenderingModifier : IRenderingModifier
	{
		private ModifierSettings RenderingModifierSettings { get; set; }
		public bool IsEditMode 
		{
			get
			{
				return RenderingModifierSettings.EditMode;
			}
		}

		private ID _renderingId;
		private ID _datasourceId;
		private Dictionary<ID, ProcessingResult> _processingResults = new Dictionary<ID, ProcessingResult>();

		public RenderingModifier(ModifierSettings renderingModifierSettings)
		{
			Assert.ArgumentNotNull(renderingModifierSettings, "renderingModifierSettings");
			RenderingModifierSettings = renderingModifierSettings;
		}

		public Dictionary<ID, ProcessingResult> ChangeDatasourceForRendering(Item startItem, ID renderingId, ID datasourceId)
		{
			Assert.ArgumentNotNull(startItem, "startItem");
			Assert.ArgumentCondition(!Sitecore.Data.ID.IsNullOrEmpty(renderingId), "renderingId", "renderingId is not a valid Guid");
			Assert.ArgumentCondition(!Sitecore.Data.ID.IsNullOrEmpty(datasourceId), "datasourceId", "datasourceId is not a valid Guid");
			
			_renderingId = renderingId;
			_datasourceId = datasourceId;

			ProcessTree(startItem);

			return _processingResults;
		}

		private void ProcessTree(Item item)
		{
			Assert.ArgumentNotNull(item, "item");

			var pr = new ProcessingResult() {Path = item.Paths.ContentPath, Updated = false};
			if (!_processingResults.ContainsKey(item.ID))
			{
				_processingResults.Add(item.ID, pr);
			}
			
			ProcessItem(item);

			if (RenderingModifierSettings.ProcessDescendants)
			{
				ProcessChildren(item);
			}
		}

		private void ProcessItem(Item item)
		{
			bool isLayoutFieldValid = !string.IsNullOrEmpty(item[Sitecore.FieldIDs.LayoutField]);
			if (!isLayoutFieldValid)
			{
				return;
			}

			var layoutField = item.Fields[Sitecore.FieldIDs.LayoutField];
			if (!layoutField.ContainsStandardValue)
			{
				var layoutFieldValue = GetLayoutFieldValue(layoutField);
				var theLayout = GetLayoutDefinition(layoutFieldValue);
				SetLayoutFieldValueIfValidDevice(theLayout, item);
			}
		}

		public virtual string GetLayoutFieldValue(Field layoutField)
		{
			return LayoutField.GetFieldValue(layoutField);
		}

		public virtual LayoutDefinition GetLayoutDefinition(string layoutFieldValue)
		{
			return LayoutDefinition.Parse(layoutFieldValue);
		}

		private void SetLayoutFieldValueIfValidDevice(LayoutDefinition layoutDefinition, Item item)
		{
			bool isDeviceValid = layoutDefinition != null && layoutDefinition.Devices != null && layoutDefinition.Devices.Count > 0;
			if (isDeviceValid)
			{
				var renderings = (layoutDefinition.Devices[0] as DeviceDefinition).Renderings;

				foreach (RenderingDefinition rendering in renderings)
				{
					SetLayoutFieldValueIfApplicable(rendering, layoutDefinition, item);
				}
			}
		}

		private void SetLayoutFieldValueIfApplicable(RenderingDefinition renderingDefinition, LayoutDefinition layoutDefinition, Item item)
		{
			if (renderingDefinition.ItemID == _renderingId.ToString())
			{
				SetLayoutFieldValueIfEditMode(renderingDefinition, layoutDefinition, item);
			}
		}

		private void SetLayoutFieldValueIfEditMode(RenderingDefinition renderingDefinition, LayoutDefinition layoutDefinition, Item item)
		{
			if (_processingResults.ContainsKey(item.ID))
			{
				_processingResults[item.ID].Updated = true;	
			}
			
			if (IsEditMode)
			{
				SetLayoutFieldValue(renderingDefinition, layoutDefinition, item);
			}
		}

		private void SetLayoutFieldValue(RenderingDefinition renderingDefinition, LayoutDefinition layoutDefinition, Item item)
		{
			Assert.ArgumentNotNull(item, "item");

			using (new Sitecore.Data.Items.EditContext(item))
			{
				renderingDefinition.Datasource = _datasourceId.ToString();
				item[Sitecore.FieldIDs.LayoutField] = layoutDefinition.ToXml();
			}
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

		public static IRenderingModifier CreateNewRenderingModifier(ModifierSettings renderingModifierSettings)
		{
			return new RenderingModifier(renderingModifierSettings);
		}
	}
}