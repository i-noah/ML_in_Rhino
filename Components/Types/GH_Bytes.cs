using Grasshopper.Kernel.Types;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MLR.Components.Types
{
    public class GH_Bytes : GH_Goo<byte[]>
    {
        public GH_Bytes()
            :base((byte[])null)
        { }

        public GH_Bytes(byte[] bytes)
        {
            Value = bytes;
        }

        public GH_Bytes(GH_Bytes other)
            : this(other.Value)
        {
        }

        public override bool IsValid => m_value != null && m_value.Length > 0;

        public override string TypeName => "GH_Bytes Object";

        public override string TypeDescription => "present bytes array, can load online";

        public override bool CastFrom(object source)
        {
            if (source is not GH_String path) return false;

            if (path.Value.StartsWith("http"))
            {
                try
                {
                    var client = new RestClient(path.Value);
                    var request = new RestRequest(Method.GET);
                    var bytes = client.DownloadData(request);

                    m_value = bytes;
                    return true;
                } catch
                {
                    return false;
                }                
            } else if (File.Exists(path.Value))
            {
                try
                {
                    m_value = File.ReadAllBytes(path.Value);
                    return true;
                } catch
                {
                    return false;
                }
            }

            return false;
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(byte[])))
            {
                target = (Q)(object)m_value;
                return true;
            }
            return base.CastTo(ref target);
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_Bytes(this);
        }

        public override string ToString()
        {
            return "GH_Bytes Object";
        }
    }
}
