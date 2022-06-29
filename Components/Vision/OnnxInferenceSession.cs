using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Parameters;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using MLR.Components.Common;
using System.Windows.Forms;
using GH_IO.Serialization;
using MLR.Components.Params;

namespace MLR.Components.Vision
{
    public class OnnxInferenceSession : GH_Component
    {        
        public OnnxInferenceSession()
            : base("Onnx Inference Session", "OIS", "Onnx Inference Session", "ML", "Vision")
        { }

        public override Guid ComponentGuid => new("D0E4ECCA-BD24-435A-ACBD-0E66907DE72F");
        private string m_filename = null;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            var imageFilenameParam = new Param_Bytes
            {
                NickName = "P",
                Optional = true,
            };

            pManager.AddParameter(imageFilenameParam);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("C", "C", "", GH_ParamAccess.list);
        }
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Load Model", Menu_Clicked, enabled: true);
        }
        private void Menu_Clicked(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Multiselect = false
            };

            DialogResult res = ofd.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                m_filename = ofd.FileName;
                InferenceSessionCache.Instance.LoadInferenceSessionModel(m_filename);
                ExpireSolution(true);
            }
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (m_filename == null)
            {
                Message = "No Model";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "right click to load onnx model");
                return;
            }

            if (!File.Exists(m_filename))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{m_filename} is missing!");
                return;
            }

            Message = Path.GetFileNameWithoutExtension(m_filename);

            byte[] bytes = null;

            if (!DA.GetData(0, ref bytes)) return;

            var session = InferenceSessionCache.Instance.LoadInferenceSessionModel(m_filename);

            using Image<Rgb24> image = Image.Load<Rgb24>(bytes, out IImageFormat format);
            
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(224, 224),
                    Mode = ResizeMode.Crop
                });
            });

            Tensor<float> input = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var stddev = new[] { 0.229f, 0.224f, 0.225f };
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        input[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stddev[0];
                        input[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stddev[1];
                        input[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stddev[2];
                    }
                }
            });

            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("modelInput", input) };

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
            
            IEnumerable<float> output = results.First().AsEnumerable<float>();
            float sum = output.Sum(x => (float)Math.Exp(x));
            IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / sum);

            DA.SetDataList(0, softmax);
        }

        public override bool Read(GH_IReader reader)
        {
            if (!reader.TryGetString("model_filename", ref m_filename))
            {
                return false;
            }
            if (File.Exists(m_filename))
            {
                InferenceSessionCache.Instance.LoadInferenceSessionModel(m_filename);
            }
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("model_filename", m_filename);
            return base.Write(writer);
        }
    }
}
