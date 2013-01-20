using System;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;

using SitecoreTools.Renderings.DTO;
using SitecoreTools.Renderings.Base;

namespace SitecoreTools.Renderings
{
    public class RenderingModifier : IRenderingModifier
	{
        private RenderingModifierSettings RenderingModifierSettings { get; set; }

		public bool IsEditMode 
        {
            get
            {
                return RenderingModifierSettings.EditMode;
            }
        }

		private readonly Database _db;
		private ID _renderingId;
		private ID _datasourceId;

		private RenderingModifier(bool editMode, string database)
            : this(CreateNewRenderingModifierSettings(editMode, database))
		{
		}

        private RenderingModifier(RenderingModifierSettings renderingModifierSettings)
		{
            Assert.ArgumentNotNull(renderingModifierSettings, "renderingModifierSettings");
            Assert.ArgumentNotNullOrEmpty(renderingModifierSettings.Database, "renderingModifierSettings.Database");
            _db = Database.GetDatabase(renderingModifierSettings.Database);
            Assert.ArgumentCondition(_db != null, "renderingModifierSettings.Database", string.Format("Database {0} is not a valid database name", renderingModifierSettings.Database));
        }

		//var renderingId = new ID("34180A33-145B-47F6-B958-360F32DBFD7D");
		//var newDatasourceId = new ID("3DAAC978-9460-4C8F-86B2-74882CC28A41");
		public void ChangeDatasourceForRendering(Item startItem, ID renderingId, ID datasourceId)
		{
            Assert.ArgumentNotNull(startItem, "startItem");
            Assert.ArgumentCondition(!Sitecore.Data.ID.IsNullOrEmpty(renderingId), "renderingId", "renderingId is not a valid Guid");
            Assert.ArgumentCondition(!Sitecore.Data.ID.IsNullOrEmpty(datasourceId), "datasourceId", "datasourceId is not a valid Guid");
			
            _renderingId = renderingId;
			_datasourceId = datasourceId;

			ProcessTree(startItem);
		}

		private void ProcessTree(Item item)
		{
            Assert.ArgumentNotNull(item, "item");
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

            SetLayoutFieldValueIfNotStandardValue(item);
		}

        private void SetLayoutFieldValueIfNotStandardValue(Item item)
        {
            var layoutField = item.Fields[Sitecore.FieldIDs.LayoutField];
            if (!layoutField.ContainsStandardValue)
            {
                var layoutFieldValue = LayoutField.GetFieldValue(layoutField);
                var theLayout = LayoutDefinition.Parse(layoutFieldValue);
                SetLayoutFieldValueIfValidDevice(theLayout, item);
            }
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

        private static RenderingModifierSettings CreateNewRenderingModifierSettings(bool editMode, string database)
        {
            return new RenderingModifierSettings { EditMode = editMode, Database = database };
        }

        public static IRenderingModifier CreateNewRenderingModifier(bool editMode, string database)
        {
            return new RenderingModifier(editMode, database);
        }

        public static IRenderingModifier CreateNewRenderingModifier(RenderingModifierSettings renderingModifierSettings)
        {
            return new RenderingModifier(renderingModifierSettings);
        }
	}
}