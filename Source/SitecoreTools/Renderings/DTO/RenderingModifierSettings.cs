using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SitecoreTools.Renderings.DTO
{
    public class RenderingModifierSettings
    {
        public bool EditMode { get; set; }
        public string Database { get; set; }
        public bool ProcessDescendants { get; set; }
    }
}
