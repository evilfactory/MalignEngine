using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public interface ITextureResource : IGpuResource
{
    int Width { get; }
    int Height { get; }
    void Resize(int width, int height);
    void SubmitData(Color[] data);
    void SubmitData(System.Drawing.Rectangle bounds, Color[] data);
}