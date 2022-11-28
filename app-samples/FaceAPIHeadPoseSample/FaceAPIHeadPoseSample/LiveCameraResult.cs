using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FaceAPIHeadPoseSample
{
    public class LiveCameraResult
    {
        public DetectedFace[] Faces { get; set; } = null;
    }
}
