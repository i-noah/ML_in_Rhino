using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLR.Components.Common
{
    public class InferenceSessionCache
    {
        public static readonly InferenceSessionCache Instance = new();

        private readonly Dictionary<string, InferenceSession> m_cache;

        public InferenceSessionCache()
        {
            m_cache = new Dictionary<string, InferenceSession>();
        }

        public InferenceSession LoadInferenceSessionModel(string filename)
        {
            if (m_cache.ContainsKey(filename)) return m_cache[filename];

            var session = new InferenceSession(filename);
            m_cache[filename] = session;

            return session;
        }

        public void Dispose()
        {
            foreach(var session in m_cache.Values)
            {
                session.Dispose();
            }
        }
    }
}
