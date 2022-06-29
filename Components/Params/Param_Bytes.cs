using Grasshopper.Kernel;
using MLR.Components.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLR.Components.Params
{
    public class Param_Bytes : GH_Param<GH_Bytes>
    {
        public Param_Bytes()
            : base(new GH_InstanceDescription("GH_Bytes Object", "字节数组", "", "Params", "Input"))
        { }
        public override Guid ComponentGuid => new("DD6DBD94-77DA-426D-95F2-4A3911990767");

        public override GH_Exposure Exposure => GH_Exposure.hidden;
    }
}
