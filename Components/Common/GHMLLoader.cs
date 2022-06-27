using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLR.Components.Common
{
    public class MLRLoader : Grasshopper.Kernel.GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Rhino.RhinoApp.Closing += RhinoApp_Closing;
            return GH_LoadingInstruction.Proceed;
        }

        private void RhinoApp_Closing(object sender, EventArgs e)
        {
            InferenceSessionCache.Instance.Dispose();
        }
    }
}
