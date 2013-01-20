using System;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;

namespace SitecoreTools.Renderings
{
	public class RenderingModifier
	{
		public bool IsEditMode { get; private set; }
		private readonly Database _db;
		private ID _renderingId;
		private ID _datasourceId;

		public RenderingModifier(bool editMode, string database)
		{
			bool isDatabaseValid = !string.IsNullOrEmpty(database);
			if (!isDatabaseValid)
			{
				throw new ArgumentNullException(database);
			}

			IsEditMode = editMode;

			_db = Database.GetDatabase(database);
			if (_db == null)
			{
				throw new ArgumentException(string.Format("Database {0} is not a valid database name", database));
			}
		}

		//var renderingId = new ID("34180A33-145B-47F6-B958-360F32DBFD7D");
		//var newDatasourceId = new ID("3DAAC978-9460-4C8F-86B2-74882CC28A41");
		public void ChangeDatasourceForRendering(Item startItem, ID renderingId, ID datasourceId)
		{
			bool isStartItemValid = startItem != null;
			if (!isStartItemValid)
			{
				throw new ArgumentNullException("startItem");
			}
	
			bool isRenderingValid = !Sitecore.Data.ID.IsNullOrEmpty(renderingId);
			if (!isRenderingValid)
			{
				throw new ArgumentException("renderingId is not a valid Guid");
			}

			bool isDatasourceIdValid = !Sitecore.Data.ID.IsNullOrEmpty(datasourceId);
			if (!isDatasourceIdValid)
			{
				throw new ArgumentException("renderingId is not a valid Guid");
			}

			_renderingId = renderingId;
			_datasourceId = datasourceId;

			ProcessTree(startItem);
		}

		private void ProcessTree(Item item)
		{
			bool isItemValid = item != null;
			if (!isItemValid)
			{
				throw new ArgumentNullException("item");
			}

			ProcessItem(item);

			foreach (Item i in item.Children)
			{
				ProcessTree(i);
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
				var layoutFieldValue = LayoutField.GetFieldValue(layoutField);
				var theLayout = LayoutDefinition.Parse(layoutFieldValue);

				bool isDeviceValid = theLayout != null && theLayout.Devices != null && theLayout.Devices.Count > 0;
				if (isDeviceValid)
				{
					var renderings = (theLayout.Devices[0] as DeviceDefinition).Renderings;

					foreach (RenderingDefinition rendering in renderings)
					{
						if (rendering.ItemID == _renderingId.ToString())
						{
							using (new Sitecore.Data.Items.EditContext(item))
							{
								if (IsEditMode)
								{
									rendering.Datasource = _datasourceId.ToString();

									item[Sitecore.FieldIDs.LayoutField] = theLayout.ToXml();
								}
							}
						}
					}										
				}
			}
		}
	}
}