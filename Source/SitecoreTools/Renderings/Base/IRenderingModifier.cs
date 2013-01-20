using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sitecore.Data;
using Sitecore.Data.Items;

namespace SitecoreTools.Renderings.Base
{
    public interface IRenderingModifier
    {
        bool IsEditMode { get; }

        void ChangeDatasourceForRendering(Item startItem, ID renderingId, ID datasourceId);
    }
}
