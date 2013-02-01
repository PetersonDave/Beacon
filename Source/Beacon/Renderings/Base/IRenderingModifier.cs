using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace SitecoreTools.Renderings.Base
{
	public interface IRenderingModifier
	{
		bool IsEditMode { get; }

		Dictionary<ID, ProcessingResult> ChangeDatasourceForRendering(Item startItem, ID renderingId, ID datasourceId);
	}
}
